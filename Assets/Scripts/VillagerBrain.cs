using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steering;
using Behaviour = Steering.Behaviour;

public enum VillagerStatusEnum 
{ 
    gathering, 
    goingToResource,
    resting,
    returningToBase,
    lookingForResources,
    enemiesInRange
}
[System.Serializable] //To show struct in inspector 
public struct ResourceNode
{
    public ResourceNode(IResource resource, float timeSeen, bool harvestable)
    {
        Resource = resource;
        LastSeen = timeSeen;
        Harvestable = harvestable;
    }
    public IResource Resource;
    public float LastSeen;
    public bool Harvestable;
}

[RequireComponent(typeof(Steering.Steering))]
public class VillagerBrain : MonoBehaviour, IGather
{
    public Village m_homeVillage;

    public VillagerSettings m_settings;

    public VillagerStatusEnum m_villagerStatus;

    public Steering.Steering m_steerings;


    private Coroutine gatherCoroutine = null;

    [SerializeField] private GatherTask currentTask;

    [SerializeField] private List<ResourceNode> resourceNodes;
    [SerializeField] private ResourceNode targetNode;
    private ResourceTypeEnum targetNodeType;

    [SerializeField] private List<GameObject> enemiesInRange;
    private List<GameObject> avoidingEnemies;

    [SerializeField] private Transform restPlaceTransform;
    private Vector3 villageRestPlace;


    [SerializeField] private int woodInInv;
    [SerializeField] private int stoneInInv;
    [SerializeField] private int foodInInv;
    
    void Awake()
    {
        resourceNodes = new List<ResourceNode>();
        enemiesInRange = new List<GameObject>();
        m_steerings = GetComponent<Steering.Steering>();
        villageRestPlace = restPlaceTransform.position;
        avoidingEnemies = new List<GameObject>();
    }

    private void Start()
    {
        m_homeVillage.AddVillagerToVillage(gameObject); //Assign to villager to his village
        currentTask = m_homeVillage.GetGatheringTask(m_settings.m_inventorySize); //Get a task
        //Set our target node and act accordingly (Most likely wont have any resource nodes but
        //incase a villager starts with some nodes assigned!
        SetTargetNode();
        if (targetNode.Equals(default(ResourceNode)))
        {
            //If target node is "empty" (default settings) look for resources 
            LookForResources();
        }
        else
        {
            //Go to the target node to gather
            GoToResource();
        }

    }
    private void Update()
    {
        VisionRangeCheck();
        StatusUpdate();
    }
    public void ReturnResource(int amount, ResourceTypeEnum type)
    {
        //Check if it all fits in inventory
        if (amount > SpaceInv())
        {
            amount = SpaceInv();
        }

        //Add items to inventory and update our task
        switch (type)
        {
            case ResourceTypeEnum.food:
                foodInInv += amount;
                currentTask.FoodToGet -= amount;
                if (currentTask.FoodToGet <= 0)
                {
                    SetTargetNode();
                }
                break;
            case ResourceTypeEnum.stone:
                stoneInInv += amount;
                currentTask.StoneToGet -= amount;
                if (currentTask.StoneToGet <= 0)
                {
                    SetTargetNode();
                }
                break;
            case ResourceTypeEnum.wood:
                woodInInv += amount;
                currentTask.WoodToGet--;
                if (currentTask.WoodToGet <= 0)
                {
                    SetTargetNode();
                }
                break;
            default:
                Debug.LogError("Resource type not implemented! " + type);
                break;
        }
    
        //Clear gather coroutine
        gatherCoroutine = null;
    }

    public void TurnAround()
    {
        // Turn around and continue wandering (Villagers cannot go out of bounds otherwise)
        List<IBehaviour> behaviours = new List<IBehaviour>()
        {new Wander(Vector3.zero,transform), new AvoidObstacle()};
        m_steerings.SetBehaviours(behaviours);
    }

    private void StatusUpdate()
    {
        switch (m_villagerStatus)
        {
            case VillagerStatusEnum.enemiesInRange:
                //Check if there are still enemies in range
                if (enemiesInRange.Count == 0)
                {
                    //If not continue with the task
                    switch (currentTask.CurrentState)
                    {
                        case VillagerStatusEnum.goingToResource:
                            GoToResource();
                            break;
                        case VillagerStatusEnum.lookingForResources:
                            LookForResources();
                            break;
                        case VillagerStatusEnum.returningToBase:
                            GoToBase();
                            break;
                        default:
                            //Just incase something goes wrong default going back to base
                            GoToBase();
                            break;
                    }
                }
                else
                {
                    UpdateEnemyAvoiding();
                }
                break;
            case VillagerStatusEnum.gathering:
                //Check if there is space in inventory
                if (SpaceInv() <= 0)
                {
                    GoToBase();
                }

                if (targetNode.Resource == null)
                {
                    FindClosestNode(targetNodeType);
                    break;
                }

                //Check if in range of the target node
                else if (Vector3.Distance(transform.position,targetNode.Resource.gameObject.transform.position) 
                    <= m_settings.m_gatherRange)
                {
                    //Make sure the node is harvestable
                    if (targetNode.Resource.AbleToBeHarvested())
                    {
                        if (gatherCoroutine == null)
                        {
                            gatherCoroutine = targetNode.Resource.GatherResource(gameObject);
                        }
                    }
                    else
                    {
                        //Update the target node and find a new one
                        UpdateResourceNode(targetNode);
                        FindClosestNode(targetNodeType);
                    }
                }
                //If for some reason we are not in range walk to the resource instead
                else if (Vector3.Distance(transform.position, targetNode.Resource.gameObject.transform.position)
                    >= m_settings.m_gatherRange)
                {
                    GoToResource();
                }
                break;
            case VillagerStatusEnum.lookingForResources:
                //Check if target node isn't default values
                if (!targetNode.Equals(default(ResourceNode)))
                {
                    GoToResource();
                }
                break;
            case VillagerStatusEnum.returningToBase:
                if (Vector3.Distance(transform.position, villageRestPlace) <= Random.Range(0, 5))
                {
                    ToRest();
                }
                break;
            case VillagerStatusEnum.goingToResource:
                if (Vector3.Distance(transform.position, targetNode.Resource.gameObject.transform.position)
                    <= m_settings.m_gatherRange)
                {
                    ToHarvest();
                }
                break;
        }

        //No matter the current state always check for enemies in range to avoid
        if (enemiesInRange.Count > 0 && 
            m_villagerStatus != VillagerStatusEnum.enemiesInRange)
        {
            AvoidEnemies();
        }
    }

    private void VisionRangeCheck()
    {
        Collider[] collInRange = Physics.OverlapSphere(transform.position, m_settings.m_visionRange);
        List<GameObject> _enemiesInRange = new List<GameObject>();

        foreach (Collider collider in collInRange) 
        {
            //Check if item is a resource node
            if (collider.GetComponent<IResource>() != null)
            {
                //Check if item already is in list if not add it
                if (CheckIfNodeIsInList(collider.gameObject))
                {
                    UpdateResourceNode(collider.gameObject);
                }
                else
                {
                    AddNodeToList(collider.gameObject);
                }
            }
            else if (collider.GetComponent<IEnemy>() != null)
            {
                _enemiesInRange.Add(collider.gameObject);
            }
        }

        enemiesInRange = _enemiesInRange;
        //Check for new closest node and update it if it changed and check if the node isn't default values
        ResourceNode closestTargetNode = FindClosestNode(targetNodeType);
        if (!closestTargetNode.Equals(default(ResourceNode)))
        {
            //Check if our target node isn't a default one as well
            if (!targetNode.Equals(default(ResourceNode)))
            {
                if (targetNode.Resource.gameObject != closestTargetNode.Resource.gameObject)
                {
                    targetNode = closestTargetNode;
                    if (m_villagerStatus == VillagerStatusEnum.goingToResource ||
                        m_villagerStatus == VillagerStatusEnum.lookingForResources)
                    {
                        //Update the movement to new target
                        GoToResource();
                    }
                }
            }
            else
            {
                targetNode = closestTargetNode;
                if (m_villagerStatus == VillagerStatusEnum.goingToResource || 
                    m_villagerStatus == VillagerStatusEnum.lookingForResources)
                {
                    //Update the movement to new target
                    GoToResource();
                }
            }
        }
    }

    private void GoToResource()
    {
        //Set enum status
        m_villagerStatus = VillagerStatusEnum.goingToResource;
        currentTask.CurrentState = VillagerStatusEnum.goingToResource;

        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        if (targetNode.Equals(targetNode.Equals(default(ResourceNode))))
        {
            SetTargetNode();
        }

        //SetBehaviours
        List<IBehaviour> behaviours = new List<IBehaviour>() 
        {new SeekMovement(targetNode.Resource.gameObject), new AvoidObstacle()};
        m_steerings.SetBehaviours(behaviours);        
    }

    private void ToHarvest()
    {
        //Set enum
        m_villagerStatus = VillagerStatusEnum.gathering;

        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        if (targetNode.Equals(targetNode.Equals(default(ResourceNode))))
        {
            SetTargetNode();
        }

        //Set behaviours
        List<IBehaviour> behaviours = new List<IBehaviour>() { new Idle() };
        m_steerings.SetBehaviours(behaviours);
    }

    private void ToRest()
    {
        //Set enum 
        m_villagerStatus = VillagerStatusEnum.resting;

        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        //Set behaviours
        List<IBehaviour> behaviours = new List<IBehaviour>() { new Idle() };
        m_steerings.SetBehaviours(behaviours);

        StartCoroutine(Resting());
    }

    private IEnumerator Resting()
    {
        //Call the village resting function, clear inv and wait the resting time
        m_homeVillage.VillagerResting(currentTask, foodInInv, woodInInv, stoneInInv);
        foodInInv = 0;
        woodInInv = 0;
        stoneInInv = 0;
        yield return new WaitForSeconds(m_settings.m_restTime);
        //Get new task and update the node for it
        currentTask = m_homeVillage.GetGatheringTask(m_settings.m_inventorySize);
        SetTargetNode();
        if (targetNode.Equals(default(ResourceNode)))
        {
            //If target node is "empty" (default settings) look for resources 
            LookForResources();
        }
        else
        {
            //Go to the target node to gather
            GoToResource();
        }
    }

    

    private void GoToBase()
    {
        //Set enum 
        m_villagerStatus = VillagerStatusEnum.returningToBase;
        currentTask.CurrentState = VillagerStatusEnum.returningToBase;
        
        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        //SetBehaviours
        List<IBehaviour> behaviours = new List<IBehaviour>()
        {new SeekMovement(villageRestPlace), new AvoidObstacle()};
        m_steerings.SetBehaviours(behaviours);
    }

    private void LookForResources()
    {
        //Set enum 
        m_villagerStatus = VillagerStatusEnum.lookingForResources;
        currentTask.CurrentState = VillagerStatusEnum.lookingForResources;

        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        //SetBehaviours
        List<IBehaviour> behaviours = new List<IBehaviour>()
        {new Wander(Vector3.zero,transform), new AvoidObstacle()};
        m_steerings.SetBehaviours(behaviours);
    }

    private void AvoidEnemies()
    {
        //Set enum 
        m_villagerStatus = VillagerStatusEnum.enemiesInRange;

        //Check if gathering, stop gathering if you are    
        if (gatherCoroutine != null)
        {
            targetNode.Resource.StopGathering(gatherCoroutine);
            gatherCoroutine = null;
        }

        //SetBehaviours
        List<IBehaviour> behaviours = new List<IBehaviour>();

        foreach (GameObject enemy in enemiesInRange)
        {
            behaviours.Add(new Evade(enemy));
            avoidingEnemies.Add(enemy);
        }

        behaviours.Add(new AvoidObstacle());

        m_steerings.SetBehaviours(behaviours);        
    }

    private void UpdateEnemyAvoiding()
    {
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (GameObject enemy in avoidingEnemies)
        {
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesToRemove.Add(enemy);
                m_steerings.RemoveBehaviour(enemy);
            }
        }

        foreach (GameObject enemy in enemiesToRemove)
        {
            avoidingEnemies.Remove(enemy);
        }

        foreach (GameObject enemy in enemiesInRange)
        {
            if (!avoidingEnemies.Contains(enemy))
            {
                m_steerings.AddBehaviour(new Evade(enemy));
                avoidingEnemies.Add(enemy);
            }
        }

    }

    private void SetTargetNode()
    {
        if (currentTask.FoodPriority > currentTask.WoodPriority &&
    currentTask.FoodPriority > currentTask.StonePriority)
        {
            //Food top priorty
            if (currentTask.FoodToGet > 0)
            {
                //Find closest node and set target node varibles 
                targetNode = FindClosestNode(ResourceTypeEnum.food);
                targetNodeType = ResourceTypeEnum.food;
            }
            else
            {
                if (currentTask.StonePriority > currentTask.WoodPriority)
                {
                    //Stone 2nd prio
                    if (currentTask.StoneToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.stone);
                        targetNodeType = ResourceTypeEnum.stone;
                    }
                    else
                    {
                        if (currentTask.WoodToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.wood);
                            targetNodeType = ResourceTypeEnum.wood;
                        }
                    }
                }
                else if (currentTask.StonePriority < currentTask.WoodPriority)
                {
                    //Wood 2nd prio
                    if (currentTask.WoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.wood);
                        targetNodeType = ResourceTypeEnum.wood;
                    }
                    else
                    {
                        if (currentTask.StoneToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.stone);
                            targetNodeType = ResourceTypeEnum.stone;
                        }
                    }
                }
            }
        }
        else if (currentTask.WoodPriority > currentTask.FoodPriority &&
                 currentTask.WoodPriority > currentTask.StonePriority)
        {
            //Wood top priorty
            if (currentTask.WoodToGet > 0)
            {
                //Find closest node and set target node varibles 
                targetNode = FindClosestNode(ResourceTypeEnum.wood);
                targetNodeType = ResourceTypeEnum.wood;
            }
            else
            {
                if (currentTask.StonePriority > currentTask.FoodPriority)
                {
                    //Stone 2nd prio
                    if (currentTask.StoneToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.stone);
                        targetNodeType = ResourceTypeEnum.stone;
                    }
                    else
                    {
                        if (currentTask.FoodToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.food);
                            targetNodeType = ResourceTypeEnum.food;
                        }
                    }
                }
                else if (currentTask.StonePriority < currentTask.FoodPriority)
                {
                    //Food 2nd prio
                    if (currentTask.FoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.food);
                        targetNodeType = ResourceTypeEnum.food;
                    }
                    else
                    {
                        if (currentTask.StoneToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.stone);
                            targetNodeType = ResourceTypeEnum.stone;
                        }
                    }
                }
            }
        }
        else if (currentTask.StonePriority > currentTask.WoodPriority &&
                 currentTask.StonePriority > currentTask.WoodPriority)
        {
            //Stone top priorty
            if (currentTask.StoneToGet > 0)
            {
                //Find closest node and set target node varibles 
                targetNode = FindClosestNode(ResourceTypeEnum.stone);
                targetNodeType = ResourceTypeEnum.stone;
            }
            else
            {
                if (currentTask.FoodPriority > currentTask.WoodPriority)
                {
                    //Food 2nd prio
                    if (currentTask.FoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.food);
                        targetNodeType = ResourceTypeEnum.food;
                    }
                    else
                    {
                        if (currentTask.WoodToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.wood);
                            targetNodeType = ResourceTypeEnum.wood;
                        }
                    }
                }
                else if (currentTask.FoodPriority < currentTask.WoodPriority)
                {
                    //Wood 2nd prio
                    if (currentTask.WoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.wood);
                        targetNodeType = ResourceTypeEnum.wood;
                    }
                    else
                    {
                        if (currentTask.FoodToGet > 0)
                        {
                            targetNode = FindClosestNode(ResourceTypeEnum.food);
                            targetNodeType = ResourceTypeEnum.food;
                        }
                    }
                }
            }
        }
        else if (currentTask.FoodPriority == currentTask.WoodPriority &&
                 currentTask.FoodPriority > currentTask.StonePriority)
        {
            //Food and wood top priority 
            if (currentTask.FoodToGet <= 0)
            {
                if (currentTask.WoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.wood);
                    targetNodeType = ResourceTypeEnum.wood;
                }
                else if (currentTask.StoneToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
            }
            else if (currentTask.WoodToGet <= 0)
            {
                if (currentTask.FoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
                else if (currentTask.StoneToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
            }
            else
            {
                if (Random.Range(0,2) == 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
                else
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.wood);
                    targetNodeType = ResourceTypeEnum.wood;
                }
            }
        }
        else if (currentTask.FoodPriority > currentTask.WoodPriority &&
            currentTask.FoodPriority == currentTask.StonePriority)
        {
            //Food and stone top priority
            if (currentTask.FoodToGet <= 0)
            {
                if (currentTask.StoneToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
                else if (currentTask.WoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.wood);
                    targetNodeType = ResourceTypeEnum.wood;
                }
            }
            else if (currentTask.StoneToGet <= 0)
            {
                if (currentTask.FoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
                else if (currentTask.StoneToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
                else
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
            }
        }
        else if (currentTask.WoodPriority > currentTask.FoodPriority &&
            currentTask.WoodPriority == currentTask.StonePriority)
        {
            //Wood and stone top priority
            if (currentTask.StoneToGet <= 0)
            {
                if (currentTask.WoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.wood);
                    targetNodeType = ResourceTypeEnum.wood;
                }
                else if (currentTask.FoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
            }
            else if (currentTask.WoodToGet <= 0)
            {
                if (currentTask.StoneToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
                else if (currentTask.FoodToGet > 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.food);
                    targetNodeType = ResourceTypeEnum.food;
                }
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.wood);
                    targetNodeType = ResourceTypeEnum.wood;
                }
                else
                {
                    targetNode = FindClosestNode(ResourceTypeEnum.stone);
                    targetNodeType = ResourceTypeEnum.stone;
                }
            }
        }
        else
        {
            //All equal
            //Loop until choice is made
            bool choiceMade = false;
            while (!choiceMade)
            {
                int random = Random.Range(0, 3);
                if (random == 0)
                {
                    if (currentTask.FoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.food);
                        targetNodeType = ResourceTypeEnum.food;
                        choiceMade = true;
                    }
                }
                else if (random == 1)
                {
                    if (currentTask.WoodToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.wood);
                        targetNodeType = ResourceTypeEnum.wood;
                        choiceMade = true;
                    }
                }
                else if (random == 2)
                {
                    if (currentTask.StoneToGet > 0)
                    {
                        targetNode = FindClosestNode(ResourceTypeEnum.stone);
                        targetNodeType = ResourceTypeEnum.stone;
                        choiceMade = true;
                    }
                }
            }
        }
    }
    
   
    private bool CheckIfNodeIsInList(GameObject resource)
    {
        bool foundInList = false;
        foreach (ResourceNode node in resourceNodes)
        {
            //Check if node is in the list
            if (node.Resource.gameObject == resource)
            {
                foundInList = true;
            }
        }

        return foundInList;
    }

    private void AddNodeToList(GameObject resource)
    {
        ResourceNode newNode = new ResourceNode(resource.GetComponent<IResource>(),
            Time.time, resource.GetComponent<IResource>().AbleToBeHarvested());
        resourceNodes.Add(newNode);
    }

    private void UpdateResourceNode(ResourceNode node)
    {
        node.LastSeen = Time.time;
        node.Harvestable = node.Resource.AbleToBeHarvested();
    }

    private void UpdateResourceNode(GameObject obj)
    {
        ResourceNode node;
        foreach (ResourceNode _node in resourceNodes)
        {
            if (_node.Resource.gameObject == obj)
            {
                node = _node;
            }
        }
        node.LastSeen = Time.time;
        node.Harvestable = obj.GetComponent<IResource>().AbleToBeHarvested();
    }

    private ResourceNode FindClosestNode(ResourceTypeEnum type)
    {
        //Initialize varibles
        ResourceNode closestNode = new ResourceNode();
        float distanceToClosestsNode = float.MaxValue;

        foreach (ResourceNode node in resourceNodes)
        {
            //Check if node is wanted type
            if (node.Resource.m_settings.resourceType == type)
            {
                //Calculate if any enemies in vision range and too close to the node
                bool enemyInRangeOfNode = false;
                foreach (GameObject enemy in enemiesInRange)
                {
                    if (Vector3.Distance(node.Resource.gameObject.transform.position, enemy.transform.position) 
                        < m_steerings.m_settings.m_avoidDistance)
                    {
                        enemyInRangeOfNode = true;
                    }
                }

                //Get the distance to the node and see if its closer and no enemies are in range
                float distanceFromNode = Vector3.Distance(transform.position, node.Resource.gameObject.transform.position);
                if (distanceFromNode < distanceToClosestsNode && !enemyInRangeOfNode)
                {
                    //Check if it was harvstable last time
                    if (node.Harvestable)
                    {
                        closestNode = node;
                        distanceToClosestsNode = distanceFromNode;
                    }
                    //If it wasn't harvestable but it's been a while we'll go check it anyways
                    else if (!node.Harvestable && m_settings.m_timeUntilNodeCheck > Time.time - node.LastSeen)
                    {
                        closestNode = node;
                        distanceToClosestsNode = distanceFromNode;
                    }
                }
            }
        }
        //Return node
        return closestNode;
        //Use node.Equals(default(ResourceNode)) to check if not assigned
    }

    private int SpaceInv()
    {
        return m_settings.m_inventorySize - woodInInv - stoneInInv - foodInInv;
    }

    private void OnDrawGizmos()
    {
        GizmoDrawing.DrawCircle(transform.position, m_settings.m_visionRange, Color.red);
    }

    private void OnDisable()
    {
        m_homeVillage.RemoveFromVillage(gameObject);
    }
}

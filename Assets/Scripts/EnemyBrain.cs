using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steering;
using Behaviour = Steering.Behaviour;

[RequireComponent(typeof(Steering.Steering))]
public class EnemyBrain : MonoBehaviour, IEnemy
{
    [SerializeField] private float killRange = 0.2f;
    [SerializeField] private float pursueRange = 3f;

    [SerializeField] private Transform[] wayPoints;

    private GameObject target;
    private Steering.Steering steering;

    private void Start()
    {
        steering = GetComponent<Steering.Steering>();
        ToPatrol();
    }

    private void Update()
    {
        if (target == null)
        {
            target = GetTarget();
            if (target != null)
            {
                ToAttack();
            }
        }
        else if (target != null)
        {
            if (Vector3.Distance(transform.position,target.transform.position) > pursueRange)
            {
                target = GetTarget();
                if (target != null)
                {
                    ToAttack();
                }
                else
                {
                    ToPatrol();
                }
            }

            if (target.transform != null)
            {
                if (Vector3.Distance(transform.position,target.transform.position) < killRange)
                {
                    target.gameObject.SetActive(false);
                    target = null;
                }
            }
        }        
    }

    private void ToAttack()
    {
        List<IBehaviour> behaviours = new List<IBehaviour>() { new Pursue(target), new AvoidObstacle() };
        steering.SetBehaviours(behaviours);
    }
    private void ToPatrol()
    {
        List<IBehaviour> behaviours = new List<IBehaviour>() 
        { new FollowPath(wayPoints, steering.m_settings), new AvoidObstacle() };
        steering.SetBehaviours(behaviours);
    }
    private GameObject GetTarget()
    {
        //Sphere cast in pursue range
        Collider[] collInRange = Physics.OverlapSphere(transform.position, pursueRange);
        //Initialize villager list
        List<GameObject> villagersInRange = new List<GameObject>();
        //Loop through colliders and see if they have a villager brain
        foreach (Collider collider in collInRange)
        {
            if (collider.GetComponent<VillagerBrain>() != null)
            {
                villagersInRange.Add(collider.gameObject);
            }
        }
        //Check if there are villagers in range
        if (villagersInRange.Count > 0)
        {
            //Get the closest villager to attack
            float closestDistance = float.MaxValue;
            GameObject closestTarget = villagersInRange[0];
            for (int i = 0; i < villagersInRange.Count; i++)
            {
                if (Vector3.Distance(transform.position, villagersInRange[i].transform.position) 
                    < closestDistance)
                {
                    closestTarget = villagersInRange[i];
                    closestDistance = Vector3.Distance(transform.position, villagersInRange[i].transform.position);
                }
            }
            return closestTarget;
        }
        else
        {
            return null;
        }

    }
}

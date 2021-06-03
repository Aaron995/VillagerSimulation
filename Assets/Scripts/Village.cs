using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable] //To show struct in inspector 
public struct GatherTask
{
    public GatherTask(int food,int wood, int stone, int foodPrio, int woodPrio, int stonePrio)
    {
        FoodToGet = food;
        WoodToGet = wood;
        StoneToGet = stone;
        FoodPriority = foodPrio;
        WoodPriority = woodPrio;
        StonePriority = stonePrio;
        CurrentState = VillagerStatusEnum.lookingForResources;
    }
    public int FoodToGet;
    public int WoodToGet;
    public int StoneToGet;
    public int FoodPriority;
    public int WoodPriority;
    public int StonePriority;

    public VillagerStatusEnum CurrentState;
}
public class Village : MonoBehaviour
{
    public VillageSettings m_settings;

    public List<GameObject> m_villagers = new List<GameObject>();

    private List<GatherTask> gatherTasks = new List<GatherTask>();

    [SerializeField] private int foodAmount;
    [SerializeField] private int woodAmount;
    [SerializeField] private int stoneAmount;

    public void AddVillagerToVillage(GameObject villager)
    {
        m_villagers.Add(villager);
    }

    public GatherTask GetGatheringTask(int inventorySlots)
    {
        //Get the amount of materials stored and the ones expected to be brought in
        int expectedFood = foodAmount;
        int expectedWood = woodAmount;
        int expectedStone = stoneAmount;
        foreach (GatherTask task in gatherTasks)
        {
            expectedFood += task.FoodToGet;
            expectedWood += task.WoodToGet;
            expectedStone += task.StoneToGet;
        }

        //Get the amount of minimal materials needed to survive clamping it to 0 if nothing is needed to max of what this villager can carry
        Dictionary<string, int> minimalMaterialsNeeded = new Dictionary<string, int>();
        minimalMaterialsNeeded.Add("food", Mathf.Clamp(m_settings.m_foodPerVillager * m_villagers.Count - expectedFood, 0, inventorySlots));
        minimalMaterialsNeeded.Add("wood", Mathf.Clamp(m_settings.m_woodPerVillager * m_villagers.Count - expectedWood,0, inventorySlots));
        minimalMaterialsNeeded.Add("stone", Mathf.Clamp(m_settings.m_stonePerVillager * m_villagers.Count - expectedStone,0, inventorySlots));


        //TODO REWRITE THIS CHOICE PART OR JUST GENERAL CLEANUP
        int foodNeeded = 0;
        int woodNeeded = 0;
        int stoneNeeded= 0;

        //Determine what minimal materials needed, sorted by their priority
        if (m_settings.m_foodPriority > m_settings.m_woodPriority &&
            m_settings.m_foodPriority > m_settings.m_stonePriority)
        {
            //Food top priority           
            foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"],0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
            
            //Check if there is space in inventory to continue
            if (SpaceLeft(inventorySlots, foodNeeded,woodNeeded,stoneNeeded) > 0)
            {
                if (m_settings.m_woodPriority > m_settings.m_stonePriority)
                {
                    //Wood 2nd priority
                    woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    
                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else if (m_settings.m_woodPriority < m_settings.m_stonePriority)
                {
                    //Stone 2nd priority
                    stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else
                {
                    //wood and stone equal
                    //Check if minimal materials needed together are more then space left
                    if (minimalMaterialsNeeded["stone"] + minimalMaterialsNeeded["wood"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
                    {
                        //Try to distrubute evenly by checking if spaceleft is odd or not
                        if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                        {
                            //Check if wood is less then half of the space left
                            if (minimalMaterialsNeeded["wood"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                            {
                                woodNeeded += minimalMaterialsNeeded["wood"];
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["stone"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2) 
                            {
                                stoneNeeded += minimalMaterialsNeeded["stone"];
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);

                            }
                        }
                        else
                        {
                            //Check if wood is less then half of the space left
                            if (minimalMaterialsNeeded["wood"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                woodNeeded += minimalMaterialsNeeded["wood"];
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                stoneNeeded += minimalMaterialsNeeded["stone"];
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, spaceLeft / 2);
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, spaceLeft / 2);

                                //Randomly give the 1 left over to a random resource
                                if (Random.Range(0,2) == 0)
                                {
                                    stoneNeeded += 1;
                                }
                                else
                                {
                                    woodNeeded += 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        //If enough space left to add both minimal materials add them both
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        woodNeeded += minimalMaterialsNeeded["wood"];
                    }
                }
            }            
        }
        else if (m_settings.m_woodPriority > m_settings.m_foodPriority &&
                 m_settings.m_woodPriority > m_settings.m_stonePriority)
        {
            //Wood top priorty
            woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

            //Check if there is space in inventory to continue
            if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
            {
                if (m_settings.m_foodPriority > m_settings.m_stonePriority)
                {
                    //Food 2nd priority
                    foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else if (m_settings.m_foodPriority < m_settings.m_stonePriority)
                {
                    //Stone 2nd priority
                    stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else
                {
                    //food and stone equal
                    //Check if minimal materials needed together are more then space left
                    if (minimalMaterialsNeeded["stone"] + minimalMaterialsNeeded["food"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
                    {
                        //Try to distrubute evenly by checking if spaceleft is odd or not
                        if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                        {
                            //Check if food is less then half of the space left
                            if (minimalMaterialsNeeded["food"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                            {
                                foodNeeded += minimalMaterialsNeeded["food"];
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["stone"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                            {
                                stoneNeeded += minimalMaterialsNeeded["stone"];
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                            }
                        }
                        else
                        {
                            //Check if food is less then half of the space left
                            if (minimalMaterialsNeeded["food"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                woodNeeded += minimalMaterialsNeeded["food"];
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                stoneNeeded += minimalMaterialsNeeded["stone"];
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, spaceLeft / 2);
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, spaceLeft / 2);

                                //Randomly give the 1 left over to a random resource
                                if (Random.Range(0, 2) == 0)
                                {
                                    stoneNeeded += 1;
                                }
                                else
                                {
                                    foodNeeded += 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        //If enough space left to add both minimal materials add them both
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        foodNeeded += minimalMaterialsNeeded["food"];
                    }
                }
            }
        }
        else if (m_settings.m_stonePriority > m_settings.m_woodPriority &&
                 m_settings.m_stonePriority > m_settings.m_woodPriority)
        {
            //Stone top priorty
            stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

            //Check if there is space in inventory to continue
            if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
            {
                if (m_settings.m_woodPriority > m_settings.m_foodPriority)
                {
                    //Wood 2nd priority
                    woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else if (m_settings.m_woodPriority < m_settings.m_foodPriority)
                {
                    //Food 2nd priority
                    foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));

                    if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
                    {
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                }
                else
                {
                    //wood and food equal
                    //Check if minimal materials needed together are more then space left
                    if (minimalMaterialsNeeded["food"] + minimalMaterialsNeeded["wood"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
                    {
                        //Try to distrubute evenly by checking if spaceleft is odd or not
                        if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                        {
                            //Check if wood is less then half of the space left
                            if (minimalMaterialsNeeded["wood"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                            {
                                woodNeeded += minimalMaterialsNeeded["wood"];
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["food"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                            {
                                foodNeeded += minimalMaterialsNeeded["food"];
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);

                            }
                        }
                        else
                        {
                            //Check if wood is less then half of the space left
                            if (minimalMaterialsNeeded["wood"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                woodNeeded += minimalMaterialsNeeded["wood"];
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Check if stone is less then half of the space left
                            else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                            {
                                foodNeeded += minimalMaterialsNeeded["food"];
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                            }
                            //Both minimal materials are more then half of inventory space
                            else
                            {
                                int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                                foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, spaceLeft / 2);
                                woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, spaceLeft / 2);

                                //Randomly give the 1 left over to a random resource
                                if (Random.Range(0, 2) == 0)
                                {
                                    foodNeeded += 1;
                                }
                                else
                                {
                                    woodNeeded += 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        //If enough space left to add both minimal materials add them both
                        foodNeeded += minimalMaterialsNeeded["food"];
                        woodNeeded += minimalMaterialsNeeded["wood"];
                    }
                }
            }
        }
        else if (m_settings.m_foodPriority == m_settings.m_woodPriority &&
                 m_settings.m_foodPriority > m_settings.m_stonePriority)
        {
            //Food and wood  top priority 
            if (minimalMaterialsNeeded["food"] + minimalMaterialsNeeded["wood"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
            {
                //Try to distrubute evenly by checking if spaceleft is odd or not
                if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                {
                    //Check if wood is less then half of the space left
                    if (minimalMaterialsNeeded["wood"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        woodNeeded += minimalMaterialsNeeded["wood"];
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["food"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        foodNeeded += minimalMaterialsNeeded["food"];
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);

                    }
                }
                else
                {
                    //Check if wood is less then half of the space left
                    if (minimalMaterialsNeeded["wood"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        woodNeeded += minimalMaterialsNeeded["wood"];
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        foodNeeded += minimalMaterialsNeeded["food"];
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, spaceLeft / 2);
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, spaceLeft / 2);

                        //Randomly give the 1 left over to a random resource
                        if (Random.Range(0, 2) == 0)
                        {
                            foodNeeded += 1;
                        }
                        else
                        {
                            woodNeeded += 1;
                        }
                    }
                }
            }
            else
            {
                //If enough space left to add both minimal materials add them both
                foodNeeded += minimalMaterialsNeeded["food"];
                woodNeeded += minimalMaterialsNeeded["wood"];
            }

            //If at the end still space left add the minimal stone needed
            if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
            {
                stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
            }
        }
        else if (m_settings.m_foodPriority > m_settings.m_woodPriority &&
            m_settings.m_foodPriority == m_settings.m_stonePriority)
        {
            //Food and stone top priority
            //Check if minimal materials needed together are more then space left
            if (minimalMaterialsNeeded["stone"] + minimalMaterialsNeeded["food"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
            {
                //Try to distrubute evenly by checking if spaceleft is odd or not
                if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                {
                    //Check if food is less then half of the space left
                    if (minimalMaterialsNeeded["food"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        foodNeeded += minimalMaterialsNeeded["food"];
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["stone"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                        foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                    }
                }
                else
                {
                    //Check if food is less then half of the space left
                    if (minimalMaterialsNeeded["food"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        woodNeeded += minimalMaterialsNeeded["food"];
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, spaceLeft / 2);
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, spaceLeft / 2);

                        //Randomly give the 1 left over to a random resource
                        if (Random.Range(0, 2) == 0)
                        {
                            stoneNeeded += 1;
                        }
                        else
                        {
                            foodNeeded += 1;
                        }
                    }
                }
            }
            else
            {
                //If enough space left to add both minimal materials add them both
                stoneNeeded += minimalMaterialsNeeded["stone"];
                foodNeeded += minimalMaterialsNeeded["food"];
            }
        }
        else if (m_settings.m_woodPriority > m_settings.m_foodPriority &&
            m_settings.m_woodPriority == m_settings.m_stonePriority)
        {
            //Wood and stone top priority
            //Check if minimal materials needed together are more then space left
            if (minimalMaterialsNeeded["stone"] + minimalMaterialsNeeded["wood"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
            {
                //Try to distrubute evenly by checking if spaceleft is odd or not
                if (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) % 2 == 0)
                {
                    //Check if wood is less then half of the space left
                    if (minimalMaterialsNeeded["wood"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        woodNeeded += minimalMaterialsNeeded["wood"];
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["stone"] < SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2)
                    {
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) / 2);

                    }
                }
                else
                {
                    //Check if wood is less then half of the space left
                    if (minimalMaterialsNeeded["wood"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        woodNeeded += minimalMaterialsNeeded["wood"];
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Check if stone is less then half of the space left
                    else if (minimalMaterialsNeeded["stone"] < (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1) / 2)
                    {
                        stoneNeeded += minimalMaterialsNeeded["stone"];
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded));
                    }
                    //Both minimal materials are more then half of inventory space
                    else
                    {
                        int spaceLeft = (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) - 1);
                        stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, spaceLeft / 2);
                        woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, spaceLeft / 2);

                        //Randomly give the 1 left over to a random resource
                        if (Random.Range(0, 2) == 0)
                        {
                            stoneNeeded += 1;
                        }
                        else
                        {
                            woodNeeded += 1;
                        }
                    }
                }
            }
            else
            {
                //If enough space left to add both minimal materials add them both
                stoneNeeded += minimalMaterialsNeeded["stone"];
                woodNeeded += minimalMaterialsNeeded["wood"];
            }
        }
        else
        {
            //Everything equal priority
            if (minimalMaterialsNeeded["food"] + minimalMaterialsNeeded["stone"] + minimalMaterialsNeeded["wood"] > SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded))
            {
                int spaceLeft = SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded);
                int extraAmount = 0;
                int doesntFitAmount = 3;
                //Making sure to equal out to a number dividable by 3 and saving the extras we put aside 
                while (spaceLeft % 3 != 0)
                {
                    spaceLeft--;
                    extraAmount++;
                }

                //Check what minimal needs are able to fit in 1/3rd of the inventory 
                if (minimalMaterialsNeeded["food"] <= spaceLeft / 3)
                {                    
                    foodNeeded += minimalMaterialsNeeded["food"];
                    extraAmount += spaceLeft / 3 - minimalMaterialsNeeded["food"];
                    doesntFitAmount--;
                }

                if (minimalMaterialsNeeded["wood"] <= spaceLeft / 3)
                {
                    woodNeeded += minimalMaterialsNeeded["wood"];
                    extraAmount += spaceLeft / 3 - minimalMaterialsNeeded["wood"];
                    doesntFitAmount--;
                }

                if (minimalMaterialsNeeded["stone"] <= spaceLeft / 3)
                {
                    stoneNeeded += minimalMaterialsNeeded["stone"];
                    extraAmount += spaceLeft / 3 - minimalMaterialsNeeded["stone"];
                    doesntFitAmount--;
                }

                //Check if its dividable by the doesn't fit amount if not store extra in variable to randomly give at the end
                int extraForRandom = 0;
                while (extraAmount % doesntFitAmount != 0)
                {
                    extraAmount--;
                    extraForRandom++;
                }

                Dictionary<string,int> notFull = new Dictionary<string,int>();

                if (minimalMaterialsNeeded["food"] > spaceLeft / 3)
                {
                    int totalSpaceAviliable = (spaceLeft / 3) + (extraAmount / doesntFitAmount);
                    foodNeeded += Mathf.Clamp(minimalMaterialsNeeded["food"], 0, totalSpaceAviliable);
                    if (minimalMaterialsNeeded["food"] > totalSpaceAviliable)
                    {
                        notFull.Add("food", minimalMaterialsNeeded["food"] - totalSpaceAviliable);
                    }
                }

                if (minimalMaterialsNeeded["wood"] > spaceLeft / 3)
                {
                    int totalSpaceAviliable = (spaceLeft / 3) + (extraAmount / doesntFitAmount);
                    woodNeeded += Mathf.Clamp(minimalMaterialsNeeded["wood"], 0, totalSpaceAviliable);
                    if (minimalMaterialsNeeded["wood"] > totalSpaceAviliable)
                    {
                        notFull.Add("wood", minimalMaterialsNeeded["wood"] - totalSpaceAviliable);
                    }
                }

                if (minimalMaterialsNeeded["stone"] > spaceLeft / 3)
                {
                    int totalSpaceAviliable = (spaceLeft / 3) + (extraAmount / doesntFitAmount);
                    stoneNeeded += Mathf.Clamp(minimalMaterialsNeeded["stone"], 0, totalSpaceAviliable);
                    if (minimalMaterialsNeeded["stone"] > totalSpaceAviliable)
                    {
                        notFull.Add("stone", minimalMaterialsNeeded["stone"] - totalSpaceAviliable);
                    }
                }


                for (int i = 0; i < extraForRandom; i++)
                {
                    if (notFull.Count > 1)
                    {                        
                        int random = Random.Range(0, notFull.Count);
                        if (notFull.ElementAt(random).Key == "stone")
                        {
                            stoneNeeded++;
                            notFull["stone"]--;
                            if (notFull["stone"] == 0)
                            {
                                notFull.Remove("stone");
                            }
                        }
                        else if (notFull.ElementAt(random).Key == "wood")
                        {
                            woodNeeded++;
                            notFull["wood"]--;
                            if (notFull["wood"] == 0)
                            {
                                notFull.Remove("wood");
                            }
                        }
                        else if (notFull.ElementAt(random).Key == "food")
                        {
                            foodNeeded++;
                            notFull["food"]--;
                            if (notFull["food"] == 0)
                            {
                                notFull.Remove("food");
                            }
                        }
                    }
                    else if (notFull.Count == 1)
                    {                        
                        if (notFull.ElementAt(0).Key == "stone")
                        {
                            stoneNeeded++;
                            notFull["stone"]--;
                            if (notFull["stone"] == 0)
                            {
                                notFull.Remove("stone");
                            }
                        }
                        else if (notFull.ElementAt(0).Key == "wood")
                        {
                            woodNeeded++;
                            notFull["wood"]--;
                            if (notFull["wood"] == 0)
                            {
                                notFull.Remove("wood");
                            }
                        }
                        else if (notFull.ElementAt(0).Key == "food")
                        {
                            foodNeeded++;
                            notFull["food"]--;
                            if (notFull["food"] == 0)
                            {
                                notFull.Remove("food");
                            }
                        }
                    }
                    else
                    {
                        break;
                    }                    
                }
            }
            else
            {
                //There is enough space to add all minimal materials
                foodNeeded += minimalMaterialsNeeded["food"];
                woodNeeded += minimalMaterialsNeeded["wood"];
                stoneNeeded += minimalMaterialsNeeded["stone"];
            }
        }

        //Just randomly assign non essentials materials to fill the inventory
        while (SpaceLeft(inventorySlots, foodNeeded, woodNeeded, stoneNeeded) > 0)
        {
            int random = Random.Range(0,3);
            if (random == 0)
            {
                foodNeeded++;
            }
            else if (random == 1)
            {
                woodNeeded++;
            }
            else if (random == 2)
            {
                stoneNeeded++;
            }
        }

        return new GatherTask(foodNeeded,woodNeeded,stoneNeeded,
            m_settings.m_foodPriority, m_settings.m_woodPriority,m_settings.m_stonePriority);
    }

    private int SpaceLeft(int inventorySpace, int foodToGet, int woodToGet, int stoneToGet)
    {
        return inventorySpace - foodToGet - woodToGet - stoneToGet;
    }
    //If a villager is resting it might as well store all its materials and get a new task when it goes out again
    public void VillagerResting(GatherTask task, int food, int wood, int stone)
    {
        gatherTasks.Remove(task);
        foodAmount += food;
        woodAmount += wood;
        stoneAmount += stone;

        foodAmount -= m_settings.m_foodPerVillager;
        woodAmount -= m_settings.m_woodPerVillager;
        stoneAmount -= m_settings.m_stonePerVillager;
    }
    public void RemoveFromVillage(GameObject villager)
    {
        m_villagers.Remove(villager);
    }
}

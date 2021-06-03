using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Village Settings", menuName = "Village/Village Settings")]

public class VillageSettings : ScriptableObject
{
    [Range(1, 10)] public int m_foodPriority = 3; //How important is food to the village
    [Range(1, 10)] public int m_woodPriority = 2; //How important is wood to the village
    [Range(1, 10)] public int m_stonePriority= 1; //How important is stone to the village

    public int m_foodPerVillager = 3; //How much food does a villager use per rest
    public int m_woodPerVillager = 2; //How much wood does a villager use per rest
    public int m_stonePerVillager= 1; //How much stone does a villager use per rest
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Villager Settings", menuName = "Villager/Villager Settings")]


public class VillagerSettings : ScriptableObject
{
    public int m_inventorySize = 25; //Amount of resources in inventory
    public float m_visionRange = 3.5f; //Range the villager will see enemies and resources
    public float m_timeUntilNodeCheck = 60f; //Time until a villager will recheck an empty node in s
    public float m_gatherRange = 1f; //Range where villagers can gather resources in m
    public float m_restTime = 15f; //Time a villager needs to rest in s
}

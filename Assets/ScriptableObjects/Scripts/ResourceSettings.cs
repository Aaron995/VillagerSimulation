using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource Settings", menuName = "Resources/Resource Settings")]
public class ResourceSettings : ScriptableObject
{
    public int m_startingAmount = 100; //Amount the object spawns with
    public float m_refillDuration = 60f; //How much seconds between each refill "tick" 
    public int m_refillAmount = 10; //How much resources gets added back in on refill
    public float m_gatherTime = 0.9f; //Duration to gather resource
    public int m_gatherAmount = 1; //Amount gather per action
    public ResourceTypeEnum resourceType; //What kind of resource is this

}

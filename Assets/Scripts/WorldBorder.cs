using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBorder : MonoBehaviour
{   
    private void OnTriggerStay(Collider other)
    {
        VillagerBrain brain = other.gameObject.GetComponent<VillagerBrain>();
        if (brain != null)
        {

            brain.TurnAround();
        }
    }
}

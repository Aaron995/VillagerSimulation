using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour, IResource
{
    [SerializeField] private ResourceSettings settings;
    public ResourceSettings m_settings { get => settings; } 

    private int resourceAmount; //Amount of resources in current node
    private float refillTimer; //Refill timer
    void Awake()
    {
        //Set up varibles for use
        resourceAmount = m_settings.m_startingAmount;
        refillTimer = 0;
    }

    void Update()
    {
        //Run refill timer and refill when it hits the duration
        if (refillTimer < m_settings.m_refillDuration && resourceAmount < m_settings.m_startingAmount)
        {
            refillTimer += Time.deltaTime;
        }
        else if (refillTimer >= m_settings.m_refillDuration && resourceAmount < m_settings.m_startingAmount)
        {
            resourceAmount = Mathf.Clamp(resourceAmount + m_settings.m_refillAmount, 0, m_settings.m_startingAmount);
            refillTimer = 0;
        }
    }

    public bool AbleToBeHarvested()
    {
        if (resourceAmount > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Coroutine GatherResource(GameObject gather)
    {
        if (gather.GetComponent<IGather>() != null) //Check if the gather has proper interface
        {
            return StartCoroutine(Gathering(gather)); //Start Coroutine for gather
        }
        else
        {
            Debug.LogError("Non gather trying to gather resource!");
            return null;
        }
    }
    public void StopGathering(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }

    IEnumerator Gathering(GameObject gather)
    {
        //Wait gathering time
        yield return new WaitForSeconds(m_settings.m_gatherTime);        
        //Return resources to gather
        if (m_settings.m_gatherAmount > resourceAmount)
        {
            gather.GetComponent<IGather>().ReturnResource(resourceAmount, m_settings.resourceType);
        }
        else
        {
            gather.GetComponent<IGather>().ReturnResource(m_settings.m_gatherAmount, m_settings.resourceType);
        }
        //Remove resource from resource in node
        resourceAmount -= m_settings.m_gatherAmount; 
    }

}

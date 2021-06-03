using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public static class BehaviourSupport
    {
        public static List<Collider> FindCollidersWithLayerMask(LayerMask layerMask)
        {
            //Make the list we will be returning
            List<Collider> colliders = new List<Collider>();

            //Get all colliders in the scene
            Collider[] allColliders = GameObject.FindObjectsOfType(typeof(Collider)) as Collider[];

            //Sort colliders and add the ones that are in our layermask
            foreach (Collider collider in allColliders)
            {
                if (layerMask == (layerMask | (1 << collider.gameObject.layer)))
                {
                    colliders.Add(collider);
                }
            }
            //Return the list
            return colliders;
        }
    }
}

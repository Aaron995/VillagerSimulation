using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class AvoidObstacle : Behaviour
    {
        //Used to draw gizomos
        private Vector3 hitPoint;
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            if (context.m_velocity.magnitude < 0.5f)
            {
                //return zero steering force if close to target
                return Vector3.zero;
            }

            //Cast the raycast and store hit info 
            RaycastHit[] hits;
            hits = Physics.RaycastAll(context.m_position, context.m_velocity, context.m_settings.m_avoidDistance, context.m_settings.m_avoidLayerMask);

            //Calculate "true" hit point
            RaycastHit hit = new RaycastHit();
            float closestHitPointDistance = float.MaxValue;

            //Loop through all hits
            foreach (RaycastHit rayHit in hits)
            {
                //Check if the object isn't the owner 
                if (rayHit.collider.gameObject != context.owner)
                {
                    //Check if the hit was closer then the closest one so far
                    if (rayHit.distance < closestHitPointDistance)
                    {
                        //Make the closest one the new hit and update distance
                        hit = rayHit;
                        closestHitPointDistance = rayHit.distance;
                    }
                }        
            }

            //Check if the hit actually got changed other wise we "missed"
            if (hit.Equals(default(RaycastHit)))
            {
                //Return zero steering force as there is actually nothing to avoid
                return Vector3.zero;
            }

            //Save hit point for gizmo drawing
            hitPoint = hit.point;

            //Calculate desired velocity
            m_velocityDesired = (hit.point - hit.collider.transform.position).normalized * context.m_settings.m_avoidMaxForce;

            //Make sure desired velocity and velocity are not aligned 
            float angle = Vector3.Angle(m_velocityDesired, context.m_velocity);
            if (angle > 179)
            {
                m_velocityDesired = Vector3.Cross(Vector3.up, context.m_velocity);
            }

            //Return steering force
            return m_velocityDesired - context.m_velocity;            
        }

        public override void OnDrawGizmos(BehaviourContext context)
        {
            GizmoDrawing.DrawRayWithDisc(context.m_position, context.m_velocity.normalized  * context.m_settings.m_avoidDistance, Color.yellow);
            GizmoDrawing.DrawDot(hitPoint, Color.green);
        }
    }
}

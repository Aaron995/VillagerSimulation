using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class SeekMovement : Behaviour
    {
        private Vector3 targetPosition;

        //Refer gameobject for dynamic seek going towards a moving target
        public SeekMovement(GameObject _target)
        {
            m_target = _target;
        }
        //Refer a Vector3 for moving towards a location
        public SeekMovement(Vector3 _targetPosition)
        {
            targetPosition = _targetPosition;
        }
        
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Get target position       
            if (m_target == null)
            {
                context.m_positionTarget = targetPosition;
            }
            else
            {
                context.m_positionTarget = m_target.transform.position;
            }
            //Set the position target y to current one
            context.m_positionTarget.y = context.m_position.y;

            //Calculate desired velocity and return steering force
            if (ArriveEnabled(context) && WithinArriveSlowingDistnace(context))
            {
                m_velocityDesired = CalculateArriveSteeringForce(dt, context);
            }
            else
            {
                m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;                  
            }
            return m_velocityDesired - context.m_velocity;
        }
    }
}

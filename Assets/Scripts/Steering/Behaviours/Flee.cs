using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{

    public class Flee : Behaviour
    {   
        public Flee(GameObject _target)
        {
            m_target = _target;
        }


        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Check if in-range to flee.
            if (Vector3.Distance(context.m_position, m_target.transform.position) < context.m_settings.m_fleeRange)
            {
                context.m_positionTarget = m_target.transform.position; //Set the target position to flee away from
            }
            else
            {
                context.m_positionTarget = context.m_position;
            }


            //Calculate desired velocity and return steering force
            m_velocityDesired = -(context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
            return m_velocityDesired - context.m_velocity;
        }
    }
}

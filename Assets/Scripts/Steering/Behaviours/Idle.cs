using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{

    public class Idle : Behaviour
    {
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Update target position , desired velocity and return steering force
            context.m_positionTarget = context.m_position;

            m_velocityDesired = Vector3.zero;

            return m_velocityDesired - context.m_velocity;
        }
    }
}

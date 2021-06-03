using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class Pursue : Behaviour
    {
        private Vector3 targetPos;
        public Pursue(GameObject _target)
        {
            m_target = _target;
            targetPos = _target.transform.position;
        }

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {   
            //Get the last target position and update the current one
            Vector3 prevTargetPos = targetPos;
            targetPos = m_target.transform.position;

            //Calculate target speed
            Vector3 targetVelocity = (targetPos - prevTargetPos) / dt;

            //Calculate target position 
            context.m_positionTarget = targetPos + targetVelocity * context.m_settings.m_lookAheadTime;

            //Calculate deisred velocity
            if (ArriveEnabled(context) && WithinArriveSlowingDistnace(context))
            {
                m_velocityDesired = CalculateArriveSteeringForce(dt, context);
            }
            else
            {
                m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
            }

            //return steering force
            return m_velocityDesired - context.m_velocity;
        }

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
            GizmoDrawing.DrawDot(context.m_positionTarget, Color.black);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{

    public class Evade : Behaviour
    {       
        private Vector3 targetPos;
        public Evade(GameObject _target)
        {
            m_target = _target;
            targetPos = _target.transform.position;
        }

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Get the last target position and update the current one and set the y to our current one
            Vector3 prevTargetPos = targetPos;
            prevTargetPos.y = context.m_position.y;
            targetPos = m_target.transform.position;
            targetPos.y = context.m_position.y;

            //Calculate target speed
            Vector3 targetVelocity = (targetPos - prevTargetPos) / dt;

            if (Vector3.Distance(context.m_position,targetPos) >= context.m_settings.m_evadeRange)
            {
                context.m_positionTarget = context.m_position;
            }
            else
            {
                //Calculate target position 
                context.m_positionTarget = targetPos + targetVelocity * context.m_settings.m_lookAheadTime;
            }

            //Calculate deisred velocity
            m_velocityDesired = -(context.m_positionTarget - context.m_position).normalized * context.m_settings.m_evadeMaxForce;

            //return steering force
            return m_velocityDesired - context.m_velocity;
        }

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
            GizmoDrawing.DrawDot(context.m_positionTarget, Color.black);
            GizmoDrawing.DrawCircle(context.m_position, context.m_settings.m_evadeRange, Color.black);
        }
    }
}

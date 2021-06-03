using UnityEngine;

namespace Steering
{
    public abstract class Behaviour : IBehaviour
    {
        [Header("Behaviour Runtime")]
        public Vector3 m_velocityDesired = Vector3.zero; //Desired velocity

        public GameObject m_target { get; protected set; } = null;

        public virtual void Start(BehaviourContext context)
        {
            
        }


        public abstract Vector3 CalculateSteeringForce(float dt, BehaviourContext context);
        public bool ArriveEnabled(BehaviourContext context)
        {
            return context.m_settings.m_slowingDistance > 0f;
        }

        public bool WithinArriveSlowingDistnace(BehaviourContext context)
        {
            return Vector3.Distance(context.m_position, context.m_positionTarget) <= context.m_settings.m_stoppingDistance;
        }
        public Vector3 CalculateArriveSteeringForce(float dt, BehaviourContext context)
        {
            //Make sure we have a legal slowing distnace
            if (!ArriveEnabled(context))
            {
                return Vector3.zero;
            }

            //Calculate stop offset
            Vector3 stopVector = (context.m_position - context.m_positionTarget).normalized * context.m_settings.m_arriveDistance;
            Vector3 stopPosition = context.m_positionTarget + stopVector;

            //Calculate the target offset and distance
            Vector3 targetOffset = stopPosition - context.m_position;
            float distance = targetOffset.magnitude;

            //Calculate the ramped speed and clip it
            float rampedSpeed = context.m_settings.m_maxDesiredVelocity * (distance / context.m_settings.m_slowingDistance);
            float clippedSpeed = Mathf.Min(rampedSpeed, context.m_settings.m_maxDesiredVelocity);

            //Update desired velocity and staaring force
            if (distance > 0.01f)
            {
                m_velocityDesired = (clippedSpeed / distance) * targetOffset;
            }
            else
            {
                m_velocityDesired = Vector3.zero;
            }         
            return m_velocityDesired - context.m_velocity;
        }

        //--------------------------------------------------------------------------------
        //-----------------------DEBUG STUFF----------------------------------------------
        //--------------------------------------------------------------------------------

        public virtual void OnDrawGizmos(BehaviourContext context)
        {
            GizmoDrawing.DrawRayWithDisc(context.m_position, m_velocityDesired, Color.cyan);            
        }

        public void OnDrawArriveGizmos(BehaviourContext context)
        {
            GizmoDrawing.DrawCircle(context.m_positionTarget, context.m_settings.m_stoppingDistance, Color.white);
            GizmoDrawing.DrawCircle(context.m_positionTarget, context.m_settings.m_slowingDistance, Color.black);
        }

    }
}
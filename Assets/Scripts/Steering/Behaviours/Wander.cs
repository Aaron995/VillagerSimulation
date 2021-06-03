using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class Wander : Behaviour
    {
        private float wanderAngle;
        public Wander(Transform _transform)
        {
            wanderAngle = Vector3.SignedAngle(_transform.forward, Vector3.right, Vector3.up) * Mathf.Deg2Rad;
        }

        public Wander(Vector3 _target, Transform _transform)
        {
            _transform.LookAt(_target);
            wanderAngle = Vector3.SignedAngle(_transform.forward, Vector3.right, Vector3.up) * Mathf.Deg2Rad;
        }

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Update the wander delta with random angle within the range defined by noise angle
            wanderAngle += Random.Range(-0.5f * context.m_settings.m_wanderNoiseAngle * Mathf.Deg2Rad, 
                0.5f * context.m_settings.m_wanderNoiseAngle * Mathf.Deg2Rad);

            //Calculate the center of the circle
            Vector3 centerOfCircle = context.m_position + context.m_velocity.normalized * 
                context.m_settings.m_wanderCircleDistance;

            //Calculate the offset on the circle
            Vector3 offset = new Vector3(context.m_settings.m_wanderCircleRadius * Mathf.Cos(wanderAngle), 0, 
                context.m_settings.m_wanderCircleRadius * Mathf.Sin(wanderAngle));

            //Update target position plys desired velocity and return steering force
            context.m_positionTarget = centerOfCircle + offset;
            m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * 
                context.m_settings.m_maxDesiredVelocity;
            return m_velocityDesired - context.m_velocity;
        }

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);

            //Draw circle
            Vector3 centerOfCircle = context.m_position + context.m_velocity.normalized * context.m_settings.m_wanderCircleDistance;
            GizmoDrawing.DrawCircle(centerOfCircle, context.m_settings.m_wanderCircleRadius, Color.black);

            //Draw noise lines
            float a = context.m_settings.m_wanderNoiseAngle * Mathf.Deg2Rad;

            Vector3 rangeMin = new Vector3(context.m_settings.m_wanderCircleRadius * Mathf.Cos(wanderAngle - a), 0f, context.m_settings.m_wanderCircleRadius * Mathf.Sin(wanderAngle - a));
            Vector3 rangeMax = new Vector3(context.m_settings.m_wanderCircleRadius * Mathf.Cos(wanderAngle + a), 0f, context.m_settings.m_wanderCircleRadius * Mathf.Sin(wanderAngle + a));

            GizmoDrawing.DrawLine(centerOfCircle, centerOfCircle + rangeMin, Color.black);
            GizmoDrawing.DrawLine(centerOfCircle, centerOfCircle + rangeMax, Color.black);

            //Draw chosen target
            GizmoDrawing.DrawDot(context.m_positionTarget, Color.cyan);
        }
    }
}

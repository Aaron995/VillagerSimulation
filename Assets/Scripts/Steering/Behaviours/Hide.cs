using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class Hide : Behaviour
    {
        private List<Collider> colliders = new List<Collider>(); //List with colliders in the scene we can hide behind
        private List<Vector3> hidingPlaces = new List<Vector3>();//List with hiding places
        private Vector3 hidingPlace; //The current hiding place

        public Hide(GameObject _target)
        {
            m_target = _target;
        }

        public override void Start(BehaviourContext context)
        {
            base.Start(context);
            colliders = BehaviourSupport.FindCollidersWithLayerMask(context.m_settings.m_hideLayerMask);
        }
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            //Calculate position target, dsired velocity and return steering force
            context.m_positionTarget = CalculateHidingPlace(context);
            if (Vector3.Distance(context.m_positionTarget, context.m_position) < 0.1f)
            {
                context.m_positionTarget = context.m_position;
            }
            m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
            return m_velocityDesired - context.m_velocity;
        }


        private Vector3 CalculateHidingPlace(BehaviourContext context)
        {
            float closetDistanceSqr = float.MaxValue;
            hidingPlace = context.m_position;
            hidingPlaces = new List<Vector3>();

            for (int i = 0; i < colliders.Count; i++)
            {
                //Get hiding place for current obstacle and remember it for gizmodrawing
                Vector3 _hidingPlace = CalculateHidingPlace(context, colliders[i]);
                hidingPlaces.Add(_hidingPlace);

                //Update closest hiding place if this hiding place is closer then the current chosen one
                float distanceToHidingPlaceSqr = (context.m_position - _hidingPlace).sqrMagnitude;
                if (distanceToHidingPlaceSqr < closetDistanceSqr)
                {
                    closetDistanceSqr = distanceToHidingPlaceSqr; //New closest point update the distance
                    hidingPlace = _hidingPlace; //Update the new hiding place
                }
            }
            //return the hiding place
            return hidingPlace;
        }
        
        private Vector3 CalculateHidingPlace(BehaviourContext context, Collider collider)
        {
            //Calculate place for current obstacle
            Vector3 obstacleDirection = (collider.transform.position - m_target.transform.position).normalized;
            Vector3 pointOtherSide = collider.transform.position + obstacleDirection;
            Vector3 hidingPlace = collider.ClosestPointOnBounds(pointOtherSide) + (obstacleDirection * context.m_settings.m_hideOffset);

            //return hiding place
            return hidingPlace;
        }

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);

            foreach (Vector3 _hidingPlace in hidingPlaces)
            {
                if (_hidingPlace == hidingPlace)
                {
                    continue;
                }
                GizmoDrawing.DrawDot(_hidingPlace, Color.green);
            }

            GizmoDrawing.DrawDot(hidingPlace, Color.magenta);
        }
    }
}

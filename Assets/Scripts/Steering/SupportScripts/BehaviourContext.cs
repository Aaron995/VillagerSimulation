using UnityEngine;

namespace Steering
{
    public class BehaviourContext
    {
        public Vector3 m_position; // The current position
        public Vector3 m_positionTarget; //Target position
        public Vector3 m_velocity; //The current velocity
        public SteeringSettings m_settings; // All steering settings
        public GameObject owner; //The root gameobject that uses this steering


        public BehaviourContext(Vector3 position,Vector3 targetPos, Vector3 velocity, SteeringSettings settings, GameObject _owner)
        {
            m_position = position;
            m_positionTarget = targetPos;
            m_velocity = velocity;
            m_settings = settings;
            owner = _owner;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    [CreateAssetMenu(fileName = "Steering Settings", menuName = "Steering/Steering Settings")]

    public class SteeringSettings : ScriptableObject
    {
        [Header("Steering Settings")]
        public float m_maxMoveSpeed = 3f; //Max vehicle speed in m/s
        public float m_mass = 70f; //mass in kg
        public float m_maxSteeringForce = 3f; //max force in m/s
        public float m_maxDesiredVelocity = 3f; //max desired velocity in m/s

        [Header("Arrive")]
        public float m_arriveDistance = 1f; //Distance to target when we reach zero velocity in m
        public float m_slowingDistance = 2f; //Distance to the stop position where we start slowing down

        [Header("Seeking")]
        public float m_stoppingDistance = 1f; //Distance from target you are seeking in m

        [Header("Follow Path")]
        public bool m_loopPath = true; //Keep on walking on the path
        public PathStatusEnum m_startPathMode = PathStatusEnum.forwards; //Start off walking backwards or forwards
        public float m_waypointRadius = 1f; //The distance a behaviour will be able to "collect" the waypoint

        [Header("Flee")]
        public float m_fleeRange = 1f; //Distance until fleeing stops from target in m

        [Header("Pursuit and Evade")]
        public float m_lookAheadTime = 1f; //Look ahead time from target in s

        [Header("Evade")]
        public float m_evadeRange = 1f; //Distance until behaviour will stop evading the target in m
        public float m_evadeMaxForce = 5f; //Max steering force to evade target in m/s


        [Header("Wander")]
        public float m_wanderCircleDistance = 5f; //cicle distance in m
        public float m_wanderCircleRadius = 5f; //cirlce radius in m
        public float m_wanderNoiseAngle = 10f; //noise angle in degrees

        [Header("Obstacle Avoidance")]
        public float m_avoidMaxForce = 5f; //Max steering force to avoid obstacles in m/s
        public float m_avoidDistance = 2.5f; //Max distance to avoid objects
        public LayerMask m_avoidLayerMask; //The layer(s) the obstacles are on

        [Header("Hide")]
        public float m_hideOffset = 1f; //The distance from surface on other side of the collider
        public LayerMask m_hideLayerMask; //The layer(s) you can hide behind

    }
}
    
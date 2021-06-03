using UnityEngine;

namespace Steering
{
    public interface IBehaviour
    {
        /// <summary>
        /// Allow other scripts to access target of behaviour
        /// </summary>
        GameObject m_target { get; }
        /// <summary>
        /// Allow the behavior to initialize.
        /// </summary>
        /// <param name="context"> All the context information needed to perfrom the task at hand</param>
        void Start(BehaviourContext context);

        /// <summary>
        /// Calculate the steering force contributed by this behavior.
        /// </summary>
        /// <param name="dt"> The delta time for this step</param>
        /// <param name="context"> All the context information needed to perform the task at hand</param>
        Vector3 CalculateSteeringForce(float dt, BehaviourContext context);

        /// <summary>
        /// Draw the gizmos for this behavior.
        /// </summary>
        /// <param name="context"> All the context information needed to perform the task at hand</param>
        void OnDrawGizmos(BehaviourContext context);
    }
}

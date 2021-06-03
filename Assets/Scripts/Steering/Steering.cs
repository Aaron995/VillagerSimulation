using System.Collections.Generic;
using UnityEngine;


namespace Steering
{
    using BehaviourList = List<IBehaviour>;
    public class Steering : MonoBehaviour
    {
        [Header("Steering Settings")]
        public string m_label;
        public SteeringSettings m_settings; //Steering settings for all behaviors

        [Header("Steering Runtime")]
        public Vector3 m_position = Vector3.zero; //Current Position
        public Vector3 m_positionTarget; //Target position
        public Vector3 m_velocity = Vector3.zero; // current velocity
        public Vector3 m_steering; // steering force
        public BehaviourList m_behaviors = new BehaviourList(); // all behavoirs
        public BehaviourContext behaviourContext;

        [Header("Animation Runtime")]
        public float m_idleDuration;
        public Animator animator;

        [Header("Animation debug setting")]
        public bool useAnimation;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        void Start()
        {
            m_position = transform.position;
        }

        void FixedUpdate()
        {
            if (m_settings != null)
            {
                UpdateSteering();
            }
            if (useAnimation && m_settings != null)
            {
                UpdateAnimations();
            }
        }

        private void UpdateSteering()
        {
            //calculate steering force
            m_steering = Vector3.zero;
            foreach (IBehaviour behaviour in m_behaviors)
            {
                m_steering += behaviour.CalculateSteeringForce(Time.fixedDeltaTime, behaviourContext);
            }

            //Make sure steering is only done in the xz plane
            m_steering.y = 0f;

            //Clamp steering force to max and apply mass
            m_steering = Vector3.ClampMagnitude(m_steering, m_settings.m_maxSteeringForce);
            m_steering /= m_settings.m_mass;

            //Update Velocity with steering force and update position and target position            
            m_velocity = Vector3.ClampMagnitude(m_velocity + m_steering, m_settings.m_maxMoveSpeed);
            m_position += m_velocity * Time.fixedDeltaTime;
            m_positionTarget = behaviourContext.m_positionTarget;

            //Update the context
            behaviourContext.m_position = m_position;
            behaviourContext.m_velocity = m_velocity;

            //Update object with new position
            transform.position = m_position;
            transform.LookAt(m_position + Time.fixedDeltaTime * m_velocity);
        }

        private void UpdateAnimations()
        {
            float speed = m_velocity.magnitude;
            if (speed < 0.1f)
            {
                speed = 0;
            }
            animator.SetFloat("Speed", speed);
            animator.SetBool("Running", speed > behaviourContext.m_settings.m_maxMoveSpeed / 2);
        }

        public void SetBehaviours(BehaviourList behaviors, string label = "")
        {
            //Create behaviour context            
            behaviourContext = new BehaviourContext(m_position, m_position, m_velocity, m_settings, gameObject);

            //Remember the new settings
            m_label = label;
            m_behaviors = behaviors;

            //Start all behaviors
            foreach (IBehaviour behaviour in m_behaviors)
            {
                behaviour.Start(behaviourContext);
            }

        }

        public void AddBehaviour(IBehaviour behaviour)
        {
            m_behaviors.Add(behaviour);
        }

        public void RemoveBehaviour(IBehaviour behaviour)
        {
            if (m_behaviors.Contains(behaviour))
            {
                m_behaviors.Remove(behaviour);
            }
            else
            {
                Debug.LogWarning("Trying to remove " + behaviour.ToString() + " from steering behaviours. But it doesn't exist!");
            }
        }

        public void RemoveBehaviour(GameObject target)
        {
            foreach (IBehaviour behaviour in m_behaviors)
            {
                if (target == behaviour.m_target)
                {
                    m_behaviors.Remove(behaviour);
                    break;
                }
            }
        }
        //--------------------------------------------------------------------------------
        //-----------------------DEBUG STUFF----------------------------------------------
        //--------------------------------------------------------------------------------
        private void OnDrawGizmos()
        {
            GizmoDrawing.DrawRayWithDisc(transform.position, m_velocity, Color.red);
            GizmoDrawing.DrawLabel(transform.position, m_label, Color.black);

            //Draw all gizomos in all behaviors
            foreach (IBehaviour behaviour in m_behaviors)
            {
                behaviour.OnDrawGizmos(behaviourContext);
            }


        }
    }
}

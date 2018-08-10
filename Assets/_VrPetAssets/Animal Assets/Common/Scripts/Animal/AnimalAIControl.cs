using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using UnityEngine.AI;

namespace MalbersAnimations
{
    public class AnimalAIControl : MonoBehaviour
    {
        #region Components References
        private NavMeshAgent agent; // The NavMeshAgent
        protected Animal animal; // The Animal Script
        private Animator animalAnimator;
        #endregion

        #region Target Verifications
        protected Animal Target_is_Animal; // To check if the target is an animal
        protected ActionZone Target_is_ActionZone; // To check if the Target is an Action Zone
        protected MWayPoint Target_is_Waypoint; // To check if the Target is a Way Point
        #endregion

        public Transform target; // The target to path to.
        Transform deltaTarget; // Used to check if the Target has changed
        private Transform nextTarget;
        private Transform closestGrabableItem;
        public GameObject animalHead; // To give other scripts that use it a universal reference to the head transform.
        public bool AutoSpeed = true;
        public float ToTrot = 6f;
        public float ToRun = 8f;
        private float interruptTimer;

        public bool debug = true;                   // Debugging 
        public bool isWandering = false;

        bool isInActionState;                       //Check if is making any Animation Action
        bool StartingAction;                        //Check if Start the animation  

        bool sawTarget, targetOverride, iAmFixingTheAgentDisplacement;

        protected int sawTargetLayerMask = (1 << 8) | (1 << 0);// This makes a layermask using the number 8 because that is the layer that I've put all the props on.
        enum MovementStates
        {
            NormalMovement = 0,
            AlwaysWalk = 1,
            AlwaysRun = 2,
            SlowWalk = 3,
            FastRun = 4
        }
        MovementStates currentMovementState = MovementStates.NormalMovement;

        protected float DefaultStoppingDistance;

        /// <summary>
        /// Important for changing Waypoints
        /// </summary>
        private bool isMoving = false;

        private float wanderTimer;
        private float timer;

        /// <summary>
        /// the navmeshAgent associated to this GameObject
        /// </summary>
        public NavMeshAgent Agent
        {
            get
            {
                if (agent == null)
                {
                    agent = GetComponentInChildren<NavMeshAgent>();
                }
                return agent;
            }
        }


        void Start()
        {
            StartAgent();
            UpdateTarget();
        }

        /// <summary>
        /// Initialize the Ai Animal Control Values
        /// </summary>
        protected virtual void StartAgent()
        {
            wanderTimer = 3;
            timer = wanderTimer;
            animal = GetComponent<Animal>();
            animalAnimator = GetComponent<Animator>();
            Agent.updateRotation = false;
            Agent.updatePosition = false;
            isWandering = true;
            DefaultStoppingDistance = Agent.stoppingDistance; // Save the Stopping Distance.
        }

        void Update()
        {
            DisableAgent();
            TryActionZone();
            timer += Time.deltaTime;
            Agent.nextPosition = agent.transform.position; // Update the Agent Position to the Transform position

            if (!Agent.isOnNavMesh || !Agent.enabled || target == null || interruptTimer > timer)
            {
                return;
            }

            if ((Agent.nextPosition - transform.position).magnitude > 1f && !iAmFixingTheAgentDisplacement)
            {
                iAmFixingTheAgentDisplacement = true;
                StartCoroutine(FixAgentDisplacement());
            }

            UpdateTarget();
            UpdateAgent();

            if (!sawTarget && !isWandering)
            {
                CheckSightlineToTarget(); // This is something that can be just fine with a margin of error of .3 seconds to first seeing something. So if performance is lagging, feel free to cut corners here.
            }
        }

        IEnumerator FixAgentDisplacement()
        {
            yield return new WaitForSeconds(.5f);
            if ((Agent.nextPosition - transform.position).magnitude > 1f)
            {
                Agent.Warp(Agent.transform.position);
            }
            iAmFixingTheAgentDisplacement = false;
        }

        void UpdateTarget()
        {
            if (deltaTarget != target)
            {
                if (deltaTarget != null)
                {
                    ActionZone Prev_Target_ActionZon = deltaTarget ? deltaTarget.GetComponent<ActionZone>() : null;
                    if (Prev_Target_ActionZon)
                    {
                        Prev_Target_ActionZon.enabled = false;
                    }
                }

                deltaTarget = target;

                if (deltaTarget)
                {
                    Target_is_Animal = deltaTarget.GetComponent<Animal>();
                    Target_is_ActionZone = deltaTarget.GetComponent<ActionZone>();
                    Target_is_Waypoint = deltaTarget.GetComponent<MWayPoint>();

                    if (Target_is_ActionZone)
                    {
                        Target_is_ActionZone.enabled = true;
                        Agent.stoppingDistance = Target_is_ActionZone.stoppingDistance;
                    }
                    else if (Target_is_Waypoint)
                    {
                        Agent.stoppingDistance = Target_is_Waypoint.StoppingDistance;
                    }
                }
                else
                {
                    Target_is_ActionZone = null;
                    Target_is_Animal = null;
                    Target_is_Waypoint = null;

                    Agent.stoppingDistance = DefaultStoppingDistance;
                }

                sawTarget = false;
            }
        }

        /// <summary>
        /// This will disable the Agent when is not on Idle or Locomotion 
        /// </summary>
        void DisableAgent()
        {
            if ((animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Idle")))          //Activate the Agent when the animal is moving
            {
                if (!Agent.enabled)
                {
                    Agent.enabled = true;
                    isMoving = false; // Important.
                }
            }
            else
            {
                Agent.enabled = false;
            }

            if (animal.IsInAir) //Don't rotate if it's in the middle of a jump
            {
                animal.Move(Vector3.zero);
            }
        }

        /// <summary>
        /// Manage every time the animal enters an Action Zone
        /// </summary>
        void TryActionZone()
        {
            if (isInActionState != animal.CurrentAnimState.IsTag("Action"))  //If the animal is Executing an action, save it
            {
                isInActionState = animal.CurrentAnimState.IsTag("Action");   //Store it if there was any change on the action 

                if (isInActionState)
                {
                    
                }
                else
                {
                    StartingAction = false;
                }
            }
        }

        Vector3 pos;
        /// <summary>
        /// Updates the Agents using he animation root motion
        /// </summary>
        protected virtual void UpdateAgent()
        {
            Vector3 Direction = Vector3.zero;                             //Set the Direction to Zero         

            if (isWandering)
            {
                if (timer >= wanderTimer)
                {
                    pos = RandomNavSphere(transform.position, 300, -1);
                    timer = interruptTimer = 0;
                }

                if (pos != Vector3.zero)
                {
                    Agent.SetDestination(pos);
                }
            }
            else if (target != null)
            {
                Agent.SetDestination(target.position);
            }

            if (Agent.remainingDistance > Agent.stoppingDistance)
            {
                Direction = Agent.desiredVelocity.normalized;
                isMoving = true;
                StartingAction = false;
            }
            else
            {
                if (Target_is_ActionZone && !StartingAction)        //If the Target is an Action Zone Start the Action
                {
                    StartingAction = true;
                    //animal.Action = true;                           //Activate the Action on the Animal
                }

                if (Target_is_Waypoint && isMoving)
                {
                    SetTarget(Target_is_Waypoint.NextWaypoint.transform);
                }
            }

            animal.Move(Direction);                                 //Set the Movement to the Animal

            if (AutoSpeed) AutomaticSpeed();                         //Set Automatic Speeds

            CheckOffMeshLinks();                                     //Jump/Fall behaviour 
        }

        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;

            randDirection += origin;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
        }

        /// <summary>
        /// Manage all Off Mesh Links
        /// </summary>
        protected virtual void CheckOffMeshLinks()
        {
            if (Agent.isOnOffMeshLink)
            {
                OffMeshLinkData CurrentOffmeshLink_Data = Agent.currentOffMeshLinkData;

                OffMeshLink CurrentOffMeshLink = CurrentOffmeshLink_Data.offMeshLink;  //Check if the OffMeshLink is a Custom Off Mesh Link
                ActionZone OffMeshZone = null;

                if (CurrentOffMeshLink)                                         //Checking if the OffMeshLink is an Action Zone
                {
                    OffMeshZone = CurrentOffMeshLink.GetComponentInChildren<ActionZone>();
                    if (!OffMeshZone) OffMeshZone = CurrentOffMeshLink.GetComponentInParent<ActionZone>();
                }

                if (OffMeshZone)
                {
                    if (!StartingAction)        //If the Target is an Action Zone Start the Action
                    {
                        StartingAction = true;
                        //animal.Action = true;                           //Activate the Action on the Animal
                        return;
                    }
                }

                if (CurrentOffmeshLink_Data.linkType == OffMeshLinkType.LinkTypeManual)
                {
                    Transform NearTransform = CurrentOffMeshLink.startTransform;

                    if (CurrentOffMeshLink.endTransform.position == CurrentOffmeshLink_Data.startPos) //Verify the start point of the OffMeshLink
                    {
                        NearTransform = CurrentOffMeshLink.endTransform;
                    }

                    StartCoroutine(MalbersTools.AlignTransformsC(transform, NearTransform, 0.5f, false, true)); // Align the Animal to the Link

                    if (CurrentOffMeshLink.area == 2)                          //if the OffMesh Link is a Jump type
                    {
                        animal.SetJump();
                    }
                }
            }
        }

        /// <summary>
        /// Change velocities
        /// </summary>
        protected virtual void AutomaticSpeed()
        {
            if (currentMovementState == MovementStates.NormalMovement)
            {
                if (Agent.remainingDistance < ToTrot)         //Set to Walk
                {
                    animal.Speed1 = true;
                }
                else if (Agent.remainingDistance < ToRun)     //Set to Trot
                {
                    animal.Speed2 = true;
                }
                else if (Agent.remainingDistance > ToRun)     //Set to Run
                {
                    animal.Speed3 = true;
                }
            }
            else if (currentMovementState == MovementStates.AlwaysWalk || currentMovementState == MovementStates.SlowWalk)
            {
                animal.Speed1 = true;
            }
            else if (currentMovementState == MovementStates.AlwaysRun || currentMovementState == MovementStates.FastRun)
            {
                animal.Speed3 = true;
            }

            if (currentMovementState == MovementStates.FastRun)
            {
                animalAnimator.speed = 2f;
            }
            else if (currentMovementState == MovementStates.SlowWalk)
            {
                animalAnimator.speed = .3f;
            }
            else
            {
                animalAnimator.speed = 1f;
            }
        }

        /// <summary>
        /// Set to next Target
        /// </summary>
        public void SetTarget(Transform target, bool ignoreOverride = false)
        {
            if (!targetOverride || ignoreOverride)
            {
                isMoving = false;

                if (target)
                {
                    isWandering = false;
                    this.target = target;
                }
                else
                {
                    isWandering = true;
                }

                UpdateTarget();
            }
        }

        public void SetClosestGrabbableItem(Transform item)
        {
            closestGrabableItem = item;
        }

        public Transform GetClosestGrabableItem()
        {
            return this.closestGrabableItem;
        }

        /// <summary>
        /// Stop movement of this agent along its current path.
        /// </summary>
        protected virtual void Agent_Stop()
        {
            Agent.isStopped = true;
        }

        /// <summary>
        /// Resume the movement along the current path after a pause
        /// </summary>
        protected void Agent_Resume()
        {
            Agent.isStopped = false;
        }

        //Toggle Off and On the Agent
        IEnumerator ToggleAgent()
        {
            Agent.enabled = false;
            yield return null;
            Agent.enabled = true;
        }

        public void CheckSightlineToTarget()
        {
            Vector3 directionToTarget = target.position - animalHead.transform.position;
            RaycastHit hit;
            Physics.Raycast(new Ray(animalHead.transform.position, directionToTarget), out hit, directionToTarget.magnitude, sawTargetLayerMask, QueryTriggerInteraction.Ignore);

            float degreesToTarget = Mathf.Abs(FunctionalAssist.AngleOffAroundAxis(transform.forward, directionToTarget, Vector3.up));

            if (degreesToTarget < 90f && hit.transform == target)
            {
                if (Target_is_ActionZone) Target_is_ActionZone.onSight.Invoke();

                sawTarget = true;
            }
        }

        public void InterruptPathing(float delayTime)
        {
            interruptTimer = timer + delayTime;
            animal.Move(Vector3.zero);
        }

        public void ChangeMovement(int newMovement)
        {
            currentMovementState = (MovementStates)newMovement;
        }

        public void ToggleTargetOverride()
        {
            targetOverride = !targetOverride;
        }

#if UNITY_EDITOR
        /// <summary>
        /// DebugOptions
        /// </summary>
        void OnDrawGizmos()
        {
            if (!debug) return;

            if (AutoSpeed)
            {
                Vector3 pos = Agent ? Agent.transform.position : transform.position;
                Pivots P = GetComponentInChildren<Pivots>();
                pos.y = P.transform.position.y;

                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, ToRun);

                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, ToTrot);

                if (Agent)
                {
                    UnityEditor.Handles.color = Color.red;
                    UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, Agent.stoppingDistance);
                }
            }
        }

#endif
    }
}

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
        private RelativeHeadRotate headRotation;
        #endregion

        #region Target Verifications
        protected ActionZone Target_is_ActionZone; // To check if the Target is an Action Zone
        protected MWayPoint Target_is_Waypoint; // To check if the Target is a Way Point
        #endregion

        [HideInInspector]
        public Transform target; // The target to path to.
        public Transform deltaTarget; // Used to check if the Target has changed
        private Transform closestGrabableItem;
        public GameObject animalHead; // To give other scripts that use it a universal reference to the head transform.
        public bool AutoSpeed = true;
        public float ToTrot = 6f;
        public float ToRun = 8f;
        private float interruptTimer;
        private NavMeshPath path;
        private Vector3 pathEndDestination;

        public bool debug = true;                   // Debugging 
        public bool isWandering = false;
        bool isInActionState; // Check if is making any Animation Action
        bool StartingAction; // Check if Start the animation  
        bool sawTarget; // After changing targets, have we ever had line of sight between the fox and the target?
        bool targetOverride;
        bool isFixingTheAgentDisplacement;
        bool cannotPathToTarget;
        bool performingHeadOverride;

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
#if UNITY_EDITOR
            UpdateTarget();
#endif
        }

        /// <summary>
        /// Initialize the Ai Animal Control Values
        /// </summary>
        protected virtual void StartAgent()
        {
            path = new NavMeshPath();
            wanderTimer = 3;
            timer = wanderTimer;
            animal = GetComponent<Animal>();
            animalAnimator = GetComponent<Animator>();
            headRotation = GetComponent<RelativeHeadRotate>();
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

            if (!Agent.isOnNavMesh || !Agent.enabled || interruptTimer > timer)
            {
                return;
            }

            if ((Agent.nextPosition - transform.position).sqrMagnitude > 1f && !isFixingTheAgentDisplacement)
            {
                isFixingTheAgentDisplacement = true;
                StartCoroutine(FixAgentDisplacement());
            }

            UpdateAgent();

            if (!(isWandering || cannotPathToTarget || sawTarget))
            {
                CheckSightlineToTarget(); // This is something that can be just fine with a margin of error of .3 seconds to first seeing something. So if performance is lagging, feel free to cut corners here.
            }

#if UNITY_EDITOR
            UpdateTarget();
#endif
        }

        IEnumerator FixAgentDisplacement()
        {
            yield return new WaitForSeconds(.5f);
            if ((Agent.nextPosition - transform.position).sqrMagnitude > 1f)
            {
                Agent.Warp(Agent.transform.position);
            }
            isFixingTheAgentDisplacement = false;
        }

        void UpdateTarget()
        {
#if UNITY_EDITOR
            if (target != deltaTarget)
            {
                SetTarget(deltaTarget);
            }
#endif
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

                if (!isInActionState)
                {
                    StartingAction = false; // This whole function is largely vestigial. It still has one function that might be necessary, so it stays for now. For now
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
                Agent.CalculatePath(target.position, path);

                if (path.corners.Length > 0)
                {
                    pathEndDestination = path.corners[path.corners.Length - 1];
                }
                else
                {
                    pathEndDestination = transform.position;
                }

                if ((pathEndDestination - target.position).sqrMagnitude > .04f)
                {
                    if (!cannotPathToTarget)
                    {
                        cannotPathToTarget = true;
                        Agent.SetDestination(pathEndDestination);
                        SetStoppingDistance();
                        StartCoroutine(PathingTimeOut());
                    }
                }
                else
                {
                    Agent.path = path;

                    if (cannotPathToTarget)
                    {
                        cannotPathToTarget = false;
                        SetStoppingDistance();
                    }
                }
            }

            if (Agent.remainingDistance > Agent.stoppingDistance)
            {
                Direction = Agent.desiredVelocity.normalized;
                isMoving = true;
                StartingAction = false;
            }
            else
            {
                if (Target_is_ActionZone && !StartingAction) // If the Target is an Action Zone Start the Action
                {
                    StartingAction = true;
                }
                else if (Target_is_Waypoint && isMoving)
                {
                    SetTarget(Target_is_Waypoint.NextWaypoint.transform);
                }
            }

            animal.Move(Direction);                                 //Set the Movement to the Animal

            if (AutoSpeed) AutomaticSpeed();                         //Set Automatic Speeds

            CheckOffMeshLinks();                                     //Jump/Fall behaviour 
        }

        IEnumerator PathingTimeOut()
        {
            yield return new WaitForSeconds(14f); // If the fox can't figure out where it's going in 14 seconds, then it ain't happening.
            if (cannotPathToTarget)
            {
                TriggerHeadOverride(1f, 3.5f, 1f);
                InterruptPathing(6f);
                isWandering = true;
                yield return new WaitForSeconds(2f);
                GetComponent<FoxSounds>().VoiceFox(5);
                yield return new WaitForSeconds(2.5f);
            }
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

        void SetStoppingDistance()
        {
            if (cannotPathToTarget)
            {
                Agent.stoppingDistance = Mathf.Abs(pathEndDestination.y - target.position.y) + 1f;
            }
            else if (Target_is_ActionZone)
            {
                Target_is_ActionZone.enabled = true;
                Agent.stoppingDistance = Target_is_ActionZone.stoppingDistance;
            }
            else if (Target_is_Waypoint)
            {
                Agent.stoppingDistance = Target_is_Waypoint.StoppingDistance;
            }
            else
            {
                Agent.stoppingDistance = DefaultStoppingDistance;
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
                    Target_is_ActionZone = target.GetComponent<ActionZone>();
                    Target_is_Waypoint = target.GetComponent<MWayPoint>();
                    SetStoppingDistance();
                }
                else
                {
                    isWandering = true;
                    Target_is_ActionZone = null;
                    Target_is_Waypoint = null;
                    Agent.stoppingDistance = DefaultStoppingDistance;
                }

                sawTarget = false;

#if UNITY_EDITOR
                deltaTarget = target;
#endif
            }
        }

        public void SetClosestGrabbableItem(Transform item)
        {
            closestGrabableItem = item;
        }

        public Transform GetClosestGrabableItem()
        {
            return closestGrabableItem;
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
            Physics.Raycast(new Ray(animalHead.transform.position, directionToTarget), out hit, 10f, sawTargetLayerMask, QueryTriggerInteraction.Ignore);

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

        public void TriggerJawOverride(float newJawWeight, float newJawOffset = 0)
        {
            StartCoroutine(JawOverride(newJawWeight, newJawOffset));
        }

        IEnumerator JawOverride (float newJawWeight, float newJawOffset, string animName = "JawOpen")
        {
            float jawTimer = 0;
            float transitionTime = .5f;

            newJawWeight = Mathf.Clamp(newJawWeight, 0, 1f);
            float oldJawWeight = animalAnimator.GetLayerWeight(3);
            float weightDifference = newJawWeight - oldJawWeight;

            if (newJawWeight == 0)
            {
                newJawOffset = 0;
            }
            float oldJawOffset = headRotation.jawOffset;
            float offsetDifference = newJawOffset - oldJawOffset;

            if (oldJawWeight == 0)
            {
                animalAnimator.SetBool(animName, true);
            }

            while (jawTimer > transitionTime)
            {
                jawTimer = jawTimer + Time.deltaTime;
                animalAnimator.SetLayerWeight(3, oldJawWeight + ((weightDifference * jawTimer) / transitionTime));
                headRotation.jawOffset = oldJawOffset + (offsetDifference * jawTimer) / transitionTime;
                yield return new WaitForEndOfFrame();
            }
            animalAnimator.SetLayerWeight(3, newJawWeight);
            headRotation.jawOffset = newJawOffset;

            if (newJawWeight == 0)
            {
                animalAnimator.SetBool(animName, false);
            }
        }

        public void TriggerHeadOverride(float inTime, float stayTime, float outTime, string animName = "HeadShake")
        {
            StartCoroutine(HeadOverride(inTime, stayTime, outTime, animName));
        }

        IEnumerator HeadOverride(float inTime, float stayTime, float outTime, string animName)
        {
            if (!performingHeadOverride)
            {
                performingHeadOverride = true; // I want to make damn well certain this isn't going to trigger multiple times in a row.
                float headTimer = 0;
                animalAnimator.SetBool(animName, true);
                while (headTimer < inTime)
                {
                    Debug.Log("This is only going to trigger once, probably");
                    headTimer = headTimer + Time.deltaTime;
                    animalAnimator.SetLayerWeight(2, headTimer / inTime);
                    yield return new WaitForEndOfFrame();
                }
                headTimer = 0;
                animalAnimator.SetLayerWeight(2, 1f);

                yield return new WaitForSeconds(stayTime);

                while (headTimer < outTime)
                {
                    headTimer = headTimer + Time.deltaTime;
                    animalAnimator.SetLayerWeight(2, 1f - (headTimer / outTime));
                    yield return new WaitForEndOfFrame();
                }
                animalAnimator.SetLayerWeight(2, 0f);
                animalAnimator.SetBool(animName, false);
                performingHeadOverride = false;
            }
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

using MalbersAnimations.Utilities;
using System.Collections;
using UnityEngine;
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
        protected Rigidbody target_Has_Rigidibody;
        #endregion

        public Transform target; // The target to path to.
        public Transform deltaTarget; // Used to check if the Target has changed
        private Transform closestGrabableItem;
        public GameObject animalHead; // To give other scripts that use it a universal reference to the head transform.
        public bool AutoSpeed = true;
        public float ToTrot = 6f, ToRun = 8f;
        private float interruptTimer, pathingTimer;
        private NavMeshPath path;
        private Vector3 pathEndDestination;

        PathingCheckStates currentPathingState = PathingCheckStates.NotChecking;
        float pathingCheckEndTime = 0;
        float pathingCheckSubTime = 0;

        public bool debug = true;                   // Debugging 
        public bool isWandering = false;

        private float targetJawWeight = 0, targetJawOffset = 0;

        #region State Bools
        bool isInActionState; // Check if is making any Animation Action
        bool StartingAction; // Check if Start the animation  
        bool sawTarget; // After changing targets, have we ever had line of sight between the fox and the target?
        bool targetOverride;
        bool isFixingTheAgentDisplacement;
        bool cannotPathToTarget;
        bool performingHeadOverride;
        bool destinationSet;
        #endregion

        protected int sawTargetLayerMask = (1 << 8) | (1 << 0);// This makes a layermask using the number 8 because that is the layer that I've put all the props on.

        enum MovementStates
        {
            NormalMovement = 0,
            AlwaysWalk = 1,
            AlwaysRun = 2,
            SlowWalk = 3,
            FastRun = 4
        }
        enum PathingCheckStates
        {
            NotChecking,
            ProvingPresence,
            ProvingAbsence,
            WaitingToCancel
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
            DefaultStoppingDistance = Agent.stoppingDistance; // Save the Stopping Distance.
        }

        void Update()
        {
#if UNITY_EDITOR
            UpdateTarget();

            //for (int i = 0; i < animalAnimator.GetCurrentAnimatorClipInfo(0).Length; i++)
            //{
            //    Debug.Log(animalAnimator.GetCurrentAnimatorClipInfo(0)[i].clip);
            //}

            //Debug.Log(animalAnimator.GetCurrentAnimatorStateInfo(0).);

            //for (int i = 0; i < animalAnimator.GetCurrentAnimatorStateInfo(0).length; i++)
            //{
            //    Debug.Log(animalAnimator.GetCurrentAnimatorStateInfo(0));
            //}
#endif

            if (transform.position.y < -20f)
            { // This is a simple check to see if the Fox has managed to clip entirely out of the playable area and fall into the void.
                transform.position = new Vector3(0, 3f, 0);
            }

            DisableAgent();
            TryActionZone();
            timer += Time.deltaTime;
            Agent.nextPosition = agent.transform.position; // Update the Agent Position to the Transform position

            if (!Agent.isOnNavMesh || !Agent.enabled)
            {
                animalAnimator.speed = 1f;
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
            if (target != deltaTarget)
            {
                SetTarget(deltaTarget);
            }
        }

        /// <summary>
        /// This will disable the Agent when is not on Idle or Locomotion 
        /// </summary>
        void DisableAgent()
        {
            if (!Agent.enabled && (animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Idle")))          //Activate the Agent when the animal is moving
            {
                Agent.enabled = true;
                isMoving = false; // Important.
            }
            else if (Agent.enabled && !(animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Idle")))
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
                if ((Agent.destination - target.position).sqrMagnitude > .121 && !cannotPathToTarget)
                { // What we want here is to trigger once each time respectively...
                    if (destinationSet && pathingTimer < Time.time)
                    { // I'm fine with being a little spammy, as long as we keep the spam down to a nice moderate minimum... Once every 3/4ths of a second or so probably fits the bill.
                        Agent.SetDestination(target.position);
                        pathingTimer = Time.time + .75f;
                    }

                    if (!destinationSet)
                    {
                        Agent.SetDestination(target.position); // First we must always give the Agent a chance to path, since the call is asynchronous (better performance that way, I say). Roughly about a second and a half should be sufficient for our purposes.
                        SetStoppingDistance();
                        destinationSet = true;
                        currentPathingState = PathingCheckStates.ProvingPresence;
                        pathingCheckEndTime = Time.time + 12f; // This is a sort of cut-off to prevent EvaluatePathing from just looping indefinitely.
                        pathingCheckSubTime = Time.time + 4f;
                    }
                }
                else if ((Agent.destination - target.position).sqrMagnitude <= .121 && cannotPathToTarget)
                {

                    if (destinationSet)
                    {
                        destinationSet = false;
                        SetStoppingDistance();

                        if (path.corners.Length > 0)
                        {
                            pathEndDestination = path.corners[path.corners.Length - 1];
                        }
                        else
                        {
                            pathEndDestination = transform.position;
                        }
                        Agent.SetDestination(pathEndDestination);
                    }

                } // So, the limitations of this code are thusly: If a player throws a grabbable object in a location that cannot be pathed to and then hits that object with another object so that it's pushed back to a pathable location, we can't detect that. It's possible to fix this by using a second AIPathingAgent to check this (better than using calculate path, since that produces unreliable results, and it always fully computes the path in that frame... performance issues.) If we only wake up the second agent when there are pathing issues, it won't be a problem. I think.

                if (debug && Agent.path.corners.Length > 1)
                {
                    for (int i = 0; i < Agent.path.corners.Length - 1; i++)
                    {
                        Debug.DrawLine(Agent.path.corners[i], Agent.path.corners[i + 1], Color.red);
                    }
                }

                if (currentPathingState != PathingCheckStates.NotChecking)
                {
                    EvaluatePathing();
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

            if (interruptTimer > timer)
            {
                Direction = Vector3.zero;
            }

            animal.Move(Direction);                                 //Set the Movement to the Animal

            if (AutoSpeed)
            {
                AutomaticSpeed();                         //Set Automatic Speeds
            }
            
            CheckOffMeshLinks();                                     //Jump/Fall behaviour 
        }

        private void EvaluatePathing()
        {
            if (currentPathingState == PathingCheckStates.ProvingPresence)
            {
                if ((target_Has_Rigidibody && target_Has_Rigidibody.velocity.sqrMagnitude > .25f) || animal.CurrentAnimState.IsTag("Action"))
                {
                    pathingCheckEndTime += Time.deltaTime;
                    pathingCheckSubTime += Time.deltaTime;
                }
                else if (Agent.path.corners.Length > 1 && (Agent.path.corners[Agent.path.corners.Length - 1] - target.position).sqrMagnitude < .121f)
                {
                    currentPathingState = PathingCheckStates.ProvingAbsence;
                    pathingCheckSubTime = Time.time + 16f; // This right here is fairly important. It's a hard limit of 16 seconds on arriving at any target in the room. This might need to be adjusted if the fox is moving faster or slower than usual. But, I think, if this is the case then we might be losing the attention of the players...
                }
                else if (pathingCheckSubTime < Time.time || pathingCheckEndTime < Time.time)
                {
                    cannotPathToTarget = true;
                    currentPathingState = PathingCheckStates.WaitingToCancel;
                    pathingCheckSubTime = Time.time + 8f;
                }
            }
            else if (currentPathingState == PathingCheckStates.ProvingAbsence)
            {
                if (!(animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Jump") || animal.CurrentAnimState.IsTag("Recover")))
                {
                    pathingCheckSubTime += Time.deltaTime;
                }

                if ((Agent.path.corners[Agent.path.corners.Length - 1] - target.position).sqrMagnitude > .121f)
                {
                    currentPathingState = PathingCheckStates.ProvingPresence;
                    pathingCheckSubTime = Time.time + 4f;
                }
                else if (pathingCheckSubTime < Time.time)
                {
                    cannotPathToTarget = true;
                    currentPathingState = PathingCheckStates.WaitingToCancel;
                    pathingCheckSubTime = Time.time + 8f;
                }
            }
            else if (currentPathingState == PathingCheckStates.WaitingToCancel)
            {
                if (pathingCheckSubTime < Time.time || Agent.isOnNavMesh && Agent.remainingDistance < Agent.stoppingDistance)
                {
                    TriggerHeadOverride(1f, 2.5f, 1f);
                    InterruptPathing(4.3f);
                    SetTarget(null, true, true);
                    StartCoroutine(FoxVoiceDelay());
                }
            }
        }

        IEnumerator FoxVoiceDelay()
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
            GetComponent<FoxSounds>().VoiceFox(5);
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
                if (Agent.remainingDistance < ToTrot / 2f)
                { // Turns out, if the animal is always running, the fox can end up orbiting anything with a stopping distance of .63 or lower. It's difficult to trigger, but it can happen.
                    animal.Speed1 = true;
                }
                else
                {
                    animal.Speed3 = true;
                }
            }

            if (currentMovementState == MovementStates.FastRun)
            {
                animalAnimator.speed = 2f;
            }
            else if (currentMovementState == MovementStates.SlowWalk)
            {
                animalAnimator.speed = .7f;

                //if (Agent.remainingDistance < 2f)
                //{
                //    animalAnimator.speed = .3f;
                //}
                //else
                //{ // The following is a simple linear equation to ensure that animalAnimator.Speed = 1 at 4 or higher, and .3 at 2 or lower.
                //    animalAnimator.speed = Mathf.Min(1f, (Agent.remainingDistance * .35f) - .4f);
                //}
            }
            else
            {
                animalAnimator.speed = 1f;
            }
        }

        /// <summary>
        /// Set to next Target
        /// </summary>
        public void SetTarget(Transform target, bool ignoreOverride = false, bool resetOverride = false)
        {
            ActionZone targetActionZone = null;

            if (target)
            { // It's a little bit silly having this out here, but I need to make sure that onEnable is always invoked on actionzones when this is called, even if the fox never actually goes for the stated object.
                targetActionZone = target.GetComponent<ActionZone>();

                if (targetActionZone)
                {
                    targetActionZone.onEnable.Invoke();
                }
            }

            if (!targetOverride || ignoreOverride)
            {
                if (target == null || resetOverride)
                {
                    targetOverride = false;
                    if (currentMovementState != MovementStates.NormalMovement)
                    {
                        ChangeMovement(0); // This is a failsafe. I'm specifically thinking of the JackInTheBox here, because if the fox sees the JackInTheBox but it's in an unpathable location then the fox might just move everywhere slowly. Thassa no good.
                    }
                }

                isMoving = destinationSet = sawTarget = cannotPathToTarget = false;
                this.target = target;

                if (Target_is_ActionZone)
                {
                    Target_is_ActionZone.readyToTrigger = false;
                }

                if (target)
                {
                    isWandering = false;
                    Target_is_ActionZone = targetActionZone;
                    Target_is_Waypoint = target.GetComponent<MWayPoint>();
                    target_Has_Rigidibody = target.GetComponent<Rigidbody>();

                    if (Target_is_ActionZone)
                    {
                        Target_is_ActionZone.readyToTrigger = true;
                    }
                }
                else
                {
                    isWandering = true;
                    Target_is_ActionZone = null;
                    Target_is_Waypoint = null;
                    Agent.stoppingDistance = DefaultStoppingDistance;
                }

#if UNITY_EDITOR
                deltaTarget = this.target;
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

        public void SetTargetOverride(bool newOverrideState)
        {
            targetOverride = newOverrideState;
        }

        public void TriggerJawOverride(float newJawWeight, float newJawOffset = 0)
        {
            StartCoroutine(JawOverride(newJawWeight, newJawOffset));
        }

        IEnumerator JawOverride(float newJawWeight, float newJawOffset, float transitionTime = .5f, string animName = "JawOpen")
        {
            float jawTimer = 0;

            float oldJawWeight = targetJawWeight;
            targetJawWeight = newJawWeight = Mathf.Clamp(newJawWeight, 0, 1f);
            float weightDifference = newJawWeight - oldJawWeight;

            if (newJawWeight == 0)
            {
                newJawOffset = 0;
            }
            float oldJawOffset = targetJawOffset;
            targetJawOffset = newJawOffset;
            float offsetDifference = newJawOffset - oldJawOffset;

            if (oldJawWeight == 0)
            {
                animalAnimator.SetBool(animName, true);
            }

            while (jawTimer < transitionTime)
            {
                jawTimer += Time.deltaTime;
                animalAnimator.SetLayerWeight(3, oldJawWeight + ((weightDifference * jawTimer) / transitionTime));
                headRotation.jawOffset = oldJawOffset + ((offsetDifference * jawTimer) / transitionTime);
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

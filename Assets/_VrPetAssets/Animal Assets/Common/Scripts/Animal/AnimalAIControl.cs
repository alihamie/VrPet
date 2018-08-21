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

        public Transform target; // The target to path to.
        public Transform deltaTarget; // Used to check if the Target has changed
        private Transform closestGrabableItem;
        public GameObject animalHead; // To give other scripts that use it a universal reference to the head transform.
        public bool AutoSpeed = true;
        public float ToTrot = 6f, ToRun = 8f;
        private float interruptTimer;
        private NavMeshPath path;
        private Vector3 pathEndDestination;

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
#endif
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
                if ((Agent.destination - target.position).sqrMagnitude > .121 && !cannotPathToTarget && !destinationSet)
                { // What we want here is to trigger once each time respectively...
                    destinationSet = true;
                    Debug.Log("A");
                    Agent.SetDestination(target.position); // First we must always give the Agent a chance to path, since the call is asynchronous (better performance that way, I say). Roughly about a second and a half should be sufficient for our purposes.
                    SetStoppingDistance();
                    StartCoroutine(PathingTimeOut()); // A simple delayed check to see if we're actually pathing to the target, or just twiddling our thumbs.
                }
                else if ((Agent.destination - target.position).sqrMagnitude <= .121 && cannotPathToTarget && destinationSet)
                {
                    Debug.Log("B");
                    destinationSet = false;
                    if (path.corners.Length > 0)
                    {
                        pathEndDestination = path.corners[path.corners.Length - 1];
                    }
                    else
                    {
                        pathEndDestination = transform.position;
                    }

                    Agent.SetDestination(pathEndDestination);
                    SetStoppingDistance();
                } // So, the limitations of this code are thusly: If a player throws a grabbable object in a location that cannot be pathed to and then hits that object with another object so that it's pushed back to a pathable location, we can't detect that. It's possible to fix this by using a second AIPathingAgent to check this (better than using calculate path, since that produces unreliable results, and it always fully computes the path in that frame... performance issues.) If we only wake up the second agent when there are pathing issues, it won't be a problem. I think.

                if (debug && Agent.path.corners.Length > 1)
                {
                    for (int i = 0; i < Agent.path.corners.Length - 1; i++)
                    {
                        Debug.DrawLine(Agent.path.corners[i], Agent.path.corners[i + 1], Color.red);
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
            bool arrivedAtPseudoTarget = false;
            bool weHaveAPath = false;
            float checkingTimeLimit = 2.5f;
            float checkingTime = 0;
            float timeOutLimit = 12f;
            float timeOut = 0;
            
            // So if we fail to get a path in 4 seconds, we're done. But if we do, and that path remains valid for another fours seconds, we're home clear. But if we get a valid path, lose it, and then regain it we should register that properly, with a cutoff of something like 12-18 seconds in case we've managed to create a perfect loop.
            // Of course, if the fox is animating, then we're already there and we don't have to bother with checking any of this.
            while (!(weHaveAPath || animal.CurrentAnimState.IsTag("Action")))
            {
                while (!(checkingTime > checkingTimeLimit || weHaveAPath || animal.CurrentAnimState.IsTag("Action") || timeOut > timeOutLimit))
                {
                    checkingTime += Time.fixedDeltaTime;
                    if (Agent.path.corners.Length > 1)
                    {
                        if ((Agent.path.corners[Agent.path.corners.Length - 1] - target.position).sqrMagnitude < .121f)
                        {
                            weHaveAPath = true;
                        }
                    }
                    yield return new WaitForFixedUpdate();
                }

                if (!weHaveAPath)
                { // Basically, if we've failed to acquire a path in four seconds there's no point to go on, so we go to this particular dead-end cul de sac.
                    Debug.Log("C");
                    cannotPathToTarget = true;
                    checkingTime = 0;
                    checkingTimeLimit = 8f;
                    while (!(checkingTime > checkingTimeLimit || arrivedAtPseudoTarget))
                    {
                        if (!cannotPathToTarget)
                        { // The only way this is going to be true is if we set a target after it was registered as unpathable to. In which case, we need to clear older versions of this coroutine away to make way for a new coroutine. This'll work as long as the increments of this while loop are less than the checking time.
                            yield break;
                        }
                        checkingTime += .5f;
                        if (Agent.isOnNavMesh)
                        {
                            if (Agent.remainingDistance < Agent.stoppingDistance) // Just a quick check to see if we've arrived.
                            {
                                arrivedAtPseudoTarget = true;
                            }
                        }
                        yield return new WaitForSeconds(.5f);
                    }

                    if (cannotPathToTarget)
                    {
                        TriggerHeadOverride(1f, 2.5f, 1f);
                        InterruptPathing(4.3f);
                        SetTarget(null);
                        yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
                        GetComponent<FoxSounds>().VoiceFox(5);
                    }
                    yield break;
                }

                timeOut += checkingTime;
                checkingTime = 0;

                while (!(checkingTime > checkingTimeLimit || !weHaveAPath || animal.CurrentAnimState.IsTag("Action")))
                {
                    checkingTime += Time.fixedDeltaTime;
                    if (Agent.path.corners.Length > 1)
                    {
                        if ((Agent.path.corners[Agent.path.corners.Length - 1] - target.position).sqrMagnitude > .121f)
                        {
                            weHaveAPath = false;
                        }
                    }
                    yield return new WaitForFixedUpdate();
                }
                timeOut += checkingTime;
                checkingTime = 0;
                yield return new WaitForFixedUpdate();
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
        public void SetTarget(Transform target, bool ignoreOverride = false, bool resetOverride = false)
        {
            if (!targetOverride || ignoreOverride)
            {
                if (resetOverride)
                {
                    targetOverride = false;
                    if (currentMovementState != MovementStates.NormalMovement)
                    {
                        ChangeMovement(0); // This is a failsafe. I'm specifically thinking of the JackInTheBox here, because if the fox sees the JackInTheBox but it's in an unpathable location then the fox might just move everywhere slowly. Thassa no good.
                    }
                }
                isMoving = destinationSet = sawTarget = cannotPathToTarget = false;

                this.target = target;

                if (target)
                {
                    isWandering = false;
                    Target_is_ActionZone = target.GetComponent<ActionZone>();
                    Target_is_Waypoint = target.GetComponent<MWayPoint>();
                    SetStoppingDistance();
                }
                else
                {
                    isWandering = true;
                    targetOverride = false; // This should mean that whenever the fox is wandering, it'll have targetOverride shut off. Just in case.
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

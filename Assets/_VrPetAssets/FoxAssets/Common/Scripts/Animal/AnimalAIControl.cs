using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

namespace MalbersAnimations
{
    public class AnimalAIControl : MonoBehaviour
    {
        #region Components References
        private NavMeshAgent agent;                 //The NavMeshAgent
        protected Animal animal;                    //The Animal Script
        #endregion

        #region Target Verifications
        protected Animal Target_is_Animal;          //To check if the target is an animal
        protected ActionZone Target_is_ActionZone;  //To check if the Target is an Action Zone
        protected MWayPoint Target_is_Waypoint;     //To check if the Target is a Way Point
        protected bool Target_is_DropArea;
        #endregion

        public Transform target;                    //The Target
        Transform deltaTarget;                      //Used to check if the Target has changed
        private Transform closestGrabableItem;
        public bool AutoSpeed = true;               
        public float ToTrot = 6f;
        public float ToRun = 8f;

        public bool debug = true;                //Debuging 
        public bool isWandering = false;

        bool isInActionState;                     //Check if is making any Animation Action
        bool StartingAction;                      //Check if Start the animation  

        protected float DefaultStoppingDistance;
        Transform NextTarget;
       

        /// <summary>
        /// Important for changing Waypoints
        /// </summary>
        private bool isMoving = false;

        protected float RemainingDistance;

        private float wanderTimer;
        private float timer;

        /// <summary>
        /// the navmeshAgent asociate to this GameObject
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


        void Start(){ StartAgent();  }

        /// <summary>
        /// Initialize the Ai Animal Control Values
        /// </summary>
        protected virtual void StartAgent()
        {
            wanderTimer = 3;
            timer = wanderTimer;
            animal = GetComponent<Animal>();
            Agent.updateRotation = false;
            Agent.updatePosition = false;
            isWandering = true;
            DefaultStoppingDistance = Agent.stoppingDistance; //Save the Stoping Distance
        }

        void Update()
        {
            DisableAgent();                                             
            TryActionZone();
            timer += Time.deltaTime;
            Agent.nextPosition = agent.transform.position;                      //Update the Agent Position to the Transform position


            if (!Agent.isOnNavMesh || !Agent.enabled) return;
            if (target == null) return;

            UpdateAgent();
            UpdateTarget();
        }

        //If the target changes do something
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
                //if (debug) Debug.Log("Target Updated: "+ target.name);

                Target_is_Animal = deltaTarget ? deltaTarget.GetComponent<Animal>() : null;
                Target_is_ActionZone = deltaTarget ? deltaTarget.GetComponent<ActionZone>() : null;
                Target_is_Waypoint = deltaTarget ? deltaTarget.GetComponent<MWayPoint>() : null;
                Target_is_DropArea = deltaTarget ? deltaTarget.tag == "DropArea" : false;
                Agent.stoppingDistance = DefaultStoppingDistance;

                if (Target_is_ActionZone)
                {
                    Target_is_ActionZone.enabled = true;
                    Agent.stoppingDistance = Target_is_ActionZone.stoppingDistance;
                }
                else if (Target_is_Waypoint)
                {
                    Agent.stoppingDistance = Target_is_Waypoint.StoppingDistance;
                }

                if (Target_is_DropArea)
                {
                    Agent.stoppingDistance = 0.1f;
                }
            }
        }

        /// <summary>
        /// This will disable the Agent when is not on Idle or Locomotion 
        /// </summary>
        void DisableAgent()
        {
            if ((animal.CurrentAnimState.IsTag("Locomotion") 
                || animal.CurrentAnimState.IsTag("Idle")))          //Activate the Agent when the animal is moving
            {
                if (!Agent.enabled)
                {
                    Agent.enabled = true;
                   if(debug) Debug.Log("Enable Agent. Animal " + name + " is Moving");
                    isMoving = false;                               //Important
                }
            }
            else
            {
                if (Agent.enabled)                      //Disable the Agent whe is not on Locomotion or Idling
                {
                    Agent.enabled = false;
                    if (debug) Debug.Log("Disable Agent. Animal "+ name +" is doing an action, jumping or falling");
                }
            }

            if (animal.IsInAir) //Don't rotate if is in the middle of a jump
            {
                animal.Move(Vector3.zero);
            }
        }

        /// <summary>
        /// Manage Everytime the animal enters in an Action Zone
        /// </summary>
        void TryActionZone()
        {
            if (isInActionState != animal.CurrentAnimState.IsTag("Action"))  //If the animal is Executing an action Save it
            {
                isInActionState = animal.CurrentAnimState.IsTag("Action");   //Store is there was any change on the action 

                if (isInActionState)
                {
                  if (Target_is_ActionZone && Target_is_ActionZone.NextTarget)
                        SetTarget(Target_is_ActionZone.NextTarget);
                }
                else
                {
                    StartingAction = false;
                    animal.ActionID = -1;                       //Reset the Action
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

            if(isWandering)
            {
                if (timer >= wanderTimer)
                {
                    pos = RandomNavSphere(transform.position, 300, -1);
                    timer = 0;
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

            RemainingDistance = Agent.remainingDistance;

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
                    animal.Action = true;                           //Activate the Action on the Animal
                }
                if (Target_is_Waypoint && isMoving)
                {
                    SetTarget(Target_is_Waypoint ? Target_is_Waypoint.NextWaypoint.transform : null);
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
                        animal.Action = true;                           //Activate the Action on the Animal
                        return;
                    }
                }

                if (CurrentOffmeshLink_Data.linkType == OffMeshLinkType.LinkTypeManual)
                {
                    Transform NearTransform = CurrentOffMeshLink.startTransform;

                    if (CurrentOffMeshLink.endTransform.position == CurrentOffmeshLink_Data.startPos) //Verify the start point of the OffMeshLink
                    { NearTransform = CurrentOffMeshLink.endTransform; }

                    StartCoroutine(MalbersTools.AlignTransformsC(transform, NearTransform, 0.5f, false, true)); //Aling the Animal to the Link

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

        /// <summary>
        /// Set to next Target
        /// </summary>
        public void SetTarget(Transform target)
        {
            isWandering = false;
            this.target = target;
            isMoving = false;
            UpdateTarget();
        }

        public void SetClosestGrabbableItem(Transform item)
        {
            this.closestGrabableItem = item;
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
#if UNITY_5_6_OR_NEWER
                Agent.isStopped = true;
#else
            Agent.Stop();
#endif
        }

        /// <summary>
        /// Resume the movement along the current path after a pause
        /// </summary>
        protected void Agent_Resume()
        {
#if UNITY_5_6_OR_NEWER
            Agent.isStopped = false;
#else
            Agent.Resume();
#endif
        }

        //Toogle Off and On the Agent
        IEnumerator ToogleAgent()
        {
            Agent.enabled = false;
            yield return null;
            Agent.enabled = true;
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

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    [RequireComponent(typeof(BoxCollider))]
    public class ActionZone : MonoBehaviour
    {
        static Keyframe[] K = { new Keyframe(0, 0), new Keyframe(1, 1) };

        public Actions actionsToUse;

        public int ID;                                  //ID of the Action Zone (Value)
        public int index;                               //Index of the Action Zone (List index)
        public bool HeadOnly;                           //Use the Trigger for heads only

        public bool Align;                              //Align the Animal entering to the Aling Point
        public Transform AlingPoint;
        public float AlignTime = 0.5f;
        public AnimationCurve AlignCurve = new AnimationCurve(K);

        public bool AlignPos = true, AlignRot = true, AlignLookAt = false;
        bool firstTimeTrigger;

        public UnityEvent onGrab = new UnityEvent();
        public UnityEvent onEnable = new UnityEvent();
        public UnityEvent onEnd = new UnityEvent();
        public UnityEvent onAction = new UnityEvent();
        public UnityEvent onSight = new UnityEvent();

        private EasyInputVR.StandardControllers.StandardGrabReceiver grabReceiver;

#if UNITY_EDITOR
        public bool invokeGrab, invokeEnable, invokeEnd, invokeAction, invokeSight;
#endif
        //───────AI───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        public float stoppingDistance = 0.5f;
        public Transform NextTarget;

        private void Start()
        {
            grabReceiver = GetComponent<EasyInputVR.StandardControllers.StandardGrabReceiver>();
        }

        void OnEnable()
        {
            onEnable.Invoke();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (firstTimeTrigger && collision.gameObject.layer == 20)
            {
                Physics.IgnoreLayerCollision(20, 8);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (!enabled || firstTimeTrigger)
            {
                return;
            }

            Animal animal = other.GetComponentInParent<Animal>();
            AnimalAIControl animalAIControl = other.GetComponentInParent<AnimalAIControl>();

            if (animal && other.gameObject.layer == 20) // In short, the entering object needs to have the Animal and AnimalAIControl scripts attached and be on the Animal Layer for us to do anything.
            {
                if (animalAIControl)
                {
                    if (animalAIControl.isWandering == true)
                    {
                        return; // If the AnimalAIControl script is attached, but the animal is wandering, also do nothing.
                    }
                }

                if (HeadOnly && !other.name.ToLower().Contains("head"))
                {
                    return; // Finally, if HeadOnly is enabled then the collider needs to be attached to a head to trigger anything.
                }
            }
            else
            {
                return;
            }

            StartCoroutine(startAnimation(ID, animal, animalAIControl));
        }

        IEnumerator startAnimation(int id, Animal animal, AnimalAIControl ai) // Here's where we make the Fox do a dance... figuratively.
        {
            firstTimeTrigger = true; // This only needs to trigger once. Can't just set enabled to false because that'd turn the coroutines off. Or so I think. Haven't tested it, only read it on the internet. But when has the internet ever lied to me?
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if (tag == "GrabableItem" && ai)
            {
                ai.SetClosestGrabbableItem(transform);

                if (id == 25) // If we're trying to grab the object, we need to be faster if the object is going faster.
                {
                    if (rigidbody.velocity.sqrMagnitude > .25f || rigidbody.angularVelocity.sqrMagnitude > .09)
                    {
                        id = 19;
                    }
                }
            }

            while (!animal.CurrentAnimState.IsTag("Idle") || Mathf.Abs(animal.transform.position.y - transform.position.y) > .12f)
            { // This is a final sanity check to make sure that we haven't managed to fall or jump away from the object after pathing into it's trigger.
                if (ai)
                {
                    if (ai.target != transform)
                    { // I'm kinda suprised I didn't think to put this check in earlier.
                        enabled = false;
                        Physics.IgnoreLayerCollision(20, 8, false);
                        yield break;
                    }
                    else if (grabReceiver)
                    {
                        if (grabReceiver.grabMode)
                        {
                            enabled = false;
                            Physics.IgnoreLayerCollision(20, 8, false);
                            yield break;
                        }
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

            if (id != -1) // -1 is the id of the attack animation. Which isn't set up as an action, so I think it'll cause some problems if I try to treat it like one.
            {
                animal.ActionEmotion(id);
                animal.EnableAction(true);
            }
            else
            {
                animal.SetAttack();
                if (rigidbody)
                { // This here's to make the object the triggered actionzone is attached to get push away when the fox attacks it.
                    rigidbody.velocity = (transform.position - animal.transform.position + new Vector3(0, 1.2f, 0)).normalized * 5f;
                }
            }

            ActionAlign(animal);

            StartCoroutine(SetNextTarget(animal, ai));
        }

        IEnumerator SetNextTarget(Animal animal, AnimalAIControl ai)
        {
            yield return new WaitForSeconds(.4f);

            while (animal.CurrentAnimState.IsTag("Action") || animal.CurrentAnimState.IsTag("Attack"))
            {
                yield return new WaitForSeconds(.1f);
            }

            Physics.IgnoreLayerCollision(8, 20, false); // Yes, that's right. ALL COLLISION BETWEEN LAYERS 8 and 20. Because otherwise I'd have to record each collision and disable it on a case-by-case basis. Less elegant though. If the CPU cost for doing this on a case-by-case basis isn't too high, I could probably do the more elegant solution.

            if (ai)
            {
                if (ai.target == transform)
                {
                    ai.SetTarget(NextTarget, true);
                }
            }

            onEnd.Invoke();

            animal.ActionEmotion(-1); // Reset the ActionID int parameter for the Animator.
            animal = null; // These three lines are just cleanup for next time, to make sure everything is properly triggered.
            firstTimeTrigger = false;
            enabled = false;
        }

        /// <summary>
        /// Used to align the animal
        /// </summary>
        private void ActionAlign(Animal animal)
        {
            onAction.Invoke();
            if (Align && AlingPoint)
            {
                IEnumerator alignmentCoroutine = null; // I don't know why alignmentCoroutine needs to be set to null first when it always set to something. Probably important though.

                if (AlignLookAt)
                {
                    alignmentCoroutine = Utilities.MalbersTools.AlignLookAtTransform(animal.transform, AlingPoint, AlignTime, AlignCurve);
                }
                else
                {
                    alignmentCoroutine = Utilities.MalbersTools.AlignTransformsC(animal.transform, AlingPoint, AlignTime, AlignPos, AlignRot, AlignCurve);
                }

                StartCoroutine(alignmentCoroutine);
            }
        }

        public override bool Equals(object target)
        {
            if (target is ActionZone)
            {
                ActionZone targetToCopy = (ActionZone)target;
                ID = targetToCopy.ID;
                NextTarget = targetToCopy.NextTarget;
                onSight = targetToCopy.onSight;
                onAction = targetToCopy.onAction;
                onEnd = targetToCopy.onEnd;
                onGrab = targetToCopy.onGrab;
                onEnable = targetToCopy.onEnable;

                BoxCollider myCollider = GetComponent<BoxCollider>();
                BoxCollider copiedCollider = targetToCopy.gameObject.GetComponent<BoxCollider>();

                myCollider.size = copiedCollider.size;
                myCollider.center = copiedCollider.center;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }

        public void CopyActionzone(ActionZone targetToCopy)
        {
            Equals(targetToCopy);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (EditorAI)
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, stoppingDistance);
            }

        }
#endif

        [HideInInspector] public bool EditorShowEvents = true;
        [HideInInspector] public bool EditorAI = true;
        [HideInInspector] public bool EditorShowForce = true, forceEnable = false, forceAction = false;
        [HideInInspector] public bool forceEnd = false, forceSight = false, forceGrab = false;
    }
}
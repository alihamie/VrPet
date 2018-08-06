using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Events;
using UnityEngine.Events;
using System;

namespace MalbersAnimations
{
    [RequireComponent(typeof(BoxCollider))]
    public class ActionZone : MonoBehaviour
    {
        static Keyframe[] K = { new Keyframe(0, 0), new Keyframe(1, 1) };

        public Actions actionsToUse;

        public bool automatic;                          //Set the Action Zone to Automatic
        public int ID;                                  //ID of the Action Zone (Value)
        public int index;                               //Index of the Action Zone (List index)
        public float AutomaticDisabled = 1f;            //is Automatic is set to true this will be the time to disable temporarly the Trigger
        public bool HeadOnly;                           //Use the Trigger for heads only

        public bool Align;                              //Align the Animal entering to the Aling Point
        public Transform AlingPoint;
        public float AlignTime = 0.5f;
        public AnimationCurve AlignCurve = new AnimationCurve(K);

        public bool AlignPos = true, AlignRot = true, AlignLookAt = false;
        bool firstTimeGrab = true, firstTimeTrigger;
        protected List<Collider> _colliders;
        protected Animal animal;
        private AnimalAIControl animalAIControl;

        public UnityEvent onGrab = new UnityEvent();
        public AnimalEvent onEnable = new AnimalEvent();
        public AnimalEvent OnEnter = new AnimalEvent();
        public AnimalEvent OnExit = new AnimalEvent();
        public AnimalEvent OnAction = new AnimalEvent();
        public AnimalEvent OnSight = new AnimalEvent();

        //public static List<ActionZone> ActionZones;

        //───────AI───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        public float stoppingDistance = 0.5f;
        public Transform NextTarget;

        private Collider previousCollider;

        void OnEnable()
        {
            //if (ActionZones == null) ActionZones = new List<ActionZone>();

            //ActionZones.Add(this);          //Save the the Action Zones

            onEnable.Invoke(animal);
        }

        //void OnDisable()
        //{
        //    ActionZones.Remove(this);
        //}

        void OnTriggerEnter(Collider other)
        {
            if (this.enabled == false)
                return;

            Animal animal = other.GetComponentInParent<Animal>();
            AnimalAIControl newAIControl = other.GetComponentInParent<AnimalAIControl>();

            if (newAIControl) animalAIControl = newAIControl;

            if (animalAIControl != null && animalAIControl.isWandering == true)
            {
                return;
            }

            if (other.gameObject.layer != 20)
            {
                return;                           //Just use the Colliders with the Animal Layer on it
            }

            if (!animal)
            {
                return;
            }//If there's no animal script found skip all

            if (_colliders == null)
            {
                _colliders = new List<Collider>();                              //Check all the colliders that enters the Action Zone Trigger
            }

            if (HeadOnly && !other.name.ToLower().Contains("head"))
            {
                return;     //If is Head Only and no head was found Skip
            }

            if (_colliders.Find(item => item == other) == null)                 //if the entering collider is not already on the list add it
            {
                _colliders.Add(other);
            }

            if (animal == this.animal) return;                      //if the animal is the same do nothing
            else
            {
                this.animal = animal;
            }
            //StartCoroutine(startAnimation(animal, id, animalAIControl));
        }

        void OnTriggerStay(Collider other)
        {
            if (enabled == false || !animal) return;

            if ((animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Idle")) && !firstTimeTrigger && _colliders.Count > 0)
            {
                firstTimeTrigger = true;

                if (tag == "GrabableItem") animalAIControl.SetClosestGrabbableItem(transform);

                StartCoroutine(startAnimation(animal, ID, animalAIControl));
            }
            else if ((animal.CurrentAnimState.IsTag("Locomotion") || animal.CurrentAnimState.IsTag("Idle")) && firstTimeTrigger && animalAIControl.target)
            {
                if (animalAIControl.target == transform) animalAIControl.isWandering = true;
            }
        }

        IEnumerator startAnimation(Animal animal, int id, AnimalAIControl ai)
        {
            Rigidbody rigidbody = this.GetComponent<Rigidbody>();

            while (rigidbody != null && rigidbody.velocity.magnitude > 0.2f && ai.Agent.remainingDistance < 0.5f)
            {
                yield return new WaitForEndOfFrame();
            }

            //animal.OnAction.AddListener(OnActionListener);          //Listen when the animal activate the Action Input

            OnEnter.Invoke(animal);

            if (id != -1) animal.ActionEmotion(id);

            if (automatic)       //Just activate when is on the Locomotion State if this is automatic
            {
                if (id != -1)
                {
                    animal.EnableAction(true);
                }
                else
                {
                    animal.SetAttack();

                    GetComponent<Rigidbody>().velocity = (transform.position - animal.transform.position + new Vector3(0, 1.2f, 0)).normalized * 5f;
                }

                //StartCoroutine(ReEnable(animal));
                //animal.ActionEmotion(-1);                           //Reset the Action ID
                //this.animal = null;
                OnActionListener();
            }
        }

        void OnTriggerExit(Collider other)
        {
            Animal animal = other.GetComponentInParent<Animal>();

            if (!animal) return; //If there's no animal script found skip all

            if (animal != this.animal) return;

            if (HeadOnly && !other.name.Contains("Head")) return;

            RemoveCollider(other);

            if (_colliders.Count == 0)
            {
                //Debug.Log(name + " is setting a target.");
                animalAIControl.SetTarget(NextTarget, true);
                OnExit.Invoke(animal);                              //Invoke On Exit when all colliders of the animal has exited the Trigger Zone
                //animal.OnAction.RemoveListener(OnActionListener);   //Remove the Method fron the Action Listener
                animal.ActionEmotion(-1);                           //Reset the Action ID
                this.animal = null;

                firstTimeTrigger = false;

                enabled = false;
            }
        }

        public void RemoveCollider(Collider other)
        {
            if (_colliders.Find(item => item == other))     //Remove the collider that entered off the list.
            {
                _colliders.Remove(other);
            }
        }

        /// <summary>
        /// This will disable the Collider on the action zone
        /// </summary>
        /// <param name="animal"></param>
        /// <returns></returns>
        //IEnumerator ReEnable(Animal animal) //For Automatic only 
        //{
        //    if (AutomaticDisabled > 0)
        //    {
        //        GetComponent<Collider>().enabled = false;

        //        yield return null;
        //        yield return null;
        //        //animal.ActionEmotion(-1);
        //        yield return new WaitForSeconds(AutomaticDisabled);
        //        GetComponent<Collider>().enabled = true;
        //    }
        //    this.animal = null;     //Reset animal
        //    _colliders = null;      //Reset Colliders

        //    enabled = false;
        //    OnExit.Invoke(animal);
        //    animal.ActionEmotion(-1);                           //Reset the Action ID
        //    firstTimeTrigger = false;

        //    yield return null;
        //}

        public virtual void _DestroyActionZone(float time)
        {
            Destroy(gameObject, time);
        }

        /// <summary>
        /// Used for checking if the animal press the action button
        /// </summary>
        private void OnActionListener()
        {
            if (!animal) return;

            OnAction.Invoke(animal);
            if (Align && AlingPoint)
            {
                IEnumerator ICo = null;

                if (AlignLookAt)
                {
                    ICo = Utilities.MalbersTools.AlignLookAtTransform(animal.transform, AlingPoint, AlignTime, AlignCurve);
                }
                else
                {
                    ICo = Utilities.MalbersTools.AlignTransformsC(animal.transform, AlingPoint, AlignTime, AlignPos, AlignRot, AlignCurve);
                }

                StartCoroutine(ICo);
            }

            StartCoroutine(CheckForCollidersOff());

            //animal.OnAction.RemoveListener(OnActionListener);

            //animal.ActionID = -1;
            //animal = null;
        }

        IEnumerator CheckForCollidersOff()
        {
            yield return null;
            yield return null;
            if (_colliders != null && _colliders[0] && _colliders[0].enabled == false)
            {
                animal.OnAction.RemoveListener(OnActionListener);
                animal.ActionID = -1;
                animal = null;
                _colliders = null;
            }
        }

        public void CopyActionzone(ActionZone targetToCopy)
        {
            ID = targetToCopy.ID;
            NextTarget = targetToCopy.NextTarget;
            OnEnter = targetToCopy.OnEnter;
            OnSight = targetToCopy.OnSight;
            OnAction = targetToCopy.OnAction;
            OnExit = targetToCopy.OnExit;
            onGrab = targetToCopy.onGrab;
            onEnable = targetToCopy.onEnable;

            BoxCollider myCollider = GetComponent<BoxCollider>();
            BoxCollider copiedCollider = targetToCopy.gameObject.GetComponent<BoxCollider>();

            myCollider.size = copiedCollider.size;
            myCollider.center = copiedCollider.center;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (EditorAI)
            {
                //Debug.DrawLine(transform.position, NextTarget.transform.position, Color.green);
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, stoppingDistance);
            }

        }
#endif

        [HideInInspector] public bool EditorShowEvents = true;
        [HideInInspector] public bool EditorAI = true;
    }
}
﻿using UnityEngine;
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

		//───────AI───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
		public float stoppingDistance = 0.5f;
		public Transform NextTarget;

		void OnEnable()
		{
			onEnable.Invoke();
		}

		void OnTriggerEnter(Collider other)
		{
			if (enabled == false)
			{
				return;
			}

			Animal animal = other.GetComponentInParent<Animal>();
			AnimalAIControl animalAIControl = other.GetComponentInParent<AnimalAIControl>();

			if (animalAIControl && animal && other.gameObject.layer == 20) // In short, the entering object needs to have the Animal and AnimalAIControl scripts attached and be on the Animal Layer for us to do anything.
			{
				if (animalAIControl.isWandering == true)
				{
					return; // If the AnimalAIControl script is attached, but the animal is wandering, also do nothing.
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

			if (!firstTimeTrigger) // Trying to make sure that this only triggers once as the fox is entering, rather than once per collider attached to the fox.
			{
				StartCoroutine(startAnimation(animal, ID, animalAIControl));
			}
		}

		IEnumerator startAnimation(Animal animal, int id, AnimalAIControl ai) // Here's where we make the Fox do a dance... figuratively.
		{
			firstTimeTrigger = true;
			Rigidbody rigidbody = GetComponent<Rigidbody>();

			if (tag == "GrabableItem")
			{
				ai.SetClosestGrabbableItem(transform);

				if (rigidbody) // If it's a grabable item, it'll probably have a rigidbody... but there's no way to say that'll always be the case.
				{
					float rigidbodyAverageVelocity = 1f;
					float threshold = stoppingDistance * .04f;

					while (rigidbodyAverageVelocity > threshold) // I want to make sure that the grabbed object will move less than the entirety of the stopping distance away within the next two seconds.
					{
						rigidbodyAverageVelocity = +rigidbody.velocity.magnitude;
						rigidbodyAverageVelocity = rigidbodyAverageVelocity * .8f; // This is a silly setup, but it'll find me a rough average of the object's velocity. Which is all I need.
						yield return new WaitForSeconds(.1f); // Had some problems with WaitForEndOfFrame() in the editor. Might be an editor-specific thing, but this works just fine for me too.
					}
				}
			}

			while (!animal.CurrentAnimState.IsTag("Idle") || Mathf.Abs(animal.transform.position.y - transform.position.y) > .12f)
			{ // This is a final sanity check to make sure that we haven't managed to fall or jump away from the object after pathing into it's trigger.
				yield return new WaitForSeconds(.1f);
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

			StartCoroutine(SetNextTarget(ai, animal));
		}

		IEnumerator SetNextTarget(AnimalAIControl ai, Animal animal)
		{
			yield return new WaitForSeconds(.4f);

			while (animal.CurrentAnimState.IsTag("Action"))
			{
				Debug.Log("Still in an action");
				yield return new WaitForSeconds(.1f);
			}

			Debug.Log(animal.CurrentAnimState.IsTag("Action"));
			Debug.Log(animal.CurrentAnimState.IsTag("Idle"));
			Debug.Log(animal.CurrentAnimState.IsTag("Locomotion"));
			Debug.Log("Hey, we're not in an action anymore!");

			ai.SetTarget(NextTarget, true);
			onEnd.Invoke();

			animal.ActionEmotion(-1); //Reset the Action ID
			animal = null;
			enabled = false;
			firstTimeTrigger = false;
		}

		/// <summary>
		/// Used to align the animal
		/// </summary>
		private void ActionAlign(Animal animal)
		{
			onAction.Invoke();
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
		}

		public void CopyActionzone(ActionZone targetToCopy)
		{
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
	}
}
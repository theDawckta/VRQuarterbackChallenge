using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

public class AnalyticsGazeTracker : MonoBehaviour {

	public GameObject ObjectToTrack;
	public string CustomEventName;
	public string EventDataName = "";
	public string EventDataParam = "";
	public bool UseUnityEvent = false;
	public UnityEvent UnityEvent;

	private Interactible _interactible;
	private BoxCollider _boxCollider;

	void Awake()
	{
		if (ObjectToTrack == null)
		{
			ObjectToTrack = this.gameObject;
		}

		_interactible = ObjectToTrack.gameObject.GetComponent<Interactible> ();
		_boxCollider = ObjectToTrack.gameObject.GetComponent<BoxCollider>();

		if (UseUnityEvent)
		{
			if (UnityEvent == null)
				throw new Exception ("You must define a UnityEvent for Analytics GazeTracker.");
		} else
		{
			if (CustomEventName == null)
				throw new Exception ("You must define a CustomEventName for Analytics GazeTracker.");
		}

		if (_interactible == null)
			throw new Exception ("You must define an _interactible for Analytics GazeTracker.");
		if (_boxCollider == null)
			throw new Exception ("You must define an _boxCollider for Analytics GazeTracker.");
		
	}

	void OnEnable()
	{
		_interactible.OnClick += OnClick;
	}

	void OnDisable()
	{
		_interactible.OnClick -= OnClick;
	}

	void OnClick()
	{
		if (UseUnityEvent)
		{
			UnityEvent.Invoke ();
		} else
		{
			#region Analytics Call
			Analytics.CustomEvent(CustomEventName, new Dictionary<string, object>
			{
				{ EventDataName, EventDataName },
			});
			#endregion
		}
	}

	public void SetBoxSize()
	{
		StartCoroutine(SizeBoxCollider());
	}

	IEnumerator SizeBoxCollider()
	{
		yield return new WaitForEndOfFrame();
		Vector3 newSize = ObjectToTrack.gameObject.GetComponent<RectTransform> ().sizeDelta;
		//		if (newSize.z <= 0)
		//		{
		//			newSize = new Vector3 (newSize.x,newSize.y,5.0f);
		//		}
		_boxCollider.size = newSize;

	}
}

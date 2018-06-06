using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class UISnapper : MonoBehaviour {

    [Serializable]
    public struct SnapObject
    {
        public Transform toSnap;
        [HideInInspector]
        public Vector3 initPos,initRot;
        public Transform targetPosition;
        public bool useTween;
    }

    public GameObject snapOutIcon, snapInIcon;
	public TextMeshProUGUI Textfield;

    [Tooltip("Style of the animation")]
	public Ease animationStyle = Ease.InOutCubic;
    [Tooltip("Duration for the animation")]
    public float animDuration;
    [Tooltip("List of all objects that need to be snapped with their target Positions")]
    public List<SnapObject> snapObjects;
    //[Tooltip("Is the UI snapped out?")]
    //public bool snappedOut;

    private Interactible interactible;
    private bool isAnimating;
    private StatsController statsController;
	private AudioController _AudioController;

    private void Awake()
    {
        interactible = GetComponent<Interactible>();
        if (interactible == null)
            throw new Exception("An Interactible must exist on this button.");

        //for (int i = 0; i < snapObjects.Count; i++)
        //{
        //    snapObjects[i].initPos = snapObjects[i].toSnap.transform.position;
        //    snapObjects[i].initRot = snapObjects[i].toSnap.transform.rotation.eulerAngles;
        //}

        statsController = FindObjectOfType<StatsController>();
        GlobalVars.Instance.IsUISnappedOut = false;
        //ToggleSnap();
        snapOutIcon.SetActive(false);
        snapInIcon.SetActive(true);
        SnapToTarget(true);
		_AudioController = Extensions.GetRequired<AudioController>();
    }

    /// <summary>
    /// Toggles the snap state based on the current state
    /// </summary>
    void ToggleSnap()
    {
        if (!isAnimating)
        {
            isAnimating = true;

            if (GlobalVars.Instance.IsUISnappedOut)
            {
				_AudioController.PlayAudio(AudioController.AudioClips.GenericClick);
                snapOutIcon.SetActive(true);
                snapInIcon.SetActive(false);
                SnapToInitValues();
            }
            else
            {
				_AudioController.PlayAudio(AudioController.AudioClips.SmallClick);
                snapOutIcon.SetActive(false);
                snapInIcon.SetActive(true);
                SnapToTarget();
            }
        }
    }

	public void SnapToTarget(bool isInitSnap = false)
    {
        GlobalVars.Instance.IsUISnappedOut = true;
        for (int i = 0; i < snapObjects.Count; i++)
        {
            statsController.SetStatsUnderlay(false);
			if (snapObjects [i].toSnap.gameObject.activeInHierarchy)
			{
				if (snapObjects [i].useTween && !isInitSnap)
				{
					snapObjects [i].toSnap.transform.DOMove(snapObjects [i].targetPosition.position, animDuration).SetEase(animationStyle);
					snapObjects [i].toSnap.transform.DORotate(snapObjects [i].targetPosition.rotation.eulerAngles, animDuration / 2f).SetEase(animationStyle);
				} else
				{
					snapObjects [i].toSnap.position = snapObjects [i].targetPosition.position;
					snapObjects [i].toSnap.rotation = snapObjects [i].targetPosition.rotation;
				}
			}

        }
		GlobalVars.Instance.IsAllowControlsOpen = true;
        isAnimating = false;
        EventManager.Instance.StatsToggledEvent("snappedOut");
    }

    public void SnapToInitValues()
    {
        GlobalVars.Instance.IsUISnappedOut = false;
        for (int i = 0; i < snapObjects.Count; i++)
        {
            statsController.SetStatsUnderlay(true);
			if (snapObjects [i].toSnap.gameObject.activeInHierarchy)
			{
				if (snapObjects [i].useTween)
				{
					snapObjects [i].toSnap.transform.DOMove(snapObjects [i].initPos, animDuration).SetEase(animationStyle);
					snapObjects [i].toSnap.transform.DORotate(snapObjects [i].initRot, animDuration / 2f).SetEase(animationStyle);
				} else
				{
					snapObjects [i].toSnap.position = snapObjects [i].initPos;
					snapObjects [i].toSnap.rotation = Quaternion.Euler (snapObjects [i].initRot);
				}
			}
        }
		GlobalVars.Instance.IsAllowControlsOpen = false;
        isAnimating = false;
        EventManager.Instance.StatsToggledEvent("snappedIn");
    }

    public void AddSnapObject(Transform snapObject, Transform snapTarget, bool useTween)
    {
        SnapObject temp = new SnapObject();

        temp.toSnap = snapObject;
        temp.targetPosition = snapTarget;
        temp.useTween = useTween;

        temp.initPos = snapObject.transform.position;
        temp.initRot = snapObject.transform.rotation.eulerAngles;

        snapObjects.Add(temp);
    }

    void OnPanelOpened()
    {
        if (!GlobalVars.Instance.IsUISnappedOut)
            ToggleSnap();
    }

    private void SnapComplete()
    {
        isAnimating = false;
    }

    private void OnEnable()
    {
        interactible.OnClick += ToggleSnap;
        EventManager.OnPanelOpenComplete += OnPanelOpened;
    }

    private void OnDisable()
    { 
        interactible.OnClick -= ToggleSnap;
        EventManager.OnPanelOpenComplete -= OnPanelOpened;
    }
}

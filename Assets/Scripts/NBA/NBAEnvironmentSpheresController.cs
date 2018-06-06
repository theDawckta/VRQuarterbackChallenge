using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NBAEnvironmentSpheresController : MonoBehaviour {

	public GameObject NBALightsOnSphere;
	public GameObject NBALightsOffSphere;

	private Material _LightsOffMaterial;
	private float _LightsFadeTime = 0.6f;

	void Awake()
	{
		if (NBALightsOffSphere != null)
		{
			Renderer rend = NBALightsOffSphere.GetComponent<Renderer>();
			if (rend != null)
			{
				_LightsOffMaterial = rend.material;
				if(_LightsOffMaterial != null)
					_LightsOffMaterial.DOFade(0, 0);
			}
			NBALightsOffSphere.SetActive (false);
		}
	}

	/// <summary>
	/// Hide our lights if they are not available
	/// </summary>
	void HideLightsOff()
	{
		if (NBALightsOffSphere != null)
		{
			if(_LightsOffMaterial != null)
				_LightsOffMaterial.DOFade(0, 0);

			NBALightsOffSphere.SetActive (false);
		}
	}

	/// <summary>
	/// Turns the lights off by toggling/fading sphere
	/// </summary>
	void TurnLightsOff()
	{

		if (NBALightsOffSphere != null)
		{
			NBALightsOffSphere.SetActive (true);
			if(_LightsOffMaterial != null)
				_LightsOffMaterial.DOFade(1.0f, _LightsFadeTime);
		}

	}

	/// <summary>
	/// Turns the lights on by toggling/fading sphere
	/// </summary>
	void TurnLightsOn()
	{
		if (NBALightsOffSphere != null)
		{
			NBALightsOffSphere.SetActive (true);
			if(_LightsOffMaterial != null)
				_LightsOffMaterial.DOFade(0, _LightsFadeTime).OnComplete(HideLightsOff);
		}
	}

	void EventManager_OnLightsOnEvent ()
	{
		TurnLightsOn ();
	}

	void EventManager_OnLightsOffEvent ()
	{
		TurnLightsOff ();
	}

	void OnEnable()
	{
		EventManager.OnLightsOffEvent += EventManager_OnLightsOffEvent;
		EventManager.OnLightsOnEvent += EventManager_OnLightsOnEvent;
	}

	void OnDisable()
	{
		EventManager.OnLightsOffEvent -= EventManager_OnLightsOffEvent;
		EventManager.OnLightsOnEvent -= EventManager_OnLightsOnEvent;
	}
}
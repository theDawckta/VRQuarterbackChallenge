using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HighlightMarker : MonoBehaviour 
{
	public delegate void HighlightMarkerClickAction(HighlightMarker highlightMarker);
	public event HighlightMarkerClickAction OnHighlightMarkerClick;
	
	public SpriteRenderer MarkerImage;
	public string MarkerUrl;
	public long TimeStamp;

	private Interactible _Interactible;

	void Awake()
	{
		_Interactible = GetComponent<Interactible>();

		if (_Interactible == null)
			throw new Exception("An _Interactible must exist on this button.");
	}

	void OnEnable()
	{
		_Interactible.OnClick += OnHighlightClick;
	}

	void OnDisable()
	{
		_Interactible.OnClick -= OnHighlightClick;
	}

	void OnHighlightClick()
	{
		HighlightMarkerClickEvent(this);
	}

	public void HighlightMarkerClickEvent(HighlightMarker highlightMarkerClicked)
    {
		OnHighlightMarkerClick(highlightMarkerClicked);
    }
}

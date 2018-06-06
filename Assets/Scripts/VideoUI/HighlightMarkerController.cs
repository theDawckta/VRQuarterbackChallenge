using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class HighlightMarkerController : MonoBehaviour 
{
	public delegate void OnMarkerClickEvent(HighlightMarkerController thisMarker);
	public event OnMarkerClickEvent OnMarkerClicked;
	[HideInInspector]
	public long Position;
	
	private Interactible _highlightMarkerInteractible;
	private SpriteRenderer _highlightMarkerSprite;
	private Color _rolloverColor;
	private Vector3 _overScale;
	private Vector3 _originalScale;

	void Awake()
	{
		_highlightMarkerInteractible = gameObject.GetComponent<Interactible>();
		_highlightMarkerSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
		_rolloverColor = _highlightMarkerSprite.color;
		_highlightMarkerSprite.color = Color.white;
		_originalScale = transform.localScale;
		_overScale = transform.localScale * 2.0f;
	}

	private void HighlightMarkerClicked()
	{
		OnMarkerClicked(this);
	}

	private void HighlightMarkerOver()
	{
		transform.DOScale (_overScale, 0.3f);
		_highlightMarkerSprite.DOColor (_rolloverColor, 0.3f);
	}

	private void HighlightMarkerOut()
	{
		transform.DOScale (_originalScale, 0.3f);
		_highlightMarkerSprite.DOColor (Color.white, 0.3f);
	}

	void OnEnable()
	{
		_highlightMarkerInteractible.OnClick += HighlightMarkerClicked;
		_highlightMarkerInteractible.OnOver += HighlightMarkerOver;
		_highlightMarkerInteractible.OnOut += HighlightMarkerOut;
	}

	void OnDisable()
	{
		_highlightMarkerInteractible.OnClick -= HighlightMarkerClicked;
		_highlightMarkerInteractible.OnOver -= HighlightMarkerOver;
		_highlightMarkerInteractible.OnOut -= HighlightMarkerOut;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Analytics;

public class BtnNextPrevVideoHandler : MonoBehaviour {

	public bool IsNextButton = true;

	private ButtonController _ButtonController;
	private DataCursor _Cursor;
	private List<ContentViewModel> _CurrentItems;
	private ContentViewModel _TileContentViewModel;

	void Awake()
	{
		_ButtonController = this.gameObject.GetComponent<ButtonController> ();


		if (_ButtonController == null)
			throw new Exception("A ButtonController must exist on this button.");
	}
		
	/// <summary>
	/// Loop through our video list to determine the next and previous videos
	/// </summary>
	IEnumerator Start()
	{
		yield return DataCursorComponent.Instance.GetCursorAsync(cursor => _Cursor = cursor);
		_CurrentItems = new List<ContentViewModel> (_Cursor.CurrentItems);
		//only do any of this if we have enough items to cycle through
		if (_CurrentItems.Count > 1)
		{
			for (int i = _CurrentItems.Count - 1; i >= 0; i--)
			{
				//only keep clips
				if (_CurrentItems [i].Type == ContentType.Channel || _CurrentItems [i].IsUpcoming)
					_CurrentItems.RemoveAt (i);
			}
				
			//if we only have one item after removing unnecessary pieces
			if (_CurrentItems.Count < 2)
			{
				this.gameObject.SetActive (false);
				yield break;
			}
			
			for (int i = 0; i < _CurrentItems.Count; i++)
			{

				if (_CurrentItems [i] == _Cursor.CurrentVideo)
				{
					int prevVideoIndex = i - 1;
					if (i == 0)
						prevVideoIndex = _CurrentItems.Count - 1;

					int nextVideoIndex = i + 1;
					if (i == _CurrentItems.Count - 1)
						nextVideoIndex = 0;

					//only once we get these items can we show our buttons
					if (IsNextButton)
					{
						_TileContentViewModel = _CurrentItems [nextVideoIndex];
					} else
					{
						_TileContentViewModel = _CurrentItems [prevVideoIndex];
					}
					break;
				}
			}
		} else
		{
			this.gameObject.SetActive (false);
		}
	}
		
	void OnNextPrevClick(object sender)
	{
		if (_TileContentViewModel != null)
		{
			EventManager.Instance.VideoTriggerEvent (_TileContentViewModel);
		}
	}

	void OnEnable()
	{
		if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked += OnNextPrevClick;
		}
	}

	void OnDisable()
	{
		if (_ButtonController != null)
		{
			_ButtonController.OnButtonClicked -= OnNextPrevClick;
		}
	}
}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ContentTileMenuSubnavController : MonoBehaviour 
{
	public delegate void MenuSubnavAction(ContentViewModel curContentViewModel);
	public event MenuSubnavAction OnMenuSubnav;
	
	public GameObject SubnavHolder;
	public ButtonControllerReplacement ButtonControllerPrefab;
	public GameObject HeaderBar;
	public TextMeshProUGUI HeaderTxt;
	public List <ButtonControllerReplacement> TabNavItemList;

	private List<ContentViewModel> _subItemContentViewModels = new List<ContentViewModel>();
	private ContentViewModel SubNavContentViewModel;

    private DataCursor _Cursor;
    private string targetID;

	/// <summary>
	/// Initialize our subnav items based on the current ContentViewModel
	/// </summary>
	/// <param name="curContent">Current content.</param>
	public void Init(ContentViewModel curContent)
	{
        targetID = curContent.ID;

        if (SubnavHolder != null && ButtonControllerPrefab != null)
        {
            for (int i = 0; i < curContent.Children.Count; i++)
            {
                //make sure that we actually have child elements
                ContentViewModel curChild = curContent.Children[i];
                //we should hide items that do not have any children
                if (curChild.Children.Count >= 1)
                {
                    if (i < TabNavItemList.Count)
                    {
                        ButtonControllerReplacement curSubnavButton2 = TabNavItemList[i];
                        curSubnavButton2.SetIcon(curChild.CaptionLine1);
                        Color startingColor = new Color(0.8f, 0.58f, 0.0f);
                        Color rolloverColor = Color.white;
                        curSubnavButton2.SetIconColors(startingColor, rolloverColor);
                        //curSubnavButton2.Init (Color.white, GlobalVars.Instance.GrayColor, Color.white, Color.white, GlobalVars.Instance.PrimaryColor, Color.white, Color.white);
                        curSubnavButton2.OnButtonClicked += OnSubItemClicked;
                        curSubnavButton2.gameObject.SetActive(true);
                        _subItemContentViewModels.Add(curChild);
                    }
                }
            }
        }
	}

	/// <summary>
	/// Set up the header text that appears for each section (currently section based)
	/// </summary>
	void InitHeader(ContentViewModel curContent)
	{
//TODO: likely do not need header text - will leave in case
//		if (curContent != null)
//		{
//			if (HeaderTxt != null)
//			{
//				if(curContent.CaptionLine1 != null)
//					HeaderTxt.text = curContent.CaptionLine1;
//			}
//		}
	}

	/// <summary>
	/// Highlight our subnav items based on an index
	/// </summary>
	/// <param name="index">Which item in our list should be selected</param>
	public void SetHighlightSubnav(int index)
	{
		UnHighlightSubnav ();
		if (SubnavHolder != null)
		{
			for (int i = 0; i < SubnavHolder.transform.childCount; i++)
			{
				if (index == i)
				{
					ButtonControllerReplacement curButtonController = SubnavHolder.transform.GetChild (i).GetComponent<ButtonControllerReplacement>();
					if (curButtonController != null)
					{
						curButtonController.Highlight ();
						curButtonController.Disable (false);
					}
					break;
				}
			}
		}
	}

	/// <summary>
	/// Highlights the subnav based on ContentViewModel
	/// </summary>
	/// <param name="curContentViewModel">Current content view model.</param>
//TODO: For this execution, this will probably no longer be needed and can be pulled
	public void HighlightSubnav(ContentViewModel curContentViewModel)
	{
		if (SubnavHolder != null)
		{
			UnHighlightSubnav ();
			ContentViewModel secondaryContentViewModel = ContentViewModel.FindGrandAncestor (curContentViewModel);
			InitHeader (secondaryContentViewModel);
			for (int i = 0; i < _subItemContentViewModels.Count; i++)
			{
				if (_subItemContentViewModels [i].ID == secondaryContentViewModel.ID)
				{
					SetHighlightSubnav (i);
				}
			}
		}
	}

	/// <summary>
	/// Deselect any highlighted items
	/// </summary>
	void UnHighlightSubnav()
	{
		if (SubnavHolder != null)
		{
			foreach (Transform child in SubnavHolder.transform)
			{
				ButtonControllerReplacement curButtonController = child.gameObject.GetComponent<ButtonControllerReplacement> ();
				if (curButtonController != null)
				{
					curButtonController.Enable ();
					curButtonController.UnHighlight ();
				}
			}
		}
	}

	/// <summary>
	/// Clear out the subnavigation
	/// </summary>
	void ClearSubnav()
	{
		_subItemContentViewModels.Clear ();
		if (SubnavHolder != null)
		{
			foreach (Transform child in SubnavHolder.transform)
			{
				GameObject.Destroy (child.gameObject);
			}
		}
	}

	/// <summary>
	/// Handle oindividual subnav item clicks - check which ContentViewModel we refer to
	/// </summary>
	/// <param name="sender">Sender.</param>
	void OnSubItemClicked(object sender)
	{
		if (SubnavHolder != null)
		{
			int counter = 0;
			foreach (Transform child in SubnavHolder.transform)
			{
				ButtonControllerReplacement curButton = child.gameObject.GetComponent<ButtonControllerReplacement> ();
				if (curButton == sender as ButtonControllerReplacement)
				{
					if (_subItemContentViewModels.Count > 0)
					{
						ContentViewModel curContentViewModel = _subItemContentViewModels [counter];
						MenuSubnavEvent (curContentViewModel);
						SetHighlightSubnav (counter);
						break;
					}
				}
				counter++;
			}
		}
	}

	/// <summary>
	/// Dispatch our subnav event with selected ContentViewModel to listeners
	/// </summary>
	/// <param name="curContentViewModel">Current content view model.</param>
	void MenuSubnavEvent(ContentViewModel curContentViewModel)
	{
		if (OnMenuSubnav != null)
		{
			if (curContentViewModel != null)
			{
				OnMenuSubnav (curContentViewModel);
				InitHeader (curContentViewModel);
                //targetID = curContentViewModel.ID;
			}
		}
	}

	void OnDestroy()
	{
		ClearSubnav ();
	}

	void EventManager_OnDrawContentMenuTilesComplete (ContentViewModel curContentViewModel)
	{
		HighlightSubnav (curContentViewModel);
	}

    void OnMenuUpdated(object sender, ContentUpdatedEventArgs<DataCursor> args)
    {
        _Cursor = args.Content;
        ContentViewModel whereWeWere = null;
        try
        {
            _Cursor.GetRoot().Children.Single(c => c.ID == targetID);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception in OnMenuUpdated: " + ex.ToString());
        }
        if (whereWeWere != null)
        {
            // ELF Clear out the old sub content models, we use ID searches to find them and these are obsolete
            _subItemContentViewModels = new List<ContentViewModel>();
            Init(whereWeWere);
        }
    }

    void OnEnable()
	{
		EventManager.OnDrawContentMenuTilesComplete += EventManager_OnDrawContentMenuTilesComplete;
        DataCursorComponent.Instance.Updated += OnMenuUpdated;
    }

    void OnDisable()
	{
		EventManager.OnDrawContentMenuTilesComplete -= EventManager_OnDrawContentMenuTilesComplete;
        DataCursorComponent.Instance.Updated -= OnMenuUpdated;
    }
}

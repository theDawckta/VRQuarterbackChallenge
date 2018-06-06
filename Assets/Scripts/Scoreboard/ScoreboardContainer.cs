using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// TODO: Note that this file should become "ScoreboardController", and the functionality within Scoreboard controller should roll into this file.
/// Currently just setting up input fields to be populated
/// </summary>

public class ScoreboardContainer : MonoBehaviour {

	//Many of these fields are broken into two fields to control font sizing/placement
	//they could potentially be a single field if we allow line breaks/font sizing from external file
	public Text HeaderText; 
	public Text SubheaderText;
	public Text Team1Headline;
	public Text Team1Subhead;
	public Text Team2Headline;
	public Text Team2Subhead;
	public Text Team1Score;
	public Text Team2Score;
	public Text TimeHeader;
	public Text TimeSubhead;
	//Timeouts? 
	public List<GameObject> HomeTickerItems;
	public List<GameObject> AwayTickerItems;

	//Materials on these objects may need to change to support alpha if we would like to fade in
	//Dynamic textures just need fed a http/https image path or resource path via .SetTexture
	public DynamicTexture BrandingLeft;
	public DynamicTexture BrandingCenter;
	public DynamicTexture BrandingRight;

//	public DynamicTexture AwayArrow;
//	public DynamicTexture HomeArrow;
	public GameObject AwayArrow;
	public GameObject HomeArrow;
	//TODO: need to add dynamic texture items for both home/away teams, as well as ability to control team color bars
	//currently, the mesh for the 'away' team and its color bar are combined into one item - this needs redone on design side. 
	//(Home team appears to be correct, though how they are getting the color for the material they set is currently a question out to design.


	// Use this for initialization
	void Awake () {
		CheckRequired(HeaderText, "HeaderText");
		CheckRequired(SubheaderText, "SubheaderText");
		CheckRequired(Team1Headline, "Team1Headline");
		CheckRequired(Team1Subhead, "Team1Subhead");
		CheckRequired(Team2Headline, "Team2Headline");
		CheckRequired(Team2Subhead, "Team2Subhead");
		CheckRequired(Team1Score, "Team1Score");
		CheckRequired(Team2Score, "Team2Score");
		CheckRequired(TimeHeader, "TimeHeader");
		CheckRequired(TimeSubhead, "TimeSubhead");
		CheckRequired(BrandingLeft, "BrandingLeft");
		CheckRequired(BrandingCenter, "BrandingCenter");
		CheckRequired(BrandingRight, "BrandingRight");
	}

	private void CheckRequired(object thing, string name)
	{
		if (thing == null)
			throw new Exception(String.Format("A {0} is required to run this scene.", name));
	}
}

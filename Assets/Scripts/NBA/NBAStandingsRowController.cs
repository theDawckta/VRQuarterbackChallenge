using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

public class NBAStandingsRowController : MonoBehaviour {

	public Image logo;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI gamesBehindText;
	public TextMeshProUGUI overallText;
	public SpriteAtlas logoAtlas;
	public Color clinchColor;

	public void SetUpRow (string name, string gamesBehind, string overall, bool clinched) {
		logo.sprite = logoAtlas.GetSprite (name.ToLower ());
		nameText.text = name;
		if (gamesBehind.Length == 1)
		{
			if (gamesBehind [0] == '0')
			{
				gamesBehind = "--";
			}
			else
			{
				gamesBehind += ".0";
			}
				
		}

		if (gamesBehind [gamesBehind.Length - 2] != '.' && gamesBehind [gamesBehind.Length - 2] != '-')
		{
			gamesBehind += ".0";
		}

		gamesBehindText.text = gamesBehind;
		overallText.text = overall;
		Transform greatGrandParent = gameObject.transform.parent.parent.parent.GetComponent<StandingsPanelController> ().iconHolder.transform;
			
		logo.gameObject.transform.SetParent (greatGrandParent, false);
		if (!clinched)
		{
			nameText.color = Color.white;
//			gamesBehindText.color = Color.white;
//			overallText.color = Color.white;
		}
		else
		{
			nameText.color = clinchColor;
//			gamesBehindText.color = clinchColor;
//			overallText.color = clinchColor;
		}
	}	
}

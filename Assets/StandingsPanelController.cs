﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StandingsPanelController : MonoBehaviour {

	public bool debug = false;

	public GameObject westBackground;
	public GameObject eastBackground;
	public GameObject image;
	public TextMeshProUGUI heading;
	public GameObject rowHolder;
	public GameObject iconHolder;
	public CanvasGroup standingsCanvas;

	public Dictionary<string,string> teamIDTable = new Dictionary<string, string> {
		{"1610612737","Hawks"},
		{"1610612738","Celtics"},
		{"1610612751","Nets"},
		{"1610612766","Hornets"},
		{"1610612741","Bulls"},
		{"1610612739","Cavaliers"},
		{"1610612742","Mavericks"},
		{"1610612743","Nuggets"},
		{"1610612765","Pistons"},
		{"1610612744","Warriors"},
		{"1610612745","Rockets"},
		{"1610612754","Pacers"},
		{"1610612746","Clippers"},
		{"1610612747","Lakers"},
		{"1610612763","Grizzlies"},
		{"1610612748","Heat"},
		{"1610612749","Bucks"},
		{"1610612750","Timberwolves"},
		{"1610612740","Pelicans"},
		{"1610612752","Knicks"},
		{"1610612760","Thunder"},
		{"1610612753","Magic"},
		{"1610612755","76ers"},
		{"1610612756","Suns"},
		{"1610612757","Trail Blazers"},
		{"1610612758","Kings"},
		{"1610612759","Spurs"},
		{"1610612761","Raptors"},
		{"1610612762","Jazz"},
		{"1610612764","Wizards"}
	};

	public void Start()
	{
		standingsCanvas.alpha = 0f;
	}

}

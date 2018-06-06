using System;
using System.Collections;
using UnityEngine;

public class GlobalVars : Singleton<GlobalVars>
{
    public bool IsInitialLaunch = true;
	public bool IsAllowControlsOpen = true;
	public string NoInternet = "No internet connection detected.\nPlease check your connection and try again.";
	public string VideoError = "We're sorry, an error occurred with our player.\nPlease try again.";
	public bool IsAuthorizedToDoMove = true;
    public bool IsUISnappedOut = false;
    public string CurrentTimestamp="";
    public bool feedbackPopupShown = false;
	public bool IsDaydream = false;
	public bool UseHEVCStream = false;
	public Color PrimaryColor = new Color32( 0x2f, 0x88, 0xd3, 0xFF ); //Color.red;
	public Color SecondaryColor = new Color32(0x00, 0xaf, 0xef, 0xFF); //Color.blue;
	public Color HighlightColor = new Color32( 0x33, 0x65, 0x8e, 0xFF ); //33658e Color.green;
    public Color GrayColor = Color.grey;
}

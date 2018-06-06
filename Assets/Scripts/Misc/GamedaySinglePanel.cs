using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GamedaySinglePanel : MonoBehaviour {

	public TextMeshProUGUI Team1Name;
	public TextMeshProUGUI Team2Name;
	public TextMeshProUGUI Team1Score;
	public TextMeshProUGUI Team2Score;
	public TextMeshProUGUI TimeText;
	public Image Team1Image;
	public Image Team2Image;

	void Awake()
	{
		//SetCanvasInitialAlpha (Team1Image);
		//SetCanvasInitialAlpha (Team2Image);
	}

	public void SetCanvasInitialAlpha(DynamicTexture teamTexture)
	{
		if (teamTexture != null)
		{
			CanvasGroup teamCanvas = teamTexture.GetComponent<CanvasGroup> ();
			if (teamCanvas != null)
			{
				teamCanvas.alpha = 0;
			}
		}
	}


}

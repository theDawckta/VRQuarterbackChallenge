using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextConverter : ScriptableWizard
{
	//TODO Auto fit text and rect transform convertion function

	string defaultFont = "IntelClear_Bd";
	int cnt=0;

	/// <summary>
	/// Gets the alignment for text.
	/// </summary>
	/// <returns>The alignment for text.</returns>
	/// <param name="t">T.</param>
	TextAlignmentOptions GetAlignmentForText (TextAnchor t)
	{
		switch (t) 
		{

		case TextAnchor.LowerCenter:
			return TextAlignmentOptions.Bottom;

		case TextAnchor.LowerLeft:
			return TextAlignmentOptions.BottomLeft;

		case TextAnchor.LowerRight:
			return TextAlignmentOptions.BottomRight;
		
		case TextAnchor.UpperCenter:
			return TextAlignmentOptions.Top;

		case TextAnchor.UpperLeft:
			return TextAlignmentOptions.TopLeft;

		case TextAnchor.UpperRight:
			return TextAlignmentOptions.TopRight;

		case TextAnchor.MiddleCenter:
			return TextAlignmentOptions.Center;

		case TextAnchor.MiddleLeft:
			return TextAlignmentOptions.MidlineLeft;

		case TextAnchor.MiddleRight:
			return TextAlignmentOptions.MidlineRight;

		default:
			return TextAlignmentOptions.Center;

		}
	}

	/// <summary>
	/// Gets the alignment for text.
	/// </summary>
	/// <returns>The alignment for text.</returns>
	/// <param name="t">T.</param>
	TextAnchor GetAlignmentForText(TextAlignmentOptions t)
	{
		switch (t) 
		{

		case TextAlignmentOptions.Bottom:
			return TextAnchor.LowerCenter;

		case TextAlignmentOptions.BottomLeft:
			return TextAnchor.LowerLeft;

		case TextAlignmentOptions.BottomRight:
			return TextAnchor.LowerRight;

		case TextAlignmentOptions.Top:
			return TextAnchor.UpperCenter;

		case TextAlignmentOptions.TopLeft:
			return TextAnchor.UpperLeft;

		case TextAlignmentOptions.TopRight:
			return TextAnchor.UpperRight;

		case TextAlignmentOptions.Center:
			return TextAnchor.MiddleCenter;

		case TextAlignmentOptions.MidlineLeft:
			return TextAnchor.MiddleLeft;

		case TextAlignmentOptions.MidlineRight:
			return TextAnchor.MiddleRight;

		default:
			return TextAnchor.MiddleCenter;
		}
	}

	/// <summary>
	/// Gets the wrap mode for text.
	/// </summary>
	/// <returns>The wrap mode for text.</returns>
	/// <param name="t">T.</param>
	HorizontalWrapMode GetWrapModeForText(TextOverflowModes t)
	{
		switch (t) 
		{
		case TextOverflowModes.Overflow:
			return HorizontalWrapMode.Overflow;

		case TextOverflowModes.ScrollRect:
			return HorizontalWrapMode.Wrap;

		default:
			return HorizontalWrapMode.Overflow;
			
		}
	}

	/// <summary>
	/// Gets the wrap mode for text.
	/// </summary>
	/// <returns>The wrap mode for text.</returns>
	/// <param name="t">T.</param>
	TextOverflowModes GetWrapModeForText(HorizontalWrapMode t)
	{
		switch (t) 
		{
		case HorizontalWrapMode.Overflow:
			return TextOverflowModes.Overflow;

		case HorizontalWrapMode.Wrap:
			return TextOverflowModes.ScrollRect;

		default:
			return TextOverflowModes.Overflow;

		}
	}

	/// <summary>
	/// Converts Text component to TextMeshPro -Text(UI) component.
	/// Looks for all the active gameObjects with Text component on it.
	/// </summary>
	void ConvertToTextmeshpro()
	{
		cnt = 0;
		Text[] textObjects = GameObject.FindObjectsOfType<Text> ();

		foreach (Text t in textObjects) 
		{
			Text tmp = t;
			GameObject g = t.gameObject;
			DestroyImmediate (t);
			TextMeshProUGUI textpro = g.AddComponent<TextMeshProUGUI> ();
			textpro.text = tmp.text;
			string fontname = defaultFont + " SDF";
			if(tmp.font) fontname = tmp.font.name + " SDF";
			textpro.font = (TMP_FontAsset)Resources.Load (fontname);
			textpro.fontSize=tmp.fontSize;
			textpro.alignment = GetAlignmentForText (tmp.alignment);
			textpro.overflowMode = GetWrapModeForText (tmp.horizontalOverflow);
			textpro.color = tmp.color;
			cnt++;
		}
	}

	/// <summary>
	/// Convert TextMeshPro components to Text UI.
	/// </summary>
	void ConvertToTextUI()
	{
		cnt = 0;
		TextMeshProUGUI[] textproObjects = GameObject.FindObjectsOfType<TextMeshProUGUI> ();

		foreach (TextMeshProUGUI t in textproObjects) 
		{
			TextMeshProUGUI tmp = t;
			GameObject g = t.gameObject;
			DestroyImmediate (t);
			Text textUI = g.AddComponent<Text> ();
			textUI.text = tmp.text;
			string fontname = defaultFont;
			if(tmp.font) fontname = tmp.font.name.Split(' ')[0];
			textUI.font = (Font)Resources.Load (fontname);
			textUI.fontSize=(int)tmp.fontSize;
			textUI.alignment = GetAlignmentForText (tmp.alignment);
			textUI.horizontalOverflow = GetWrapModeForText (tmp.overflowMode);
			textUI.verticalOverflow = VerticalWrapMode.Overflow;
			textUI.color = tmp.color;
			cnt++;
		}
	}

	[MenuItem("Tools/TrueVR Editor/Text Converter")]
	public static void ShowWindow()
	{
		TextConverter wizard = ScriptableWizard.DisplayWizard<TextConverter>("Text Converter");

		if (wizard == null) 
		{
			Debug.Log ("Failed to load Text Converter window");
		}
	}

	void OnGUI()
	{
		GUILayout.Label ("Text Settings", EditorStyles.boldLabel);
		GUILayout.Label ("Convert all active Text UI to TextMeshPro", EditorStyles.label);

		if (GUILayout.Button ("Convert")) 
		{
			ConvertToTextmeshpro ();
		}

		GUILayout.Label ("Revert all to Text UI component", EditorStyles.label);

		if (GUILayout.Button ("Revert")) 
		{
			ConvertToTextUI ();
		}

		if(cnt>0) GUILayout.Label (cnt + " text components changed!", EditorStyles.label);
		else  GUILayout.Label ("No change.", EditorStyles.label);
	}

}
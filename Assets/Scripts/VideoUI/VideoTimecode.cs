using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoTimecode : MonoBehaviour
{
	public TextMeshProUGUI Timecode;
    private VideoController _VideoController;

    private void Awake()
    {
        _VideoController = Extensions.GetRequired<VideoController>();
		Timecode = GetComponent<TextMeshProUGUI> ();
    }

    void Update()
    {
		if (Timecode != null)
		{
			if (Timecode.IsActive ())
			{
				string timecodeStr = _VideoController.GetVideoTimecode ();
				Timecode.text = timecodeStr;
			}
		}
    }
}
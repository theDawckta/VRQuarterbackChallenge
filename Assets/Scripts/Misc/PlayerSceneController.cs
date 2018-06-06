using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerSceneController : MonoBehaviour {

	public JackalopeMediaPlayer m_MediaPlayer;

	public Text m_MessageText;

	private bool	_wasMounted;
	private string	_moviePath = "http://live.vokevr.com/20161112FootballMinnVsNeb/gearvr/pod1/master_fullhighlight.m3u8";

	void Awake()
	{		
		m_MediaPlayer.AutoPlay = false;
	}

	// Use this for initialization
	void Start () 
	{
		_wasMounted = OVRManager.instance.isUserPresent;
	
		// Set the media path
		m_MediaPlayer.MoviePath = _moviePath;
		if (m_MediaPlayer.MoviePath != null)
			Debug.Log ("Movie Path: " + m_MediaPlayer.MoviePath);
		m_MediaPlayer.Loop = true;
		m_MediaPlayer.Play ();
	}

	void OnEnable()
	{
		
	}

	void OnDisable()
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool isMounted = OVRManager.instance.isUserPresent;
		if (_wasMounted != isMounted)
		{
			bool isPlaying = m_MediaPlayer.IsPlaying;
			if (isPlaying && !isMounted)
			{
				m_MediaPlayer.Pause();
			}

			if (!isPlaying && isMounted)
			{
				m_MediaPlayer.Play();
			}

			_wasMounted = isMounted;
		}
	}


	private void showUserMessage(string message)
	{
		m_MessageText.gameObject.SetActive (true);
		m_MessageText.text = message;
	}
}

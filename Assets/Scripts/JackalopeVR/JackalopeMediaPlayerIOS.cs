using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

#if (UNITY_IOS) && (!UNITY_EDITOR)
public partial class JackalopeMediaPlayer : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern void JRP_InitializeTexture(int textureID, int textureWidth, int textureHeight);

	[DllImport("__Internal")]
	private static extern void JRP_UpdateTexture();

	[DllImport ("__Internal")]
	private static extern IntPtr GetRenderEventFunc();

	[DllImport ("__Internal")]
	private static extern void JRP_SetMediaSource(string moviePath);

	[DllImport ("__Internal")]
	private static extern void JRP_PauseMovie();

	[DllImport ("__Internal")]
	private static extern void JRP_ResumeMovie();

	[DllImport ("__Internal")]
	private static extern void JRP_LoopPlay(bool bIsLoop);

	[DllImport ("__Internal")]
	private static extern void JRP_SetMaxBitRate(int maxBitRate);

	[DllImport ("__Internal")]
	private static extern void JRP_SeekToTime(long time);

	[DllImport ("__Internal")]
	private static extern bool JRP_IsLive();

	[DllImport ("__Internal")]
	private static extern long  JRP_GetDuration();

	[DllImport ("__Internal")]
	private static extern long  JRP_GetCurrentPosition();

	[DllImport ("__Internal")]
	private static extern int  JRP_GetPlaybackState();


private MediaPlaybackState getPlaybackState()
{
	return (MediaPlaybackState)JRP_GetPlaybackState();
}

private long getDuration()
{
	return JRP_GetDuration();
}

private long getCurrentPosition()
{
	return JRP_GetCurrentPosition();
}

private void setCurrentPosition(long pos)
{
	JRP_SeekToTime(pos);
}

private void playVideo()
{
	JRP_ResumeMovie();
}

private void pauseVideo()
{
	JRP_PauseMovie();
}

private void startVideo()
{
	JRP_SetMediaSource(_mediaFullPath);
}

private void stopVideo()
{

}

private void loopVideo(bool isLoop)
{
	JRP_LoopPlay(isLoop);
}

private bool isLive()
{
	return JRP_IsLive();
}

private void setMaxBitrate(int maxBitRate)
{
	JRP_SetMaxBitRate(maxBitRate);
}

void onStateChanged(string eventName)
{

	if(String.Compare(eventName, "onBuffering") == 0){

		if (OnBuffering != null)
			OnBuffering();

	}
	else if(String.Compare(eventName, "onReadyToPlay") == 0){

		if (OnReadyToPlay != null)
			OnReadyToPlay();

	}
	else if(String.Compare(eventName, "onEndOfStream") == 0){

		if (OnEndOfStream != null)
			OnEndOfStream();

	}
}

void onError(string errorDescription)
{
	Debug.LogError("MoviePlayer: " + errorDescription);
	if (OnError != null)
		OnError(2, 1);

}

void onChangePresentaionSize(string paramData)
{
	int nWidth = 0;
	int nHeight = 0;

	string[] ssizes = paramData.Split('-');

	if(ssizes.Length == 2)
	{
		nWidth = (int)Convert.ToDouble(ssizes[0]);
		nHeight = (int)Convert.ToDouble(ssizes[1]);
	}
}

void onUserDefinedTextMetadata(string metaData)
{
	if (OnUserDefinedTextMetadata != null)
		OnUserDefinedTextMetadata(metaData, metaData, metaData);
}

}
#endif
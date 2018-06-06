using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (!USE_UMP)
public partial class JackalopeMediaPlayer : MonoBehaviour
{
    // TODO: How do i handle the flipped image?
	public Vector2 _leftEyeOffset = new Vector2(0.0f, -0.5f);
	public Vector2 _leftEyeScale = new Vector2(1.0f, 0.5f);
	public Vector2 _rightEyeOffset = new Vector2(0.0f, 0.0f);
	public Vector2 _rightEyeScale = new Vector2(1.0f, 0.5f);

    private MediaPlayer _mediaPlayer;

    private bool _Muted;
    private bool _AutoStartTriggered = false;
    private bool _IsBuffering = true;

	partial void ReInit()
	{
		if(!Is2DUsingTimeWarp)
		{
			_leftEyeOffset = new Vector2(0.0f, 0.0f);
			_leftEyeScale = new Vector2(1.0f, 1.0f);
		}
		else if(!Is360UsingTimeWarp)
		{
			_leftEyeOffset = new Vector2(0.0f, 1.0f);
			_leftEyeScale = new Vector2(1.0f, 1.0f);
		}
		else
		{
			_leftEyeOffset = new Vector2(0.0f, -0.5f);
			_leftEyeScale = new Vector2(1.0f, 0.5f);
		}
	}

    partial void SetMuteInternal(bool mute)
    {
        _Muted = mute;
    }

    private long getDuration()
    {
        if (_mediaPlayer == null || _mediaPlayer.Info == null)
            return 0;

        return Mathf.FloorToInt(_mediaPlayer.Info.GetDurationMs());
    }

    private long getCurrentPosition()
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null)
            return 0;

        return Mathf.FloorToInt(_mediaPlayer.Control.GetCurrentTimeMs());
    }

    private void setCurrentPosition(long pos)
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null)
            return;

        if (pos <= 0)
        {
            _mediaPlayer.Control.Seek(0.0f);
            return;
        }
        _mediaPlayer.Control.Seek((float)pos);
    }

    private MediaPlaybackState getPlaybackState()
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null)
            return MediaPlaybackState.Unknown;

        if (!_mediaPlayer.Control.CanPlay())
            return MediaPlaybackState.Preparing;

        if (_mediaPlayer.Control.IsPlaying())
            return MediaPlaybackState.Playing;

        if (_mediaPlayer.Control.IsBuffering())
            return MediaPlaybackState.Buffering;

        if (!_mediaPlayer.Control.IsBuffering())
            return MediaPlaybackState.Ready;

        return MediaPlaybackState.Unknown;
    }

    private void startVideo()
    {
        // If the player doesn't exist, it should be created and added to the current component
        if (_mediaPlayer == null)
        {
            _mediaPlayer = gameObject.AddComponent<MediaPlayer>();
        }
        else
        {
            _mediaPlayer.Stop();
            _mediaPlayer.CloseVideo();
        }

        // Instantiate the media player
        _mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, _moviePath, false);

        // Register event handler
        _mediaPlayer.Events.AddListener(OnMediaPlayerEvent);

        // The flags are handled in the Update() function
        _AutoStartTriggered = true; // Will play when the media is ready to play
    }

    partial void OnUpdate()
    {
        //printMediaPlaybackStatus();

        if (_mediaPlayer == null || _mediaPlayer.Control == null || !_mediaPlayer.Control.CanPlay())
            return;

        // Check the buffering status
        if (_mediaPlayer.Control.IsBuffering() != _IsBuffering)
        {
            _IsBuffering = _mediaPlayer.Control.IsBuffering();
            if (_IsBuffering)
            {
                if (OnBuffering != null)
                    OnBuffering();
            }
            else
            {
                if (OnReadyToPlay != null)
                    OnReadyToPlay();
            }
        }

        // Check the volume
        if (_mediaPlayer.Control.IsMuted() != _Muted)
        {
            _mediaPlayer.Control.MuteAudio(_Muted);
        }
    }

    // Update the texture on the video screen
    partial void OnLateUpdate()
    {
        if (_mediaPlayer != null && _mediaPlayer.TextureProducer != null)
        {
            Texture texture = _mediaPlayer.TextureProducer.GetTexture();
            if (texture != null)
            {
                bool requiresYFlip = _mediaPlayer.TextureProducer.RequiresVerticalFlip();

                applyTextureToMesh(_leftMediaRenderer, texture, _leftEyeScale, _leftEyeOffset, requiresYFlip);

                applyTextureToMesh(_rightMediaRenderer, texture, _rightEyeScale, _rightEyeOffset, requiresYFlip);
            }
        }
    }

    private void applyTextureToMesh(Renderer renderer, Texture texture, Vector2 scale, Vector2 offset, bool requiresYFlip)
    {
        if (renderer == null)
            return;

        Material material = renderer.material;
        if (material == null)
            return;

        material.mainTexture = texture;
        if (requiresYFlip)
        {
            material.mainTextureScale = new Vector2(scale.x, -scale.y);
            material.mainTextureOffset = Vector2.up + offset;
        }
        else
        {
            material.mainTextureScale = scale;
            material.mainTextureOffset = offset;
        }
    }

    private void printMediaProperties()
    {
        if (_mediaPlayer != null && _mediaPlayer.Info != null)
        {
            int width = _mediaPlayer.Info.GetVideoWidth();
            int height = _mediaPlayer.Info.GetVideoHeight();
            long durationSeconds = (long)(_mediaPlayer.Info.GetDurationMs() / 1000f);
            float frameRate = _mediaPlayer.Info.GetVideoFrameRate();
            float displayRate = _mediaPlayer.Info.GetVideoDisplayRate();
            LogDebugMessage("Media Info: Width=" + width + ", Height=" + height + ", Duration(sec)=" + durationSeconds);
            LogDebugMessage("            Frame Rate=" + frameRate + ", DisplayR ate=" + displayRate);
        }
    }

    private void printMediaPlaybackStatus()
    {
        if (_mediaPlayer != null && _mediaPlayer.Control != null && _mediaPlayer.Info != null)
        {
            int index = _mediaPlayer.Control.GetCurrentVideoTrack();
            float startTimeMs = 0.0f;
            float endTimeMs = 0.0f;
            _mediaPlayer.Control.GetBufferedTimeRange(index, ref startTimeMs, ref endTimeMs);

            LogDebugMessage("Playback Status: IsPlaying=" + _mediaPlayer.Control.IsPlaying() + ", IsBuffering=" + _mediaPlayer.Control.IsBuffering());
            LogDebugMessage("                 Duration=" + _mediaPlayer.Info.GetDurationMs() / 1000f + ", Current Time=" + _mediaPlayer.Control.GetCurrentTimeMs() / 1000f);
            LogDebugMessage("                 Buffering Status: TimeRange(sec)=(" + startTimeMs / 1000f + ", " + endTimeMs / 1000f + "), Progress=" + _mediaPlayer.Control.GetBufferingProgress() + ", RangeCount=" + _mediaPlayer.Control.GetBufferedTimeRangeCount());
        }
    }

    private void stopVideo()
    {
        if (_mediaPlayer != null)
            _mediaPlayer.Stop();
    }

    private void playVideo()
    {
        if (_mediaPlayer != null)
        {
            // Update the media path, if needed
            if (_mediaPlayer.m_VideoPath != null && !_mediaPlayer.m_VideoPath.Equals(_moviePath))
                _mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, _moviePath, false);

            _mediaPlayer.Play();
        }
    }

    private void pauseVideo()
    {
        if (_mediaPlayer != null)
            _mediaPlayer.Pause();
    }

    // Callback function to handle events fired by the media player
    public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        LogDebugMessage("MediaPlayer Event: " + et.ToString());

        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                if (_AutoStartTriggered)
                {
                    playVideo();
                    _AutoStartTriggered = false;
                }
                break;

            case MediaPlayerEvent.EventType.Started:
                break;

            case MediaPlayerEvent.EventType.FirstFrameReady:
                break;

            case MediaPlayerEvent.EventType.MetaDataReady:
                printMediaProperties();
                break;

            case MediaPlayerEvent.EventType.FinishedPlaying:
                if (OnEndOfStream != null)
                {
                    OnEndOfStream();
                }
                break;

            case MediaPlayerEvent.EventType.Closing:
                break;

            case MediaPlayerEvent.EventType.Error:
                if (OnError != null)
                {
                    if (errorCode == ErrorCode.LoadFailed)
                        OnError(2, 8, "LoadError");
                    else
                        OnError(2, 1, "Unknown");
                }
                break;
        }
    }
}
#endif
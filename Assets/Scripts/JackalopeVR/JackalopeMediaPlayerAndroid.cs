using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

#if (UNITY_ANDROID) && (!UNITY_EDITOR)

public partial class JackalopeMediaPlayer : MonoBehaviour {

    private AndroidJavaObject   _mediaPlayer = null;
    private AndroidJavaObject   _unityActivityContext = null;

    [DllImport("JackalopeUnityRenderer")]
    private static extern void JRP_InitializeRenderer();

    [DllImport("JackalopeUnityRenderer")]
    private static extern void JRP_SetEventBase(int eventBase);

    [DllImport("JackalopeUnityRenderer")]
    private static extern IntPtr JRP_GetMediaSurfaceObject();

    [DllImport("JackalopeUnityRenderer")]
    private static extern IntPtr JRP_GetMediaSurfaceTextureId();

    [DllImport("JackalopeUnityRenderer")]
    private static extern void JRP_SetMediaSurfaceTextureParms(int texWidth, int texHeight);

    private delegate void PlaybackEndedAction();
    private event PlaybackEndedAction OnPlaybackEnded;

    private MediaPlaybackState getPlaybackState()
    {
        if (_mediaPlayer != null)
        {
            bool isPlaying = _mediaPlayer.Call<bool> ("getPlayWhenReady");
            if (isPlaying)
                return MediaPlaybackState.Playing;
            else
                return MediaPlaybackState.Paused;
        }

        return MediaPlaybackState.Ended;
    }

    private long getDuration()
    {
        if (_mediaPlayer != null)
            return _mediaPlayer.Call<long> ("getDuration");

        return -1;
    }

    private long getCurrentPosition()
    {
        if (_mediaPlayer != null)
            return _mediaPlayer.Call<long> ("getCurrentPosition");
        return -1;
    }

    private void setCurrentPosition(long pos)
    {
        if (_mediaPlayer != null)
            _mediaPlayer.Call ("seekTo", pos);
    }

    private void playVideo()
    {
        if (_mediaPlayer != null)
            _mediaPlayer.Call ("setPlayWhenReady", true);
    }

    private void pauseVideo()
    {
        if (_mediaPlayer != null)
        {
            try
            {
                _mediaPlayer.Call("setPlayWhenReady", false);
            }
            catch (Exception e)
            {
                LogErrorMessage("Failed to start/pause mediaPlayer with message " + e.Message);
            }
        }
    }

    private void startVideo()
    {
        _mediaPlayer = startVideoPlayerOnTextureId(_textureWidth, _textureHeight, _mediaFullPath);

        OnPlaybackEnded += onPlaybackEnded;
    }

    private void stopVideo()
    {
        if (_mediaPlayer != null)
        {
            if (_isLive || !_saveLastPosition)
                _savedPosition = -1;
            else
                _savedPosition = Position;

            _mediaPlayer.Call ("release");

            // This will trigger the shutdown on the render thread
            IssuePluginEvent(MediaRendererEventType.Shutdown);

            OnPlaybackEnded -= onPlaybackEnded;
            _mediaPlayer = null;
        }
    }

    private void onPlaybackEnded()
    {
        if (_loop)
        {
            setCurrentPosition(0);
            playVideo();
        }
        else
        {
            pauseVideo();
        }
    }

    /// <summary>
    /// Set up the video player with the movie surface texture id.
    /// </summary>
    AndroidJavaObject startVideoPlayerOnTextureId(int texWidth, int texHeight, string mediaPath)
    {
        Debug.Log("MoviePlayer: StartVideoPlayerOnTextureId");

        Native_SetMediaTextureParam(_textureWidth, _textureHeight);

        IntPtr androidSurface = Native_GetMediaSurfaceObject();

        // Get the Unity context
        using (AndroidJavaClass activityClass = new AndroidJavaClass("com/unity3d/player/UnityPlayer")) {
            _unityActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            if (_unityActivityContext == null)
                LogErrorMessage("MoviePlayer: Fail to get the activity context");
        }

        // Instantiate the ExoPlayer
        AndroidJavaObject mediaPlayer = new AndroidJavaObject("com/vokevr/android/exoplayerbridge/ExoMediaPlayer");
        if (mediaPlayer == null)
            LogErrorMessage("MoviePlayer: Fail to create a media player");

        // Pass the Unity context to the ExoPlayer library
        mediaPlayer.Call("setContext", _unityActivityContext);

        // Initialize the ExoPlayer
        mediaPlayer.Call("init", mediaPath);

        // Set the surface
        IntPtr setSurfaceMethodId = AndroidJNI.GetMethodID(mediaPlayer.GetRawClass(),"setSurface","(Landroid/view/Surface;)V");
        jvalue[] parms = new jvalue[1];
        parms[0] = new jvalue();
        parms[0].l = androidSurface;
        AndroidJNI.CallVoidMethod(mediaPlayer.GetRawObject(), setSurfaceMethodId, parms);

        try
        {
            // Register listeners called back from the ExoPlayer
            mediaPlayer.Call("addListener", new Listener(this));
            mediaPlayer.Call("setInfoListener", new InfoListener());
            mediaPlayer.Call("setMetadataListener", new OnId3MetadataListener(this));

            // Start the player
            mediaPlayer.Call("prepare");
            mediaPlayer.Call("setPlayWhenReady", true);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to start mediaPlayer with message " + e.Message);
        }

        return mediaPlayer;
    }

    // Defines the CallBack classes
    class Listener : AndroidJavaProxy
    {
		private JackalopeMediaPlayer _player;
    	
        public Listener(JackalopeMediaPlayer player) : base("com.vokevr.android.exoplayerbridge.ExoMediaPlayer$Listener")
        {
        	_player = player;
        }

        void onStateChanged(bool playWhenReady, int playbackState)
        {
            switch ((MediaPlaybackState)playbackState)
            {
                case MediaPlaybackState.Idle:
                    break;

                case MediaPlaybackState.Preparing:
					if (_player.OnBuffering != null)
					_player.OnBuffering();
                    break;

                case MediaPlaybackState.Buffering:
					if (_player.OnBuffering != null)
					_player.OnBuffering();
                    break;

                case MediaPlaybackState.Ready:
					if (_player.OnReadyToPlay != null)
					_player.OnReadyToPlay();
                    break;

                case MediaPlaybackState.Ended:
					if (_player.OnEndOfStream != null)
					_player.OnEndOfStream();
					if (_player.OnPlaybackEnded != null)
					_player.OnPlaybackEnded();
                    break;
            }
        }

        void onError(AndroidJavaObject exception)
        {
            string errMessage = exception.Call<string>("getMessage");
Debug.LogError("MoviePlayer: " + _player._moviePath + "          " + errMessage);
            if (_player.OnError != null)
                _player.OnError(2, 1, errMessage);
        }

        void onVideoSizeChanged(int width, int height, int unappliedRotationDegrees, float pixelWidthHeightRatio)
        {
        }
    }

    class InfoListener : AndroidJavaProxy
    {
        public InfoListener() : base("com.vokevr.android.exoplayerbridge.ExoMediaPlayer$InfoListener")
        {
        }

		void onVideoFormatChanged(String mimeType, int bitrate, int videoWidth, int videoHeight, float frameRate)
        {
			Debug.Log("MoviePlayer: onVideoFormatChanged. MimeType=" + mimeType + ", Width=" + videoWidth + ", Height=" + videoHeight);
        }

		void onAudioFormatChanged(String mimeType, int channelCount, int sampleRate)
        {
			Debug.Log("MoviePlayer: onAudioFormatChanged. MimeType=" + mimeType + ", ChannelCount=" + channelCount + ", SampleRate=" + sampleRate);
        }

        void onDroppedFrames(int count, long elapsed)
        {
            Debug.Log("MoviePlayer: onDroppedFrame: count=" + count + ", Elapsed=" + elapsed);
        }

        void onBandwidthSample(int elapsedMs, long bytes, long bitrateEstimate)
        {
            Debug.Log("MoviePlayer: onBandwidthSample. ElapsedMs=" + elapsedMs + ", Bytes=" + bytes + ", BitrateEstimate=" + bitrateEstimate);
        }

        void onLoadStarted(int sourceId, long length, int type, int trigger, AndroidJavaObject format,
            long mediaStartTimeMs, long mediaEndTimeMs)
        {
			Debug.Log("MoviePlayer: onLoadStarted. Length=" + length + ", StartTimeMs=" + mediaStartTimeMs + ", EndTimeMs=" + mediaEndTimeMs);
        }

        void onLoadCompleted(int sourceId, long bytesLoaded, int type, int trigger, AndroidJavaObject format,
            long mediaStartTimeMs, long mediaEndTimeMs, long elapsedRealtimeMs, long loadDurationMs)
        {
            Debug.Log("MoviePlayer: onLoadCompleted");
        }

        void onDecoderInitialized(String decoderName, long elapsedRealtimeMs, long initializationDurationMs)
        {
            Debug.Log("MoviePlayer: onDecoderInitialized");
        }

		void onTimelineChanged(long durationMs, long defaultPositionMs, long startTimeMs, bool isSeekable, bool isDynamic)
        {
			Debug.Log("MoviePlayer: onTimelineChanged. DurationMs=" + durationMs + ", DefaultPositionMs=" + defaultPositionMs + 
						",startTimeMs=" + startTimeMs + ", isSeekable=" + isSeekable + ", isDynamic=" + isDynamic);
        }
    }

    class InternalErrorListener : AndroidJavaProxy
    {
		private JackalopeMediaPlayer _player;

        public InternalErrorListener(JackalopeMediaPlayer player) : base("com.vokevr.android.exoplayerbridge.ExoMediaPlayer$InternalErrorListener")
        {
        	_player = player;
        }

        void onRendererInitializationError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 2, "RendererInitializationError");
        }

        void onAudioTrackInitializationError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 3, "AudioTrackInitializationError");
        }

        void onAudioTrackWriteError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 4, "AudioTrackWriteError");
        }

        void onAudioTrackUnderrun(int bufferSize, long bufferSizeMs, long elapsedSinceLastFeedMs)
        {
			if (_player.OnError != null)
			_player.OnError(2, 5, "AudioTrackUnderrun");
        }

        void onDecoderInitializationError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 6, "DecoderInitializationError");
        }

        void onCryptoError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 7, "CryptoError");
       }

        void onLoadError(int sourceId, AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 8, "LoadError");
        }

        void onDrmSessionManagerError(AndroidJavaObject exception)
        {
			if (_player.OnError != null)
			_player.OnError(2, 9, "DrmSessionManagerError");
        }
    }

    class OnId3MetadataListener : AndroidJavaProxy
    {
    	private JackalopeMediaPlayer _player;

        public OnId3MetadataListener(JackalopeMediaPlayer player) : base("com.vokevr.android.exoplayerbridge.ExoMediaPlayer$Id3MetadataListener")
        {
        	_player = player;
        }

        void onUserDefinedTextMetadata(string id, string value, string description)
        {
//			Debug.Log("Timed Metadata: id=" + id + ", value= " + value + ", description=" + description);

			if (_player.OnUserDefinedTextMetadata != null)
			_player.OnUserDefinedTextMetadata(id, value, description);
        }
    }
}

#endif

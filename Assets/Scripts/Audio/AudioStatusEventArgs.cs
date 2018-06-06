using System;

public class AudioStatusEventArgs : EventArgs
{
    public bool IsAudioOn { get; private set; }
    public AudioStatusEventArgs(bool isAudioOn)
    {
        IsAudioOn = isAudioOn;
    }
}
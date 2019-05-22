using System;

public class UserPresenceEventArgs : EventArgs
{
    public bool IsPresent { get; private set; }
    public UserPresenceEventArgs(bool isPresent)
    {
        IsPresent = isPresent;
    }
}
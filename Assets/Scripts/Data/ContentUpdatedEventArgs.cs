using System;

public class ContentUpdatedEventArgs<T> : EventArgs
{
    public T Content { get; set; }

    public ContentUpdatedEventArgs(T content)
    {
        Content = content;
    }
}
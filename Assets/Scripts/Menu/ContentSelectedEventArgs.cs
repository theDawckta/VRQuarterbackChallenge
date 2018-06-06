using System;

public class ContentSelectedEventArgs : EventArgs
{
    public ContentViewModel SelectedContent { get; set; }
    public ContentSelectedEventArgs(ContentViewModel selectedContent)
    {
        SelectedContent = selectedContent;
    }
}
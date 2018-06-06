using System;

public class TweetInfo
{
    public long TweetID { get; set; }
    public string TweetTxt { get; set; }
    public long TweetUserID { get; set; }
    public string TweetName { get; set; }
    public string TweetScreenName { get; set; }
    public string TweetURL { get; set; }
    public string TweetPicURL { get; set; }
    public int FreezeStatus { get; set; }
    public int EventID { get; set; }
}
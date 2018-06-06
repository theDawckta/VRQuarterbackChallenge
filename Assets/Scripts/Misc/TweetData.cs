using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VOKE.VokeApp.Util;

public class TweetData : MonoBehaviour
{
    //	private string TWITTER_URL = "http://35.164.33.251/voketweetstore/api/1.0/tweet/gettweets?event_id=2";
    [HideInInspector]
    public string TWITTER_URL;

    [HideInInspector]
    public List<TweetInfo> TweetInfos;

    public delegate void UpdateTweetFields(List<TweetInfo> tweetInfo);
    public static event UpdateTweetFields OnUpdateTweetFields;

    public bool isFailedToDownload;

    // Use this for initialization
    void Awake()
    {
        TweetInfos = new List<TweetInfo>();
    }

    void Init()
    {
        TweetInfos.Clear();
    }

    /// <summary>
    /// Called to download the JSON file based on the game and league played
    /// </summary>
    public IEnumerator DownloadJSONFileTweetInfo()
    {
        // Download the text from end point api
        WWW www = new WWW(TWITTER_URL);
        yield return www;

        if (!String.IsNullOrEmpty(www.error))
        {
            Trace.Error("Fail to get the JSON file from " + TWITTER_URL + ". Error=" + www.error);
            isFailedToDownload = true;
            yield break;
        }

        processJsonString(www.text);
        Trace.Log("Finished parsing the JSON file.");
        OnUpdateTweetFields(TweetInfos);
        isFailedToDownload = false;
    }

    private void processJsonString(string jsonString)
    {
        /***
		 * {
		 * 	"tweet_id":"808364372716388352",
		 * 	"tweet_text":"@2SMART4 @NBA \u270b",
		 * 	"tweet_user_id":"74593548",
		 * 	"tweet_name":"Tommy",
		 * 	"tweet_screen_name":"tommyjets",
		 * 	"tweet_url":"https:\/\/twitter.com\/tommyjets\/status\/808364372716388352",
		 * 	"tweet_profile_pic":"http:\/\/pbs.twimg.com\/profile_images\/795974994635452416\/TgDR7Yi2_normal.jpg",
		 * 	"tweet_created_at":"2016-12-12 17:34:19",
		 * 	"freeze_status":"0",
		 * 	"event_id":"0",
		 * 	"image_media":[]
		 * 	}
         ***/
        JSONObject jsonRoot = new JSONObject(jsonString);
        for (int j = 0; j < jsonRoot.list.Count; j++)
        {
            JSONObject jsonTweetInfos = jsonRoot.list[j];
            TweetInfo tweetInfo = parseTweetInfoData(jsonTweetInfos);
            //			AddTweetInfo(tweetInfo);
            if (!TweetInfos.Contains(tweetInfo))
                TweetInfos.Add(tweetInfo);
        }
    }

    private TweetInfo parseTweetInfoData(JSONObject jsonTweetInfos)
    {
        TweetInfo tweetInfo = new TweetInfo();

        for (int i = 0; i < jsonTweetInfos.list.Count; i++)
        {
            JSONObject jsonTweetInfo = jsonTweetInfos.list[i];
            string key = jsonTweetInfos.keys[i];
            if (jsonTweetInfo.type == JSONObject.Type.STRING)
            {
                /***
				 * {
				 * 	"tweet_id":"808364372716388352",
				 * 	"tweet_text":"@2SMART4 @NBA \u270b",
				 * 	"tweet_user_id":"74593548",
				 * 	"tweet_name":"Tommy",
				 * 	"tweet_screen_name":"tommyjets",
				 * 	"tweet_url":"https:\/\/twitter.com\/tommyjets\/status\/808364372716388352",
				 * 	"tweet_profile_pic":"http:\/\/pbs.twimg.com\/profile_images\/795974994635452416\/TgDR7Yi2_normal.jpg",
				 * 	"tweet_created_at":"2016-12-12 17:34:19",
				 * 	"freeze_status":"0",
				 * 	"event_id":"0",
				 * 	"image_media":[]
				 * 	}
		         ***/

                string value = jsonTweetInfo.str;
                switch (key)
                {
                    case "tweet_id":
                        {
                            tweetInfo.TweetID = long.Parse(value);
                            break;
                        }
                    case "tweet_text":
                        {
                            tweetInfo.TweetTxt = value.Replace("\\", ""); ;
                            break;
                        }
                    case "tweet_user_id":
                        {
                            tweetInfo.TweetUserID = long.Parse(value);
                            break;
                        }
                    case "tweet_name":
                        {
                            tweetInfo.TweetName = value;
                            break;
                        }
                    case "tweet_screen_name":
                        {
                            tweetInfo.TweetScreenName = value;
                            break;
                        }
                    case "tweet_url":
                        {
                            tweetInfo.TweetURL = value.Replace("\\", "");
                            break;
                        }
                    case "tweet_profile_pic":
                        {
                            tweetInfo.TweetPicURL = value.Replace("\\", "");
                            break;
                        }
                    case "freeze_status":
                        {
                            tweetInfo.FreezeStatus = int.Parse(value);
                            break;
                        }
                    case "event_id":
                        {
                            tweetInfo.EventID = int.Parse(value);
                            break;
                        }
                }
            }
        }
        return tweetInfo;
    }
}
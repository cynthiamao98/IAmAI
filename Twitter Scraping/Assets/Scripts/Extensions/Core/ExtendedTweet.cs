using System;
using Twity.DataModels.Core;

[Serializable]
public class ExtendedTweet : ExtendedTweetObjectWithUser
{
    public TweetObjectWithUser retweeted_status;
    //public string full_text;
}

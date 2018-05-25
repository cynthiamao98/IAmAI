using System;
using Twity.DataModels.Core;

[Serializable]
public class ExtendedTweetObjectWithUser : ExtendedTweetObject
{
    public TweetUser user;
}
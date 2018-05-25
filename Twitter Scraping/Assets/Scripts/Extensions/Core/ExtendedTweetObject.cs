﻿using System;
using Twity.DataModels.Entities;
using Twity.DataModels.Core;

[Serializable]
public class ExtendedTweetObject
{
    public long id;
    public string id_str;
    public string created_at;
    public string full_text;
    public bool truncated;

    public string in_reply_to_screen_name;
    public int in_reply_to_status_id;
    public string in_reply_to_status_id_str;
    public int in_reply_to_user_id;
    public string in_reply_to_user_id_str;
    public bool retweeted;
    public int retweet_count;
    public bool favorited;
    public int favorite_count;
    public bool is_quote_status;
    public int quoted_status_id;
    public string quoted_status_id_str;

    public string lang;

    public Entities entities;
    public Extended_Entities extended_entities;
}

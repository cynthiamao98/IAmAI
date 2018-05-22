using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Twity.DataModels.Core;
using Twity.DataModels.Responses;
using System.Text.RegularExpressions;
using System.Linq;

public class TwitterScraper : MonoBehaviour
{

    //Store data here, access from other code.
    public List<Tweet> _SearchTweetText = new List<Tweet>();
    public List<Trend> _Trends = new List<Trend>();
    public TweetUser _User = new TweetUser();
    public List<TweetObject> _UserTimelineTweets = new List<TweetObject>();
	  public string handle;

    public string UserTimelineTweetsJSON = "";
    //...etc

    bool AllScrapeJobsStarted = false;
    Dictionary<string, bool> ScrapingComplete = new Dictionary<string, bool>();

    public bool FollowingUser { get; set;}
  	public string ck  = null;
  	public string cs  = null;
  	public string at  = null;
  	public string ats = null;

    public void Update()
    {
		InitCreds ();
		handle =  ("Oprah");
		Scrape(handle);

		if (IsFinishedScraping()){}
        	UnityEngine.Debug.Log("Finished Scraping");
    }

    private void InitCreds()
    {
      // UnityEngine.Debug.Log("INITIALIZING");

      string filename = Application.dataPath + "/Watson/serviceCredentials/twitterCredentials.txt";
      UnityEngine.Debug.Log(filename);

      var i = 0 ;
      string[] lines = System.IO.File.ReadAllLines(filename);

      foreach (var line in lines)
      {
        if (i == 0)
          ck = line;
        if (i == 1)
          cs = line;
        if (i == 2)
          at = line;
        if (i == 3)
          ats = line;

        i++;

      }

	  InitOAuth(ck, cs, at, ats);

    }

    public void InitOAuth(string ck, string cs, string at, string ats)
    {
        Twity.Oauth.consumerKey = ck;
        Twity.Oauth.consumerSecret = cs;
        Twity.Oauth.accessToken = at;
        Twity.Oauth.accessTokenSecret = ats;
    }

    public bool IsFinishedScraping()
    {
        return AllScrapeJobsStarted && ScrapingComplete.Values.All(v => v == true);
    }

    public void FollowUser(string handle)
    {
        if (!string.IsNullOrEmpty(handle))
        {
            FollowingUser = false;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["screen_name"] = handle;
            StartCoroutine(Twity.Client.Post("friendships/create", parameters, CB_PostFollow));
        }
    }

    // All API Calls go here
    public void Scrape(string handle)
    {
        if (!string.IsNullOrEmpty(handle))
        {
            Dictionary<string, string> parameters;

            //Finished["CB_SearchTweets"] = false;
            //parameters = new Dictionary<string, string>();
            //parameters["q"] = "#blacklivesmatter";
            //parameters["count"] = 10.ToString();
            //parameters["tweet_mode"] = "extended";
            //StartCoroutine(Twity.Client.Get("search/tweets", parameters, CB_SearchTweets));

            //Finished["CB_GetTrends"] = false;
            //parameters = new Dictionary<string, string>();
            //parameters["id"] = 1.ToString();
            //StartCoroutine(Twity.Client.Get("trends/place", parameters, CB_GetTrends));

            //Finished["CB_GetUser"] = false;
            //parameters = new Dictionary<string, string>();
            //parameters["screen_name"] = handle;
            //StartCoroutine(Twity.Client.Get("users/show", parameters, CB_GetUser));

            ScrapingComplete["CB_GetUserTimeline"] = false;
            //ScrapingComplete["CB_GetHashtags"] = false;
            parameters = new Dictionary<string, string>();
            parameters["screen_name"] = handle;
            parameters["count"] = 100.ToString();
            parameters["include_rts"] = false.ToString();
            StartCoroutine(Twity.Client.Get("statuses/user_timeline", parameters, CB_GetUserTimeline));

            //Finished["CB_GetFriendsIDs"] = false;
            //parameters = new Dictionary<string, string>();
            //parameters["screen_name"] = handle;
            //parameters["count"] = 10.ToString();
            //StartCoroutine(Twity.Client.Get("friends/list", parameters, CB_GetFriendsIDs));

            //Finished["CB_GetFollowersIDs"] = false;
            //parameters = new Dictionary<string, string>();
            //parameters["screen_name"] = handle;
            //parameters["count"] = 10.ToString();
            //StartCoroutine(Twity.Client.Get("followers/list", parameters, CB_GetFollowersIDs));

            AllScrapeJobsStarted = true;
        }
        else
        {
            Debug.Log("Error: Empty handle");
        }
    }

    void CB_PostFollow(bool success, string response)
    {
        if (success)
        {
            TweetUser Response = JsonUtility.FromJson<TweetUser>(response);
            Debug.Log("Successfully followed " + Response.screen_name);
            FollowingUser = true;
        }
        else { Debug.Log("Error: Following User failed"); }
    }

    void CB_SearchTweets(bool success, string response)
    {
        if (success)
        {
            SearchTweetsResponse Response = JsonUtility.FromJson<SearchTweetsResponse>(response);
            foreach (var tweet in Response.statuses)
            {
                if (tweet.full_text != null)
                {
                    _SearchTweetText.Add(tweet);
                    Debug.Log(tweet.retweeted_status.full_text);
                } else {
                    _SearchTweetText.Add(tweet);
                    Debug.Log(tweet.text);
                }
            }
        }
        else{Debug.Log(response);}
        ScrapingComplete["CB_SearchTweets"] = true;
    }

    //Trends is stupid and is returned as a top level array in JSON with one element so
    // I just chop off  <<{"items":[>> and  <<]}>> (without angle brackets) which is tacked
    // on by JsonHelper.cs so JsonUtility.FromJson() can handle it properly.
    void CB_GetTrends(bool success, string response)
    {
        if (success)
        {
            int startIndex = 10;
            int endIndex = response.Length - 3;
            int length = endIndex - startIndex + 1;
            response = response.Substring(startIndex, length);
            TrendsResponse Response = JsonUtility.FromJson<TrendsResponse>(response);
            foreach (Trend trend in Response.trends)
            {
                _Trends.Add(trend);
                Debug.Log(trend.name);
            }
        }
        else { Debug.Log(response); }
        ScrapingComplete["CB_GetTrends"] = true;
    }

    void CB_GetUser(bool success, string response)
    {
        if (success)
        {
            TweetUser Response = JsonUtility.FromJson<TweetUser>(response);
            _User = Response;
            Debug.Log(Response.friends_count);
        }
        else { Debug.Log(response); }
        ScrapingComplete["CB_GetUser"] = true;
    }

    void CB_GetUserTimeline(bool success, string response)
    {
        if (success)
        {
            UserTimelineTweetsJSON = response;

			System.IO.File.WriteAllText(@"/Users/anazeneli/Desktop/CGUI/cgui_amir/CGUI-Watson-Analysis/WatsonAnalysis/Assets/Watson/Examples/ServiceExamples/TestData" + handle + "-timeline.json", UserTimelineTweetsJSON);

            StatusesUserTimelineResponse Response = JsonUtility.FromJson<StatusesUserTimelineResponse>(response);
            foreach (Tweet tweet in Response.items)
            {
                if (tweet.full_text != null) {
                    _UserTimelineTweets.Add(tweet);
//                    Debug.Log(tweet.retweeted_status.full_text);
                } else {
                    _UserTimelineTweets.Add(tweet);
//                    Debug.Log(tweet.text);
                }
            }
        }
        else { Debug.Log(response); }
        ScrapingComplete["CB_GetUserTimeline"] = true;

        GetHashtags();
    }

    void CB_GetFriendsIDs(bool success, string response)
    {
        if (success)
        {
            FriendsListResponse Response = JsonUtility.FromJson<FriendsListResponse>(response);
            foreach (TweetUser tweet in Response.users)
            {
                Debug.Log(tweet.name);
            }
        }
        else { Debug.Log(response); }
        ScrapingComplete["CB_GetFriendsIDs"] = true;
    }

    void CB_GetFollowersIDs(bool success, string response)
    {
        if (success)
        {
            FollowersListResponse Response = JsonUtility.FromJson<FollowersListResponse>(response);
            foreach (TweetUser tweet in Response.users)
            {
                Debug.Log(tweet.name);
            }
        }
        else { Debug.Log(response); }
        ScrapingComplete["CB_GetFollowersIDs"] = true;
    }

    void GetHashtags()
    {
        Dictionary<string, int> hashtagCounts = new Dictionary<string, int>();
        foreach (Tweet tweet in _UserTimelineTweets){
            List<string> hashtags = (from h in tweet.entities.hashtags
                                     select "#" + h.text.ToLower()).ToList();
            foreach (string h in hashtags)
            {
                if (hashtagCounts.ContainsKey(h))
                    hashtagCounts[h] += 1;
                else
                    hashtagCounts.Add(h, 1);
            }
        }

        List<KeyValuePair<string, int>> topTenHashtags = hashtagCounts.ToList();
        topTenHashtags.Sort((h1, h2) => h2.Value.CompareTo(h1.Value));
        foreach (KeyValuePair<string, int> h in topTenHashtags)
        {
//            Debug.Log(h.Key + " " + h.Value);
        }
        ScrapingComplete["CB_GetHashtags"] = true;
    }
}

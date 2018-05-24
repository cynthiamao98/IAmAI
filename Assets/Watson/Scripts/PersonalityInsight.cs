/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.PersonalityInsights.v3;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Connection;


public class PersonalityInsight : MonoBehaviour
{
	private string _username = null;
	private string _password = null;
	private string _url      = null;

	private PersonalityInsights _personalityInsights;
	private string _personalityInsightsVersionDate = "2017-05-26";

	private string _testString;
	TwitterScraper timeline;
	private string user;
	private string twitter_user;

	private string _dataPath;

	private bool _getProfileTextTested = false;
	private bool _getProfileJsonTested = false;

	void Start()
	{
		InitializeCredentials();

		// Find user handle to locate user info
		timeline = GetComponent<TwitterScraper> ();
		twitter_user = timeline.handle; //"Oprah";
		user = twitter_user;

		LogSystem.InstallDefaultReactors();
		_dataPath= Application.dataPath + "/Watson/TestData/" + twitter_user + "_timeline.json";
		_testString = System.IO.File.ReadAllText(_dataPath);

		//  Create credential and instantiate service
		Credentials credentials = new Credentials(_username, _password, _url);
		_personalityInsights = new PersonalityInsights(credentials);
		_personalityInsights.VersionDate = _personalityInsightsVersionDate;

		Runnable.Run(Examples());
	}

	private void InitializeCredentials()
	{
		string filename = Application.dataPath + "/Watson/serviceCredentials/insightsCredentials.txt";
		UnityEngine.Debug.Log (filename);

		var i = 0;
		string[] lines = System.IO.File.ReadAllLines (filename);
		UnityEngine.Debug.Log (lines [2]);

		foreach (var line in lines) {
			if (i == 0)
				_username = line;
			if (i == 1)
				_password = line;
			if (i == 2)
				_url = line;

			i++;

		}
	}

	private IEnumerator Examples()
	{
		if (!_personalityInsights.GetProfile(OnGetProfileJson, OnFail, _dataPath, ContentType.TextHtml, ContentLanguage.English, ContentType.ApplicationJson, AcceptLanguage.English, true, true, true))
			Log.Debug("ExamplePersonalityInsights.GetProfile()", "Failed to get profile!");
		while (!_getProfileJsonTested)
			yield return null;

		if (!_personalityInsights.GetProfile(OnGetProfileText, OnFail, _testString, ContentType.TextHtml, ContentLanguage.English, ContentType.ApplicationJson, AcceptLanguage.English, true, true, true))
			Log.Debug("ExamplePersonalityInsights.GetProfile()", "Failed to get profile!");
		while (!_getProfileTextTested)
			yield return null;

		Log.Debug("ExamplePersonalityInsights.Examples()", "Personality insights examples complete.");
	}

	private void OnGetProfileText(Profile profile, Dictionary<string, object> customData)
	{
		Log.Debug("ExamplePersonaltyInsights.OnGetProfileText()", "Personality Insights - GetProfileText Response: {0}", customData["json"].ToString());
		System.IO.File.WriteAllText(Application.dataPath + "/Watson/TestData/" + user + "_results.json", customData["json"].ToString());
		_getProfileTextTested = true;
	}

	private void OnGetProfileJson(Profile profile, Dictionary<string, object> customData)
	{
		Log.Debug("ExamplePersonaltyInsights.OnGetProfileJson()", "Personality Insights - GetProfileJson Response: {0}", customData["json"].ToString());
		_getProfileJsonTested = true;
	}

	private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
	{
		Log.Error("ExamplePersonaltyInsights.OnFail()", "Error received: {0}", error.ToString());
	}
}

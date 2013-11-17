using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public class DatabaseHandler : MonoBehaviour {

	public string getURL = "";
	public string postURL = "";
	private WWW www, requestWWW;

	public IDictionary questionsDict = null;

	private WWWForm answersForm = null;

	GameController _gameController = null;

	void Start () {
		StartCoroutine(LoadQuestions());

		answersForm = new WWWForm();

		_gameController = this.GetComponent<GameController>();
		if (_gameController == null) {
			_gameController = this.GetComponentInChildren<GameController>();
		}
	}

	public void ReadyData(Dictionary<string,string> dict) {
		foreach (KeyValuePair<string,string> pair in dict) {
			if (!pair.Key.Equals("") && !pair.Value.Equals("")) {
				int outValue = 0;
				if (int.TryParse(pair.Value, out outValue)) {
					answersForm.AddField(pair.Key, outValue);
				}
				else {
					answersForm.AddField(pair.Key, pair.Value);
				}
			}
		}
	}

	public void SubmitAllData() {
		answersForm.AddField("raw_time_played", Mathf.RoundToInt(_gameController.GameTime));
		answersForm.AddField("raw_wave_count", _gameController.WaveCount);

		StartCoroutine(SendForm());
	}

	IEnumerator SendForm() {
		requestWWW = new WWW(postURL, answersForm);
		
		yield return requestWWW;
		
		// Print the error to the console		
		if (!string.IsNullOrEmpty(requestWWW.error)) {			
			Debug.LogWarning("WWW request error: " + requestWWW.error);
			yield return null;
		}		
		else {				
			Debug.Log("WWW returned text: " + requestWWW.text);	
			yield return requestWWW.text;
		}
	}

	IEnumerator LoadQuestions() {
		www = new WWW(getURL);

		float elapsedTime = 0.0f;

		while (!www.isDone) {
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= 10.0f) {
				Debug.LogError("WWW request to URL: " + getURL + "\n Timed out.");
				break;
			}

			yield return null;
		}

		if (!www.isDone || !string.IsNullOrEmpty(www.error)) {
			Debug.LogError("WWW request to URL: " + getURL + " failed.\n" + www.error);
			yield break;
		}

		string response = www.text;
		Debug.Log("Received text: " + response);
		Debug.Log("WWW request (loading questions) took: " + elapsedTime.ToString() + " seconds.");

		IDictionary responseDict = (IDictionary) Json.Deserialize(response);

		questionsDict = responseDict;
	}
}

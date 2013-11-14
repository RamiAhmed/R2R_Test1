using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public class DatabaseHandler : MonoBehaviour {

	string getURL = "www.alphastagestudios.com/test/questions.json";
	string postURL = "www.alphastagestudios.com/test/answers.json";
	private WWW www;
	private WWW requestWWW;

	System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

	// Use this for initialization
	void Start () {
		StartCoroutine(SendData());
		StartCoroutine(LoadQuestions());

	}

	IEnumerator SendData() {
		string sendString = "'Demographics': {'Responses': [{'Question': 'Gender','Answer': 'Male'},{'Question': 'Age',Answer': '21-25'},'Question': 'Frequency of Playing','Answer' : 'Weekly'},{'Question': 'Amount of Playing','Answer': '1 hour or less'{'Question': 'Favourite game or game genre',}]}";

		string sendDict = Json.Serialize(sendString);

		requestWWW = new WWW(postURL, encoding.GetBytes(sendDict));

		yield return requestWWW;

		// Print the error to the console		
		if (!string.IsNullOrEmpty(requestWWW.error)) {			
			Debug.Log("request error: " + requestWWW.error);
			yield return null;
		}		
		else {				
			Debug.Log("returned data" + requestWWW.text);	
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
			Debug.LogError(string.Format("WWW request to URL: " + getURL + " failed\n{0}", www.error));
			yield break;
		}

		string response = www.text;
		Debug.Log("Received text: " + response);
		Debug.Log("WWW request took: " + elapsedTime.ToString() + " seconds.");

		IDictionary responseDict = (IDictionary) Json.Deserialize(response);

		IDictionary demographics = (IDictionary) responseDict["Demographics"];

		IList demographicsQuestions = (IList) demographics["Questions"];

		foreach (IDictionary item in demographicsQuestions) {
			string question = (string) item["Question"];
			IList options = (IList) item["Options"];
			string helperText = (string) item["HelperText"];

			Debug.Log("Question: " + question);
			Debug.Log("HelperText: " + helperText);

			foreach (string option in options) {
				Debug.Log("Option: " + option);
			}
		}

	}
}

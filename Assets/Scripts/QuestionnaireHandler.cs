﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {

	public bool QuestionnaireEnabled = true;

	public enum QuestionnaireState {
		NONE = 0,
		DEMOGRAPHICS,
		STARTING,
		DURING,
		AFTER
	};

	public QuestionnaireState CurrentState = QuestionnaireState.NONE;

	public int QuestionnaireWaveFrequency = 3;

	public float VerticalSpacing = 15f,
				TextAreaHeight = 50f,
				GUIElementHeight = 40f;
	 
	public int MaxDuringInterrupts = 3;

	private int currentDuring = 1;

	private DatabaseHandler dbHandler = null;
	private GameController _gameController = null;

	private bool showingQuestionnaire = false; 

	private IDictionary questionsDict = null;
	private Dictionary<string,string> answersDict = null;

	private Dictionary<string,int> selectionDict = null;
	private Dictionary<string,string> textDict = null;

	private Rect questionnaireRect;
	
	void Start () {
		if (QuestionnaireEnabled) {
			dbHandler = this.GetComponent<DatabaseHandler>();
			if (dbHandler == null) {
				dbHandler = this.GetComponentInChildren<DatabaseHandler>();
			}

			_gameController = this.GetComponent<GameController>();
			if (_gameController == null) {
				_gameController = this.GetComponentInChildren<GameController>();
			}

			answersDict = new Dictionary<string,string>();
			selectionDict = new Dictionary<string,int>();
			textDict = new Dictionary<string,string>();

			float width = 300f, height = 150f,
				screenWidth = Screen.width * 0.9f,
				screenHeight = Screen.height * 0.9f;

			width = screenWidth > width ? screenWidth : width;
			height = screenHeight > height ? screenHeight : height;

			questionnaireRect = new Rect((Screen.width/2f) - (width/2f), (Screen.height/2f) - (height/2f), width, height);

			CurrentState++;
		}
	}

	void Update () {
		if (QuestionnaireEnabled) {
			if (_gameController.CurrentGameState == GameController.GameState.QUESTIONNAIRE) {
				if (!showingQuestionnaire) {
					showingQuestionnaire = true;

					if (questionsDict == null && dbHandler.questionsDict != null) {
						questionsDict = dbHandler.questionsDict;
					}

					if (questionsDict == null) {
						showingQuestionnaire = false;
					}
				}
			}
			else {
				if (showingQuestionnaire) {
					showingQuestionnaire = false;
				}
			}
		}
	}

	void OnGUI() {
		if (QuestionnaireEnabled) {
			if (showingQuestionnaire) {
				questionnaireRect = GUILayout.Window(0, questionnaireRect, DrawQuestionnaire, "");
				GUI.BringWindowToFront(0);
			}
		}
	}

	private void DrawQuestionnaire(int windowID) {
		if (!GUI.skin.box.wordWrap) {
			GUI.skin.box.wordWrap = true;
		}

		GUILayout.BeginVertical();

		switch (CurrentState) {
			case QuestionnaireState.DEMOGRAPHICS: buildDemographics(); break;
			case QuestionnaireState.STARTING: buildStarting(); break;
			case QuestionnaireState.DURING: buildDuring(); break;
			case QuestionnaireState.AFTER: buildAfter(); break;
		}

		if (GetQuestionsAnswered()) {
			GUILayout.FlexibleSpace();

			if (CurrentState != QuestionnaireState.AFTER) {
				if (GUILayout.Button("Continue", GUILayout.Height(GUIElementHeight))) {
					dbHandler.ReadyData(answersDict);
					answersDict.Clear();

					if (CurrentState == QuestionnaireState.DURING) {
						currentDuring++;
						if (currentDuring > MaxDuringInterrupts) {
							CurrentState++;
						}
					}
					else {
						CurrentState++;
					}

					_gameController.CurrentGameState = GameController.GameState.PLAY;
				}
			}
			else {
				if (GUILayout.Button("Submit Answers", GUILayout.Height(GUIElementHeight))) {
					dbHandler.ReadyData(answersDict);
					answersDict.Clear();

					dbHandler.SubmitAllData();
					QuestionnaireEnabled = false;

					_gameController.CurrentGameState = GameController.GameState.PLAY;
				}
			}
		}

		GUILayout.EndVertical();
	}

	private void buildDemographics() {
		GUILayout.Box("DEMOGRAPHICS", GUILayout.Height(GUIElementHeight));

		buildSection("Demographics");
	}

	private void buildStarting() {
		GUILayout.Box("BEFORE STARTING", GUILayout.Height(GUIElementHeight));

		buildSection("Starting");
	}

	private void buildDuring() {
		GUILayout.Box("DURING " + currentDuring.ToString(), GUILayout.Height(GUIElementHeight));

		buildSection("During");
	}

	private void buildAfter() {
		GUILayout.Box("AFTER", GUILayout.Height(GUIElementHeight));

		buildSection("After");
	}

	private void buildSection(string section) {
		IList sectionList = (IList) questionsDict[section];
		foreach (IDictionary dict in sectionList) {
			string id = (string) dict["ID"];
			string question = (string) dict["Question"];
			List<string> options = convertFromIList((IList)dict["Options"]);
			string helperText = (string) dict["HelperText"];

			if (CurrentState == QuestionnaireState.DURING) {
				id += "_" + currentDuring.ToString();
			}

			if (options != null && options.Count > 0) {
				if (!selectionDict.ContainsKey(id)) {
					selectionDict.Add(id, -1);
				}

				selectionDict[id] = addMultipleChoices(id, question, options.ToArray(), helperText, selectionDict[id]);
			}
			else {
				if (!textDict.ContainsKey(id)) {
					textDict.Add(id, "");
				}

				textDict[id] = addTextQuestion(id, question, helperText, textDict[id]);
			}
		}
	}

	private List<string> convertFromIList(IList list) {
		List<string> newList = new List<string>();
		foreach (object item in list) {
			newList.Add(item.ToString());
		}

		return newList;
	}

	private int addMultipleChoices(string id, string question, string[] options, string helperText, int selection) {
		GUILayout.Box(question + "\n" + helperText, GUILayout.Height(GUIElementHeight));

		selection = GUILayout.Toolbar(selection, options, GUILayout.Height(GUIElementHeight));
		if (selection > -1) {
			AddOrReplaceToDict(id, options[selection]);
		}

		GUILayout.Space(VerticalSpacing);

		return selection;
	}

	private string addTextQuestion(string id, string question, string helperText, string returnText) {
		GUILayout.Box(question + "\n" + helperText, GUILayout.Height(GUIElementHeight));

		returnText = GUILayout.TextArea(returnText, GUILayout.Height(TextAreaHeight));
		if (returnText.Length > 0) {
			AddOrReplaceToDict(id, returnText);
		}

		GUILayout.Space(VerticalSpacing);

		return returnText;
	}

	private void AddOrReplaceToDict(string key, string value) {
		AddOrReplaceToDict(answersDict, key, value);
	}

	private void AddOrReplaceToDict(Dictionary<string,string> dict, string key, string value) {
		if (dict.ContainsKey(key)) {
			dict[key] = value;
		}
		else {
			dict.Add(key, value);
		}
	}	

	private void AddOrReplaceToDict(Dictionary<string,int> dict, string key, int value) {
		if (dict.ContainsKey(key)) {
			dict[key] = value;
		}
		else {
			dict.Add(key, value);
		}
	}	

	private bool GetQuestionsAnswered() {
		bool bAnswered = true;
		foreach (KeyValuePair<string,string> answer in answersDict)	{
			if ((answer.Value == "" || answer.Key == "") && !answer.Key.Contains("comment")) {
				bAnswered = false;
				break;
			}
		}

		foreach (KeyValuePair<string,string> text in textDict) {
			if ((text.Value == "" || text.Key == "") && !text.Key.Contains("comment")) {
				bAnswered = false;
				break;
			}
		}

		foreach (KeyValuePair<string,int> selection in selectionDict) {
			if (selection.Value < 0 || selection.Key == "") {
				bAnswered = false;
				break;
			}
		}
		
		return bAnswered;
	}
}
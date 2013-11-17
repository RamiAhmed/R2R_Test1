using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionnaireHandler : MonoBehaviour {

	public bool bQuestionnaireEnabled = true;

	public enum QuestionnaireState {
		NONE = 0,
		DEMOGRAPHICS,
		STARTING,
		DURING,
		AFTER
	};

	public QuestionnaireState currentState = QuestionnaireState.NONE;

	private int currentDuring = 1,
				maxDuring = 3;

	private DatabaseHandler dbHandler = null;
	private GameController _gameController = null;

	private bool showingQuestionnaire = false; 

	private IDictionary questionsDict = null;
	private Dictionary<string,string> answersDict = null;

	private Dictionary<string,int> selectionDict = null;
	private Dictionary<string,string> textDict = null;

	private Rect questionnaireRect;

	private float verticalSpacing = 15f,
				textAreaHeight = 40f,
				elementHeight = 30f;
	
	void Start () {
		if (bQuestionnaireEnabled) {
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

			float width = 600f, height = 400f,
				screenWidth = Screen.width * 0.9f,
				screenHeight = Screen.height * 0.9f;

			width = screenWidth > width ? screenWidth : width;
			height = screenHeight > height ? screenHeight : height;

			questionnaireRect = new Rect((Screen.width/2f) - (width/2f), (Screen.height/2f) - (height/2f), width, height);

			currentState++;
		}
	}

	void Update () {
		if (bQuestionnaireEnabled) {
			if (_gameController.CurrentGameState == GameController.GameState.QUESTIONNAIRE) {
				if (!showingQuestionnaire) {
					showingQuestionnaire = true;

					if (questionsDict == null && dbHandler.questionsDict != null) {
						questionsDict = dbHandler.questionsDict;
					}
					else {
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
		if (bQuestionnaireEnabled) {
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

		switch (currentState) {
			case QuestionnaireState.DEMOGRAPHICS: buildDemographics(); break;
			case QuestionnaireState.STARTING: buildStarting(); break;
			case QuestionnaireState.DURING: buildDuring(); break;
			case QuestionnaireState.AFTER: buildAfter(); break;
		}

		if (GetQuestionsAnswered()) {
			GUILayout.FlexibleSpace();

			if (currentState != QuestionnaireState.AFTER) {
				if (GUILayout.Button("Continue", GUILayout.Height(elementHeight))) {
					dbHandler.ReadyData(answersDict);
					answersDict.Clear();

					if (currentState == QuestionnaireState.DURING) {
						currentDuring++;
						if (currentDuring > maxDuring) {
							currentState++;
						}
					}
					else {
						currentState++;
					}
					/*
					if (currentState-1 != QuestionnaireState.DEMOGRAPHICS) {
						_gameController.CurrentGameState = GameController.GameState.PLAY;
					}
					else {
						_gameController.CurrentGameState = GameController.GameState.MENU;
					}
					*/
				}
			}
			else {
				if (GUILayout.Button("Submit Answers", GUILayout.Height(elementHeight))) {
					dbHandler.ReadyData(answersDict);
					answersDict.Clear();

					dbHandler.SubmitAllData();
					bQuestionnaireEnabled = false;

					_gameController.CurrentGameState = GameController.GameState.PLAY;
				}
			}
		}

		GUILayout.EndVertical();
	}

	private void buildDemographics() {
		GUILayout.Box("DEMOGRAPHICS", GUILayout.Height(elementHeight));

		buildSection("Demographics");
	}

	private void buildStarting() {
		GUILayout.Box("BEFORE STARTING", GUILayout.Height(elementHeight));

		buildSection("Starting");
	}

	private void buildDuring() {
		GUILayout.Box("DURING " + currentDuring.ToString(), GUILayout.Height(elementHeight));

		buildSection("During");
	}

	private void buildAfter() {
		GUILayout.Box("AFTER", GUILayout.Height(elementHeight));

		buildSection("After");
	}

	private void buildSection(string section) {
		IList sectionList = (IList) questionsDict[section];
		foreach (IDictionary dict in sectionList) {
			string id = (string) dict["ID"];
			string question = (string) dict["Question"];
			List<string> options = convertFromIList((IList)dict["Options"]);
			string helperText = (string) dict["HelperText"];

			if (currentState == QuestionnaireState.DURING) {
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
		GUILayout.Box(question, GUILayout.Height(elementHeight));
		if (helperText != "") {
			GUILayout.Box(helperText, GUILayout.Height(elementHeight));
		}

		selection = GUILayout.Toolbar(selection, options, GUILayout.Height(elementHeight));
		if (selection > -1) {
			AddOrReplaceToDict(id, options[selection]);
		}

		GUILayout.Space(verticalSpacing);

		return selection;
	}

	private string addTextQuestion(string id, string question, string helperText, string returnText) {
		GUILayout.Box(question, GUILayout.Height(elementHeight));
		if (helperText != "") {
			GUILayout.Box(helperText, GUILayout.Height(elementHeight));
		}

		returnText = GUILayout.TextArea(returnText, GUILayout.Height(textAreaHeight));
		if (returnText.Length > 0) {
			AddOrReplaceToDict(id, returnText);
		}

		GUILayout.Space(verticalSpacing);

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

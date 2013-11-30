﻿using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
	
	public GUISkin MenuSkin;

	public Texture2D HUDInstructions, UnitInstructions, TAISInstructions;
	
	private GameController _gameController;
	
	private float screenWidth, screenHeight;

	private Rect instructionsRect;

	[HideInInspector]
	public int ShowingInstructions = 0;
	
	// Use this for initialization
	void Start () {
		_gameController = this.GetComponent<GameController>();
		screenWidth = Screen.width;
		screenHeight = Screen.height;

		float instructionsWidth = screenWidth * 0.9f,
		instructionsHeight = screenHeight * 0.9f;

		instructionsRect = new Rect((screenWidth/2f) - (instructionsWidth/2f), (screenHeight/2f) - (instructionsHeight/2f), instructionsWidth, instructionsHeight);
	}
	
	void OnGUI() {
		if (_gameController.CurrentGameState == GameController.GameState.MENU || _gameController.CurrentGameState == GameController.GameState.ENDING) {
			if (MenuSkin != null && GUI.skin != MenuSkin) {
				GUI.skin = MenuSkin;
			}
			
			float width = screenWidth * 0.75f,
				height = screenHeight * 0.75f;
			float x = (screenWidth - width)/2f,
				y = (screenHeight - height)/2f;
			
			float elementHeight = 40f;
			
			string playText = "Play Game",
				playTip = "Click to start playing the game";
			if (_gameController.GameTime > 1f) {
				playText = "Resume Game";
				playTip = "Click to resume playing the game";
			}
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			GUILayout.BeginVertical();
			
			GUILayout.Box(new GUIContent("Right to Rule - Prototype 1"), GUILayout.Height(elementHeight));
			
			if (!_gameController.GameEnded) {
				if (GUILayout.Button(new GUIContent(playText, playTip), GUILayout.Height(elementHeight))) {
					_gameController.CurrentGameState = GameController.GameState.PLAY;
					if (_gameController.CurrentPlayState == GameController.PlayState.NONE) {
						_gameController.CurrentPlayState = GameController.PlayState.BUILD;
					}
				}
			}
			else {
				if (!_gameController.GameWon) {
					GUILayout.Box("Your Gate of Life died - You have lost the game.");	
				}
				else {
					GUILayout.Box("You have won the game by reaching wave " + _gameController.MaximumWaveCount + ". Congratulations!");
				}
			}

			if (HUDInstructions != null) {
				if (GUILayout.Button(new GUIContent("View HUD Instructions", "Click this button to view the HUD intructions in a new window."), GUILayout.Height(elementHeight))) {
					ShowingInstructions = 1;
				}
			}

			if (UnitInstructions != null) {
				if (GUILayout.Button(new GUIContent("View Unit Instructions", "Click this button to view the unit instructions in a new window."), GUILayout.Height(elementHeight))) {
					ShowingInstructions = 2;
				}
			}

			if (TAISInstructions != null) {
				if (GUILayout.Button(new GUIContent("View Tactics Instructions (Only applicable for scenario with tactics)", "Click this button to view the tactics system (only exists in one of the testing scenarios) instructions in a new window."), GUILayout.Height(elementHeight))) {
					ShowingInstructions = 3;
				}
			}
			/*
			if (GUILayout.Button(new GUIContent("Restart Game", "Click to restart the current level"), GUILayout.Height(elementHeight))) {
				_gameController.RestartGame();	
			}
			*/

			if (GUILayout.Button(new GUIContent("Quit Game", "Click to exit and close the game"), GUILayout.Height(elementHeight))) {
				_gameController.QuitGame();
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button(new GUIContent("A Tower Defense game by Alpha Stage Studios - www.alphastagestudios.com", "Click to open up the\n website in your default browser."), GUILayout.Height(elementHeight))) {
				Application.OpenURL("www.alphastagestudios.com");	
			}
			
			GUILayout.EndVertical();
			
			GUILayout.EndArea();
			
			if (GUI.tooltip != "") {
				Vector2 mousePos = Input.mousePosition;
				float tipWidth = 200f, 
					  tipHeight = 100f;
				
				GUI.Box(new Rect(mousePos.x - tipWidth, screenHeight - mousePos.y - tipHeight, tipWidth, tipHeight), new GUIContent(GUI.tooltip));
			}

			if (ShowingInstructions > 0) {
				instructionsRect = GUI.Window(1, instructionsRect, drawInstructions, "");
				GUI.BringWindowToFront(1);
			}
		}
		else {
			ShowingInstructions = 0;
		}
	}

	private void drawInstructions(int windowID) {
		if (ShowingInstructions > 0) {
			float width = instructionsRect.width,
			height = instructionsRect.height;

			GUI.BeginGroup(new Rect(5f, 5f, instructionsRect.width-5f, instructionsRect.height-5f));

			Texture2D texture = null;
			if (ShowingInstructions == 1) {
				texture = HUDInstructions;
			}
			else if (ShowingInstructions == 2) {
				texture = UnitInstructions;
			}
			else if (ShowingInstructions == 3) {
				texture = TAISInstructions;
			}

			if (texture != null) {
				GUI.DrawTexture(new Rect((instructionsRect.width/2f) - (width/2f), 5f, width-10f, height*0.9f), texture, ScaleMode.StretchToFill);
			}
			else {
				ShowingInstructions = 0;
			}

			if (GUI.Button(new Rect(0f, height-(height*0.075f), instructionsRect.width, (height*0.075f)), "Back")) {
				ShowingInstructions = 0;
			}

			GUI.EndGroup();
		}
	}
}

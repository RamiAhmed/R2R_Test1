using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
	
	public GUISkin MenuSkin;
	
	private GameController _gameController;
	
	private float screenWidth, screenHeight;
	
	// Use this for initialization
	void Start () {
		_gameController = this.GetComponent<GameController>();
		screenWidth = Screen.width;
		screenHeight = Screen.height;
	}
	
	// Update is called once per frame
	void Update () {

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
					_gameController.CurrentPlayState = GameController.PlayState.BUILD;
				}
			}
			else {
				GUILayout.Box("Your Gate of Life died - You have lost the game.");	
			}
			
			if (GUILayout.Button(new GUIContent("Restart Game", "Click to restart the current level"), GUILayout.Height(elementHeight))) {
				_gameController.RestartGame();	
			}
			
			if (GUILayout.Button(new GUIContent("Quit Game", "Click to exit and close the game"), GUILayout.Height(elementHeight))) {
				_gameController.QuitGame();
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button(new GUIContent("A Tower Defense game by Alpha Stage Studios - www.alphastagestudios.com", "Click to open up the\n website in default browser."), GUILayout.Height(elementHeight))) {
				Application.OpenURL("www.alphastagestudios.com");	
			}
			
			GUILayout.EndVertical();
			
			GUILayout.EndArea();
			
			if (GUI.tooltip != "") {
				Vector2 mousePos = Input.mousePosition;
				float tipWidth = 200f, 
					  tipHeight = 50f;
				
				GUI.Box(new Rect(mousePos.x - tipWidth, screenHeight - mousePos.y - tipHeight, tipWidth, tipHeight), new GUIContent(GUI.tooltip));
			}
		}
	}
}

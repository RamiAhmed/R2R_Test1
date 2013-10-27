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
		
		if (MenuSkin != null) {
			GUI.skin = MenuSkin;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (_gameController.CurrentGameState == GameController.GameState.LOADING) {
			_gameController.CurrentGameState = GameController.GameState.MENU;	
		}
	}
	
	void OnGUI() {
		if (_gameController.CurrentGameState == GameController.GameState.MENU) {
			float width = screenWidth * 0.75f,
				height = screenHeight * 0.75f;
			float x = (screenWidth - width)/2f,
				y = (screenHeight - height)/2f;
			
			float elementHeight = 40f;
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			GUILayout.BeginVertical();
			
			GUILayout.Box(new GUIContent("Right to Rule - Prototype 1"), GUILayout.Height(elementHeight));
			
			if (GUILayout.Button(new GUIContent("Play Game"), GUILayout.Height(elementHeight))) {
				_gameController.CurrentGameState = GameController.GameState.PLAY;
			}
			
			if (GUILayout.Button(new GUIContent("Quit Game"), GUILayout.Height(elementHeight))) {
				_gameController.CurrentGameState = GameController.GameState.ENDING;
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button(new GUIContent("A game by Alpha Stage Studios - www.alphastagestudios.com"), GUILayout.Height(elementHeight/2f))) {
				Application.OpenURL("www.alphastagestudios.com");	
			}
			
			GUILayout.EndVertical();
			
			GUILayout.EndArea();
		}
	}
}

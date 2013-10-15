using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	[HideInInspector]
	public List<GameObject> unitsList;
	
	private Entity selectedUnit = null;
	
	private int amountUnits = 9;
	
	private float screenWidth = 0f,
				screenHeight = 0f;
	
	private Camera playerCam;
	
	private GameController _gameController;

	
	// Use this for initialization
	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<GameObject>();
		playerCam = this.GetComponentInChildren<Camera>();
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}		
		
		if (Input.GetMouseButtonDown(0)) {			
			selectUnit();
		}
		
		if (Input.GetMouseButtonDown(1)) {
			if (selectedUnit != null && selectedUnit.GetIsUnit(selectedUnit.gameObject)) {
				moveUnit();
			}
		}
	
		
	}
					
	private void moveUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {
			if (hit.collider.GetType() == typeof(TerrainCollider)) {
				Vector3 clickedPos = new Vector3(hit.point.x, 0f, hit.point.z);
				selectedUnit.GetComponent<UnitController>().moveToPosition = clickedPos;					
				break;
			}
		}
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		if (selectedUnit != null) {
			selectedUnit.Selected = false;
			selectedUnit = null;
		}
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {
			if (hit.transform.GetComponent<Entity>() != null) {
				selectedUnit = hit.transform.GetComponent<Entity>();
				selectedUnit.Selected = true;
				break;
			}			
		}	
		
	}
	
	void OnGUI() {
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		renderSpawnGUIButtons();
		renderSelectedUnitGUI();
		renderGameTime();
	}
	
	private void renderSelectedUnitGUI() {
		if (selectedUnit != null && selectedUnit.Selected) {
			float width = screenWidth * 0.2f,
				height = screenHeight * 0.6f;
			float x = screenWidth - width - 5f,
				y = (screenHeight - height)/4f;
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			GUILayout.BeginVertical();
			
			string unitString = "Selected unit: " + selectedUnit.Name;
			unitString += "\nHitpoints: " + Mathf.Round(selectedUnit.CurrentHitPoints) + " / " + Mathf.Round(selectedUnit.MaxHitPoints);
			unitString += "\nDamage: " + selectedUnit.Damage;
			unitString += "\nAccuracy: " + selectedUnit.Accuracy;
			unitString += "\nEvasion: " + selectedUnit.Evasion;
			unitString += "\nArmor: " + selectedUnit.Armor;
			unitString += "\nPerception Range: " + selectedUnit.PerceptionRange;
			unitString += "\nAttacking Range: " + selectedUnit.AttackingRange;
			GUILayout.Box(unitString, GUILayout.Height(height), GUILayout.Width(width));
			
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}		
	}
	
	private void renderSpawnGUIButtons() {
		float width = screenWidth * 0.9f,
			height = screenHeight * 0.2f;
		float x = (screenWidth - width)/2f,
			y = screenHeight - height;
		
		GUILayout.BeginArea(new Rect(x, y, width, height));
		GUILayout.BeginHorizontal();
		
		for (int i = 0; i <= amountUnits; i++) {
			createSpawnUnitButton(i);	
		}
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void renderGameTime() {
		float time = _gameController.GameTime;	
		
		GUILayout.BeginArea(new Rect(5f, 5f, 400f, 30f));
		GUILayout.BeginHorizontal();
		
		GUILayout.Box("Last wave: " + _gameController.WaveCount);
		GUILayout.Box("Elapsed time: " + Mathf.Round(time).ToString());
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			GUILayout.Box("Build time left: " + Mathf.Round(_gameController.BuildTime) + " / " + Mathf.Round(_gameController.MaxBuildTime));	
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			GUI.color = Color.red;
			GUILayout.Box("Combat! " + _gameController.enemies.Count + " / " + _gameController.WaveSize);	
			GUI.color = Color.white;
		}
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void createSpawnUnitButton(int index) {
		if (GUILayout.Button("Spawn Unit " + index.ToString(), GUILayout.Height(40f))) {
			spawnUnit(index);
		}
	}
	
	private void spawnUnit(int index) {
		//Debug.Log("Spawn: " + index);	
		GameObject newUnit = Instantiate(Resources.Load("Unit")) as GameObject;
		newUnit.GetComponent<UnitController>().playerOwner = this;
		unitsList.Add(newUnit);		
	}
}

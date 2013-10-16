using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public List<GameObject> unitsList;
	public List<GameObject> deadUnitsList;
	
	public int PlayerLives = 30;
	public int PlayerGold = 30;
	
	private Entity selectedUnit = null;
	
	private int amountUnits = 4;
	
	private float screenWidth = 0f,
				screenHeight = 0f;
	
	private Camera playerCam;
	
	private GameController _gameController;

	
	// Use this for initialization
	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<GameObject>();
		deadUnitsList = new List<GameObject>();
		playerCam = this.GetComponentInChildren<Camera>();
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}		
		
		if (PlayerLives <= 0) {
			_gameController.CurrentGameState = GameController.GameState.ENDING;
			return;
		}
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			if (_gameController.BuildTime <= 0f) {
				respawnUnits();
			}
			
			if (selectedUnit != null) {
				if (Input.GetMouseButtonDown(1)) {
					if (selectedUnit.GetIsUnit(selectedUnit.gameObject)) {
						moveUnit();
					}
				}
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT || _gameController.CurrentPlayState == GameController.PlayState.ARENA) {
			if (selectedUnit != null) {
				if (selectedUnit.IsDead) {
					clearSelection();
				}	
			}
		}
		if (Input.GetMouseButtonDown(0)) {			
			selectUnit();
		}
		
	}
	
	private void respawnUnits() {
		if (deadUnitsList.Count > 0) {
			Debug.Log("Respawn units");
			foreach (GameObject go in deadUnitsList) {
				UnitController unit = go.GetComponent<UnitController>();
				
				unitsList.Add(go);
				unit.SetIsNotDead(true);					
				unit.transform.position = unit.LastBuildLocation;
				unit.currentUnitState = UnitController.UnitState.PLACED;
				
				go.SetActive(true);
			}
			deadUnitsList.Clear();
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
	
	private void clearSelection() {
		if (selectedUnit != null) {
			selectedUnit.Selected = false;
			selectedUnit = null;
		}		
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		clearSelection();
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {
			if (hit.transform.GetComponent<Entity>() != null) {
				if (!hit.transform.GetComponent<Entity>().IsDead) {
					selectedUnit = hit.transform.GetComponent<Entity>();
					selectedUnit.Selected = true;
					break;
				}
			}			
		}	
		
	}
	
	void OnGUI() {
		if (_gameController.CurrentGameState == GameController.GameState.ENDING) {
			renderGameOver();
			//Debug.Log("Game Over");
		}
		else if (_gameController.CurrentGameState == GameController.GameState.PLAY) {			
			if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				renderSpawnGUIButtons();
			}
			
			renderSelectedUnitGUI();
			renderGameDetails();
		}
	}
	
	private void renderGameOver() {
		float width = screenWidth * 0.5f,
			height = screenHeight * 0.5f;
		float x = width/2f,
			y = height/2f;
		GUILayout.BeginArea(new Rect(x, y, width, height));
		
		GUI.color = Color.red;
		GUILayout.Box("You have lost all lives!\nGAME OVER", GUILayout.Width(width), GUILayout.Height(height));
		GUI.color = Color.white;
		
		GUILayout.EndArea();
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
			unitString += "\nAttacks/second: " + selectedUnit.AttacksPerSecond;
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
	
	private void renderGameDetails() {
		GUILayout.BeginArea(new Rect(5f, 5f, screenWidth*0.99f, 30f));
		GUILayout.BeginHorizontal();
		
		GUILayout.Box("Last wave: " + _gameController.WaveCount);
		GUILayout.Box("Unit count: " + unitsList.Count + " / " + _gameController.MaxUnitCount);
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			GUILayout.Box("Build time left: " + Mathf.Round(_gameController.BuildTime) + " / " + Mathf.Round(_gameController.MaxBuildTime));	
			
			if (GUILayout.Button("Spawn Wave")) {
				_gameController.ForceSpawn = true;	
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			GUI.color = Color.red;
			GUILayout.Box("Combat! Creeps: " + _gameController.enemies.Count + " / " + _gameController.WaveSize);	
			GUI.color = Color.white;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("Gold: " + PlayerGold);
		GUILayout.Box("Lives left: " + PlayerLives);
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void createSpawnUnitButton(int index) {
		if (GUILayout.Button("Spawn Unit " + index.ToString(), GUILayout.Height(40f))) {
			spawnUnit(index);
		}
	}
	
	private void spawnUnit(int index) {
		GameObject newUnit = Instantiate(Resources.Load("Unit")) as GameObject;
		int cost = newUnit.GetComponent<Unit>().GoldCost;
		if (_gameController.MaxUnitCount >= unitsList.Count) {
			Destroy(newUnit);
			Debug.LogWarning("You cannot build more units!");
		}
		else if (PlayerGold >= cost) {
			newUnit.GetComponent<UnitController>().playerOwner = this;
			unitsList.Add(newUnit);		
		}
		else {
			Destroy(newUnit);
			Debug.LogWarning("Not enough gold!");
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public List<GameObject> unitsList;
	public List<GameObject> deadUnitsList;
	
	public int PlayerLives = 1;
	public int PlayerGold = 20;
	
	public float ShowFeedbackTime = 3f; // show feedback messages for 5 seconds
	
	public Faction playerFaction;
	
	private List<Entity> selectedUnits = null;
	
	private float screenWidth = 0f,
				screenHeight = 0f;
	
	private Camera playerCam;
	
	private GameController _gameController;
	
	private string feedbackText = "";
	
	public Texture marqueeGraphics;
	private Vector2 marqueeOrigin, marqueeSize;
	private Rect marqueeRect;
	private Rect backupRect;
	
	void Awake() {
		playerFaction = this.gameObject.AddComponent<Faction>();
	}
	
	// Use this for initialization
	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<GameObject>();
		deadUnitsList = new List<GameObject>();
		playerCam = this.GetComponentInChildren<Camera>();
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		selectedUnits = new List<Entity>();
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
			
			if (selectedUnits.Count > 0) {
				if (Input.GetMouseButtonDown(1)) {
					moveUnit();
				}
			}
		}
		
		handleUnitSelection();
		
	}
	
	private void respawnUnits() {
		if (deadUnitsList.Count > 0) {
			Debug.Log("Respawn units");
			foreach (GameObject go in deadUnitsList) {
				UnitController unit = go.GetComponent<UnitController>();
				
				unitsList.Add(go);
				unit.SetIsNotDead(true);					
				
				go.SetActive(true);
			}
			deadUnitsList.Clear();
		}			
		
		foreach (GameObject go in unitsList) {
			UnitController unit = go.GetComponent<UnitController>();
			unit.StopMoving();
			unit.transform.position = unit.LastBuildLocation;	
			unit.currentUnitState = UnitController.UnitState.PLACED;
		}
	}
					
	private void moveUnit() {
		//Debug.Log("MoveUnit. Selected units: " + selectedUnits.Count + ", selectedUnit: " + selectedUnit);
		if (selectedUnits.Count > 0) {			
			Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
			RaycastHit[] hits = Physics.RaycastAll(mouseRay);
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					Vector3 clickedPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					
					foreach (Entity ent in selectedUnits) {
						if (!ent.IsDead && ent.GetComponent<UnitController>().playerOwner == this) {
							ent.MoveTo(clickedPos);
						}
					}
					break;
				}
			}
		}
	}
	
	private void clearSelection() {
		Debug.Log("Clear selection");
		
		if (selectedUnits.Count > 0) {
			while (selectedUnits.Count > 0) {
				selectedUnits[0].Deselect(selectedUnits);
			}
		}
	}
	
	private void handleUnitSelection() {
		
		if (Input.GetMouseButtonDown(0)) {
			clearSelection();
			
			float _invertedY = Screen.height - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
			
			selectUnit();
		}
		
		if (Input.GetMouseButton(0)) {
			float _invertedY = Screen.height - Input.mousePosition.y;
									
			marqueeSize = new Vector2(Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - _invertedY) * -1);
			//FIX FOR RECT.CONTAINS NOT ACCEPTING NEGATIVE VALUES
			if (marqueeRect.width < 0) {
			    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y, Mathf.Abs(marqueeRect.width), marqueeRect.height);
			}
			else if (marqueeRect.height < 0) {
			    backupRect = new Rect(marqueeRect.x, marqueeRect.y - Mathf.Abs(marqueeRect.height), marqueeRect.width, Mathf.Abs(marqueeRect.height));
			}
			if (marqueeRect.width < 0 && marqueeRect.height < 0) {
			    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y - Mathf.Abs(marqueeRect.height), Mathf.Abs(marqueeRect.width), Mathf.Abs(marqueeRect.height));
			}
			
			if ((marqueeRect.width > 0f || backupRect.width > 0f) && (marqueeRect.height > 0f || backupRect.height > 0f)) {				
				foreach (GameObject unit in unitsList) {
				    //Convert the world position of the unit to a screen position and then to a GUI point
				    Vector3 _screenPos = playerCam.WorldToScreenPoint(unit.transform.position);
				    Vector2 _screenPoint = new Vector2(_screenPos.x, Screen.height - _screenPos.y);
				    //Ensure that any units not within the marquee are currently unselected
					Entity ent = unit.GetComponent<Entity>();
				    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
				        // unit.SendMessage("OnUnselected", SendMessageOptions.DontRequireReceiver);
						ent.Deselect(selectedUnits);
				    }
				    
					if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
				        // unit.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
						ent.Select(selectedUnits);
				    }
				}
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			marqueeRect.width = 0;
			marqueeRect.height = 0;
			backupRect.width = 0;
			backupRect.height = 0;
			marqueeSize = Vector2.zero;
		}
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {			
			if (hit.transform.GetComponent<Entity>() != null) {
				Entity selectedUnit = hit.transform.GetComponent<Entity>();
				selectedUnit.Select(selectedUnits);
				break;			
			}			
		}	
		
	}
	
	/* GUI & UNIT SPAWNING */
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
			renderFeedbackMessage();
			
			renderMarqueeSelection();
		}
		else if (_gameController.CurrentGameState == GameController.GameState.PAUSED) {
			float width = screenWidth * 0.2f,
				  height = 30f;
			float x = (screenWidth/2f) - (width/2f),
				  y = (screenHeight/2f) - (height/2f);
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			GUILayout.Box("PAUSED");
			
			GUILayout.EndArea();				
		}
	}
	
	private void renderMarqueeSelection() {
		marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);	
		GUI.color = new Color(0, 0, 0, .3f);
		GUI.DrawTexture(marqueeRect, marqueeGraphics);
		GUI.color = Color.white;
	}
	
	private void renderGameOver() {
		float width = screenWidth * 0.5f,
			height = screenHeight * 0.5f;
		float x = width/2f,
			y = height/2f;
		GUILayout.BeginArea(new Rect(x, y, width, height));
		
		GUI.color = Color.red;
		GUILayout.Box("You have lost your Gate of Life!\nGAME OVER", GUILayout.Width(width));
		GUI.color = Color.white;
		
		GUILayout.EndArea();
	}
	
	private void renderSelectedUnitGUI() {
		if (selectedUnits.Count > 0) {			
			float width = screenWidth * 0.2f,
				height = screenHeight * 0.6f;
			float x = screenWidth - width - 5f,
				y = (screenHeight - height)/4f;
			
			Entity selectedUnit = selectedUnits[0];
			if (selectedUnit != null) {				
				string unitString = "Selected unit: " + selectedUnit.Name;
				unitString += "\nHitpoints: " + Mathf.Round(selectedUnit.CurrentHitPoints) + " / " + Mathf.Round(selectedUnit.MaxHitPoints);
				unitString += "\nDamage: " + selectedUnit.Damage;
				unitString += "\nAccuracy: " + selectedUnit.Accuracy;
				unitString += "\nEvasion: " + selectedUnit.Evasion;
				unitString += "\nArmor: " + selectedUnit.Armor;
				unitString += "\nMovement speed: " + selectedUnit.MovementSpeed;
				unitString += "\nPerception Range: " + selectedUnit.PerceptionRange;
				unitString += "\nAttacking Range: " + selectedUnit.AttackingRange;
				unitString += "\nAttacks/second: " + selectedUnit.AttacksPerSecond;
				unitString += "\nFleeing chance: " + Mathf.RoundToInt(selectedUnit.FleeThreshold*100f) + "%";
				unitString += "\n\nTotal unit score: " + selectedUnit.GetTotalScore();
				
				GUILayout.BeginArea(new Rect(x, y, width, height));
				GUILayout.BeginVertical();			
				
				if (selectedUnit.GetIsEnemy(selectedUnit.gameObject)) {
					unitString += "\nGold reward: " + selectedUnit.GetComponent<Enemy>().GoldReward;	
				}
				else if (selectedUnit.GetIsUnit(selectedUnit.gameObject)) {
					unitString += "\nGold cost: " + selectedUnit.GetComponent<Unit>().GoldCost;
				}
				
				if (selectedUnits.Count > 1) {
					unitString += "\n\nSelected units: " + selectedUnits.Count;
				}
				
				GUILayout.Box(unitString, GUILayout.Height(height), GUILayout.Width(width));
				
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
		}		
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
		//GUILayout.Box("Lives left: " + PlayerLives);
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void renderSpawnGUIButtons() {
		float width = screenWidth * 0.9f,
			height = screenHeight * 0.2f;
		float x = (screenWidth - width)/2f,
			y = screenHeight - (height/2f);
		
		GUILayout.BeginArea(new Rect(x, y, width, height));
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(width*0.1f);
		
		for (int i = 1; i <= playerFaction.NumberOfTiers; i++) {
			createSpawnUnitButton(i-1);	
		}
		
		GUILayout.Space(width*0.1f);
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void createSpawnUnitButton(int index) {
		if (GUILayout.Button((index+1).ToString() + ": " + playerFaction.FactionUnits[index].name, GUILayout.Height(40f)) || 
			(Input.GetKeyUp((KeyCode)(49 + index)))) {
			spawnUnit(index);
		}
	}
	
	private void spawnUnit(int index) {
		foreach (GameObject go in unitsList) {
			if (go.GetComponent<UnitController>().currentUnitState == UnitController.UnitState.PLACING) {
				Debug.LogWarning("Already placing unit");
				return;
			}
		}
		
		GameObject newUnit = Instantiate(playerFaction.FactionUnits[index].gameObject) as GameObject;
		newUnit.SetActive(true);
		
		int cost = newUnit.GetComponent<Unit>().GoldCost;
		if (_gameController.MaxUnitCount <= unitsList.Count) {
			Destroy(newUnit);
			//Debug.LogWarning("You cannot build more units!");
			DisplayFeedbackMessage("You cannot build more units, you have reached the maximum.");
		}
		else if (PlayerGold >= cost) {
			newUnit.GetComponent<UnitController>().playerOwner = this;
			unitsList.Add(newUnit);		
		}
		else {
			Destroy(newUnit);
			//Debug.LogWarning("Not enough gold!");
			DisplayFeedbackMessage("You do not have enough gold.");
		}
	}
	
	private void renderFeedbackMessage() {
		if (feedbackText != "") {
			float width = screenWidth * 0.4f,
				height = 30f;
			float x = (screenWidth - width)/2f,
				y = (screenHeight/2f) - (height/2f);
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			GUI.color = Color.red;
			GUILayout.Box(feedbackText, GUILayout.Width(width), GUILayout.Height(height));
			GUI.color = Color.white;
			
			GUILayout.EndArea();			
		}
	}
	
	public void DisplayFeedbackMessage(string text) {
		feedbackText = text;
		Invoke("stopDisplayFeedback", ShowFeedbackTime);
	}
	
	private void stopDisplayFeedback() {
		feedbackText = "";	
	}
}

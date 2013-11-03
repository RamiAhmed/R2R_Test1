using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public List<GameObject> unitsList, deadUnitsList;
	public List<Entity> SelectedUnits = null;
	
	public bool isDebugging = false;
	
	public int PlayerLives = 1;
	public int PlayerGold = 20;
	
	public float ShowFeedbackTime = 3f; // show feedback messages for 3 seconds
	
	public Faction playerFaction;
	
	public Texture marqueeGraphics;	
	
	public Texture2D swordHUD, bootHUD, shieldHUD, healthContainerHUD, healthBarHUD, TacticsCircleHUD;
	
	private float screenWidth = 0f,
				screenHeight = 0f;
	
	private Camera playerCam;
	
	private GameController _gameController;
	
	private string feedbackText = "";
	
	private Vector2 marqueeOrigin, marqueeSize;
	private Rect marqueeRect, backupRect;
	
	private Color feedbackColor = Color.white;
	
	// Tactical AI system
	private bool selectingTactic = false, 
				 selectingTarget = false, 
				 selectingCondition = false;
	 
	// Use this for initialization
	void Start () {
		//playerFaction = this.GetComponent<Faction>();
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<GameObject>();
		deadUnitsList = new List<GameObject>();
		playerCam = this.GetComponentInChildren<Camera>();
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		SelectedUnits = new List<Entity>();
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
			
			if (SelectedUnits.Count > 0) {
				if (Input.GetMouseButtonDown(1)) {
					moveUnit();
				}
			}
		}
		
		float yMarginFactor = 0.25f;
		if (Input.mousePosition.y > screenHeight * yMarginFactor && 
			(!selectingTactic && !selectingTarget && !selectingCondition)) {
			handleUnitSelection();
		}
		else {
			clearMarqueeRect();
		}
		
	}
	
	private void respawnUnits() {
		if (deadUnitsList.Count > 0) {
			Debug.Log("Respawn units");
			foreach (GameObject go in deadUnitsList) {
				UnitController unit = go.GetComponent<UnitController>();
				
				unitsList.Add(go);
				unit.SetIsNotDead();					
				
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
		if (SelectedUnits.Count > 0) {			
			Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
			RaycastHit[] hits = Physics.RaycastAll(mouseRay);
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					Vector3 clickedPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					
					foreach (Entity ent in SelectedUnits) {
						if (!ent.IsDead && ent.GetIsUnit() && ent.GetComponent<UnitController>().playerOwner == this) {
							ent.MoveTo(clickedPos);
						}
					}
					break;
				}
			}
		}
	}
	
	private void clearSelection() {		
		if (SelectedUnits.Count > 0) {
			while (SelectedUnits.Count > 0) {
				SelectedUnits[0].Deselect(SelectedUnits);
			}
		}
	}
	
	private void handleUnitSelection() {		
		if (Input.GetMouseButtonDown(0)) {
			clearSelection();
			
			float _invertedY = screenHeight - Input.mousePosition.y;
			marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
			
			selectUnit();
		}
		
		if (Input.GetMouseButton(0)) {
			float _invertedY = screenHeight - Input.mousePosition.y;
									
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
				    Vector2 _screenPoint = new Vector2(_screenPos.x, screenHeight - _screenPos.y);
				    //Ensure that any units not within the marquee are currently unselected
					Entity ent = unit.GetComponent<Entity>();
				    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
						ent.Deselect(SelectedUnits);
				    }
				    
					if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
						ent.Select(SelectedUnits);
				    }
				}
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			clearMarqueeRect();
		}
	}
	
	private void clearMarqueeRect() {
		marqueeRect.width = 0;
		marqueeRect.height = 0;
		backupRect.width = 0;
		backupRect.height = 0;
		marqueeSize = Vector2.zero;		
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {			
			if (hit.transform.GetComponent<Entity>() != null) {
				Entity selectedUnit = hit.transform.GetComponent<Entity>();
				selectedUnit.Select(SelectedUnits);
				break;			
			}			
		}	
		
	}
	
	/* GUI & UNIT SPAWNING */
	void OnGUI() {
		if (_gameController.CurrentGameState == GameController.GameState.ENDING) {
			renderGameOver();
		}
		else if (_gameController.CurrentGameState == GameController.GameState.PLAY) {			
			if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				createSpawnShortcuts();
			}
			
			renderTopHUD();
			renderBottomHUD();
			
			renderFeedbackMessage();			
			renderMarqueeSelection();
			
			if (isDebugging) {
				if (SelectedUnits.Count > 0) {
					if (SelectedUnits[0].GetIsUnit()) {
						UnitController selectedUnit = SelectedUnits[0].GetComponent<UnitController>();
						
						string debugLabel = "Selected: " + selectedUnit.Class + ": " + selectedUnit.Name;
						if (selectedUnit.lastAttacker != null) {
							debugLabel += "\nLast attacker: " + selectedUnit.lastAttacker.gameObject;
						}
						else {
							debugLabel += "\nLast attacker: None";
						}
						if (selectedUnit.attackTarget != null) {
							debugLabel += "\nAttack target: " + selectedUnit.attackTarget.gameObject;
						}
						else {
							debugLabel += "\nAttack target: None"; 
						}
						debugLabel += "\nAttack count: " + selectedUnit.attackCount;
						debugLabel += "\nKill count: " + selectedUnit.killCount;
						debugLabel += "\nAttacked count: " + selectedUnit.attackedCount;
						float x = 10f, y = 50f, width = 200f, height = 300f;
						GUI.Label(new Rect(x, y, width, height), debugLabel);
					}
				}
				
			}
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
		
		GUILayout.EndArea();
	}
	
	private void renderTopHUD() {
		float width = screenWidth * 0.99f,
			height = screenHeight * 0.10f;
		float x = 1f,
			y = 2f;
		
		GUILayout.BeginArea(new Rect(x, y, width, height));		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Main Menu")) {
			_gameController.CurrentGameState = GameController.GameState.MENU;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("Last wave: " + _gameController.WaveCount);
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			GUILayout.Box("Build time left: " + Mathf.Round(_gameController.BuildTime) + " / " + Mathf.Round(_gameController.MaxBuildTime));	
			
			if (GUILayout.Button(new GUIContent("Spawn Wave"))) {
				_gameController.ForceSpawn = true;	
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			GUI.color = Color.red;
			GUILayout.Box("Combat! Creeps: " + _gameController.enemies.Count + " / " + _gameController.WaveSize);	
			GUI.color = Color.white;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("Unit count: " + unitsList.Count + " / " + _gameController.MaxUnitCount);
		
		GUILayout.Box("Gold: " + PlayerGold);		
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void renderBottomHUD() {
		float width = (screenWidth * (1f - 0.13f)) - 0.01f ,
			height = screenHeight * 0.25f;
		float x = screenWidth - width,
			y = screenHeight - height;
		
		float elementWidth = width/3f,
			elementHeight = height;
				
		GUI.BeginGroup(new Rect(x, y, width, height)); // Start Bottom HUD
		
		float unitButtonsHeight = elementHeight * 0.2f;
		float healthBarHeight = elementHeight * 0.25f;
		
		GUI.BeginGroup(new Rect(0, 0, elementWidth, elementHeight)); // Unit details
		if (SelectedUnits.Count > 0) {
			Entity selectedUnit = SelectedUnits[0];
			if (selectedUnit.GetIsUnit()) {
				UnitController selectedUnitController = selectedUnit.GetComponent<UnitController>();	
				
				if (selectedUnitController != null) { // Sell & Upgrade buttons if unit
					string sellTip = "Sell Value: " + selectedUnitController.GetSellAmount();
					if (GUI.Button(new Rect(0f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent("Sell", sellTip))) {
						selectedUnitController.SellUnit();	
					}
					
					if (selectedUnitController.CanUpgrade()) {
						string upgradeTip = "Upgrade Cost: " + selectedUnitController.UpgradesInto.GoldCost; 
						upgradeTip += "\n Upgraded Unit Score: " + selectedUnitController.UpgradesInto.GetTotalScore();
						if (GUI.Button(new Rect(elementWidth/2f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent("Upgrade", upgradeTip))) {
							selectedUnitController.UpgradeUnit();	
						}
					}
				}
			}
			
			// Health bar
			float hpWidth = (elementWidth*0.75f) * (selectedUnit.CurrentHitPoints / selectedUnit.MaxHitPoints);			
			GUI.BeginGroup(new Rect(elementWidth/4f, unitButtonsHeight, hpWidth, healthBarHeight));
				string hpTip = "Current Hitpoints: " + selectedUnit.CurrentHitPoints.ToString("F2") + " / " + selectedUnit.MaxHitPoints;
				GUI.Label(new Rect(0f, 0f, elementWidth, healthBarHeight), new GUIContent(healthBarHUD, hpTip));			
			GUI.EndGroup();
			
			GUI.Label(new Rect(elementWidth/4f, unitButtonsHeight, elementWidth, healthBarHeight), new GUIContent(healthContainerHUD));
			
			
			// Class: Name
			float unitTitleHeight = elementHeight * 0.15f;			
			GUI.Box(new Rect(1f, unitButtonsHeight + healthBarHeight, elementWidth, unitTitleHeight), selectedUnit.Class + ": " + selectedUnit.Name);

			
			// Unit details
			float detailsHeight = elementHeight - healthBarHeight - unitButtonsHeight - unitTitleHeight;
			GUI.BeginGroup(new Rect(0f, healthBarHeight + unitButtonsHeight + unitTitleHeight, elementWidth, detailsHeight));
				// Profile picture
				if (selectedUnit.ProfilePicture != null) {
					GUI.Box(new Rect(0f, 0f, elementWidth/3f, detailsHeight), selectedUnit.ProfilePicture);
				}
				else {
					GUI.Box(new Rect(0f, 0f, elementWidth/3f, detailsHeight), "No picture");
				}			
			
				// Sword
				string swordTip = "Damage: " + selectedUnit.Damage + "\n";
				swordTip += "Accuracy: " + selectedUnit.Accuracy + "\n";
				swordTip += "Attacks per Second: " + selectedUnit.AttacksPerSecond;
				GUI.Box(new Rect(elementWidth/3f, 0f, elementWidth/3f, detailsHeight), new GUIContent(swordHUD, swordTip));
				
				// Shield
				string shieldTip = "Armor: " + selectedUnit.Armor + "\n";
				shieldTip += "Evasion: " + selectedUnit.Evasion;
				GUI.Box(new Rect(elementWidth/3f*2f, 0f, elementWidth/3f, detailsHeight/2f), new GUIContent(shieldHUD, shieldTip));
				
				// Boot
				string bootTip = "Movement Speed: " + selectedUnit.MovementSpeed;
				GUI.Box(new Rect(elementWidth/3f*2f, detailsHeight/2f, elementWidth/3f, detailsHeight/2f), new GUIContent(bootHUD, bootTip));
			GUI.EndGroup();
		}
		else {
			GUI.Box(new Rect(0f, unitButtonsHeight + healthBarHeight, elementWidth, elementHeight), "No unit selected");	
		}
				
		GUI.EndGroup(); // End unit details
		
		elementHeight -= unitButtonsHeight + healthBarHeight;		
		
		GUI.BeginGroup(new Rect(elementWidth+2.5f, unitButtonsHeight + healthBarHeight, elementWidth-5f, elementHeight)); // Tactical AI System
		if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {			
			UnitController selectedUnitController = SelectedUnits[0].GetComponent<UnitController>();
			
			float columnWidth = (elementWidth-5f)/3f,
				rowHeight = elementHeight * 0.2f;
			
			GUI.BeginGroup(new Rect(0f, 0f, columnWidth, elementHeight)); // Tactics
			
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Tactics");
			
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						string tacticsString = selectedUnitController.GetTacticsName(selectedUnitController.currentTactic);
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), tacticsString)) {
							selectingTactic = true;
					
							if (selectingTarget) 
								selectingTarget = false;
					
							if (selectingCondition)
								selectingCondition = false;
						}
					}
			
				GUI.EndGroup();
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(columnWidth, 0f, columnWidth, elementHeight)); // Target
				
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Target");
				
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), selectedUnitController.GetTargetName(selectedUnitController.currentTarget))) {
							selectingTarget = true;
					
							if (selectingTactic) 
								selectingTactic = false;
					
							if (selectingCondition)
								selectingCondition = false;
						}
					}
			
				GUI.EndGroup();
			
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(columnWidth*2f, 0f, columnWidth, elementHeight)); // Condition
			
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Condition");
			
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), selectedUnitController.GetConditionName(selectedUnitController.currentCondition))) {
							selectingCondition = true;
					
							if (selectingTactic) 
								selectingTactic = false;
					
							if (selectingTarget) 
								selectingTarget = false;
						}
					}
				GUI.EndGroup();
			
			GUI.EndGroup();
			
		}
		else {
			GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "No unit selected");	
		}
		
		
		GUI.EndGroup(); // End Tactics
		
		GUI.BeginGroup(new Rect(elementWidth*2f, unitButtonsHeight + healthBarHeight, elementWidth, elementHeight)); // Spawn Grid		
		if (playerFaction.FactionUnits.Count > 0) {
			GUI.BeginGroup(new Rect(0f, 0f, elementWidth, elementHeight));
				createSpawnButton(0, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(0f, elementHeight/2f, elementWidth, elementHeight));
				createSpawnButton(2, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(elementWidth/2f, 0f, elementWidth, elementHeight));
				createSpawnButton(1, elementWidth, elementHeight);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(elementWidth/2f, elementHeight/2f, elementWidth, elementHeight));
				createSpawnButton(3, elementWidth, elementHeight);
			GUI.EndGroup();
		}
		else {
			GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "ERROR: No spawnable units");	
		}
		
		GUI.EndGroup(); // end spawn grid
		
		
		GUI.EndGroup(); // End Bottom HUD
		
		if (GUI.tooltip != "") {
			Vector2 mousePos = Input.mousePosition;
			float tipWidth = 200f, tipHeight = 60f;
			GUI.Box(new Rect(mousePos.x - tipWidth, screenHeight - mousePos.y - tipHeight, tipWidth, tipHeight), GUI.tooltip);
		}
		
		if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {
			renderTacticsInterface();
		}
		
	}
	
	private void renderTacticsInterface() {
		Vector2 center = new Vector2(screenWidth/2f, screenHeight/2f);
		float radius = 180f;
		Rect rect = new Rect(0f, 0f, 130f, 25f);
		UnitController selectedUnit = SelectedUnits[0].GetComponent<UnitController>();
		
		if (selectingTactic || selectingTarget || selectingCondition) {
			GUI.depth = 1;
			float bgRectSizeFactor = 3.1f;
			GUI.Label(new Rect(center.x-(radius*(bgRectSizeFactor/2f)), center.y-(radius*(bgRectSizeFactor/2f)), radius*bgRectSizeFactor, radius*bgRectSizeFactor), TacticsCircleHUD);
			GUI.depth = 0;
			
			if (GUI.Button(new Rect(center.x - (rect.width/2f), center.y - (rect.height/2f), rect.width, rect.height), "Cancel") || Input.GetKeyDown(KeyCode.Escape)) {
				if (selectingTactic)
					selectingTactic = false;	
				
				if (selectingTarget)
					selectingTarget = false;
				
				if (selectingCondition)
					selectingCondition = false;	
			}	
		}
		
		if (selectingTactic) {
			System.Array arr = System.Enum.GetValues(typeof(UnitController.Tactics));	

			int count = arr.Length;
			float angleStep = Mathf.PI * 2.0f / count;		
			
			for (int i = 0; i < count; i++) {
				UnitController.Tactics tactic = (UnitController.Tactics) i;
				string tacticName = selectedUnit.GetTacticsName(tactic);
				
				rect.x = center.x + Mathf.Cos(angleStep * i) * radius - rect.width/2f;
				rect.y = center.y + Mathf.Sin(angleStep * i) * radius - rect.height/2f;
				
				if (GUI.Button(rect, tacticName)) {
					selectedUnit.currentTactic = tactic;
					selectingTactic = false;
				}
			}
		}
		else if (selectingTarget) {
			System.Array arr = System.Enum.GetValues(typeof(UnitController.Target));
			
			int count = arr.Length;
			float angleStep = Mathf.PI * 2.0f / count; 
			
			for (int i = 0; i < count; i++) {
				UnitController.Target target = (UnitController.Target) i;
				string targetName = selectedUnit.GetTargetName(target);
				
				rect.x = center.x + Mathf.Cos(angleStep * i) * radius - rect.width/2f;
				rect.y = center.y + Mathf.Sin(angleStep * i) * radius - rect.height/2f;
				
				if (GUI.Button(rect, targetName)) {
					selectedUnit.currentTarget = target;
					selectingTarget = false;
				}
			}
		}
		else if (selectingCondition) {
			System.Array arr = System.Enum.GetValues(typeof(UnitController.Condition));
			
			int count = arr.Length;
			float angleStep = Mathf.PI * 2.0f / count; 
			for (int i = 0; i < count; i++) {
				UnitController.Condition condition = (UnitController.Condition) i;
				string conditionName = selectedUnit.GetConditionName(condition);
				
				rect.x = center.x + Mathf.Cos(angleStep * i) * radius - (rect.width/2f);
				rect.y = center.y + Mathf.Sin(angleStep * i) * radius - (rect.height/2f);
				
				if (GUI.Button(rect, conditionName)) {
					selectedUnit.currentCondition = condition;
					selectingTarget = false;
				}
			}				
		}		
	}
	
	private void createSpawnButton(int index, float elementWidth, float elementHeight) {
		UnitController unit = playerFaction.FactionUnits[index].GetComponent<UnitController>();
		string tip = "Gold Cost: " + unit.GoldCost + "\n"; 
		tip += "Unit Score: " + unit.GetTotalScore() + "\n";
		tip += "Unit Class: " + unit.Class;
		GUIContent btn = new GUIContent((index+1) + " : " + unit.Name, tip);
		if (GUI.Button(new Rect(0f, 0f, elementWidth/2f, elementHeight/2f), btn)) {
			spawnUnit(index);
		}
	}
	
	private void createSpawnShortcuts() {
		for (int i = 0; i < playerFaction.FactionUnits.Count; i++) {
			if (Input.GetKeyUp((KeyCode)(49 + i))) {
				spawnUnit(i);	
			}
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
			DisplayFeedbackMessage("You cannot build more units, you have reached the maximum.");
		}
		else if (PlayerGold >= cost) {
			newUnit.GetComponent<UnitController>().playerOwner = this;
			unitsList.Add(newUnit);		
		}
		else {
			Destroy(newUnit);
			DisplayFeedbackMessage("You do not have enough gold.");
		}
	}
	
	private void renderFeedbackMessage() {
		if (feedbackText != "") {
			float width = screenWidth * 0.5f,
				height = 30f;
			float x = (screenWidth - width)/2f,
				y = (screenHeight/2f) - (height/2f);
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			
			GUI.color = feedbackColor;
			GUILayout.Box(feedbackText, GUILayout.Width(width), GUILayout.Height(height));
			
			GUILayout.EndArea();			
		}
	}
	
	public void DisplayFeedbackMessage(string text) {
		DisplayFeedbackMessage(text, Color.red);
	}
	
	public void DisplayFeedbackMessage(string text, Color color) {
		feedbackText = text;
		Invoke("stopDisplayFeedback", ShowFeedbackTime);
		feedbackColor = color;
	}
	
	private void stopDisplayFeedback() {
		feedbackText = "";	
	}
}

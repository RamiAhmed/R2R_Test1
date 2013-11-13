using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public List<Entity> unitsList = null, deadUnitsList = null, SelectedUnits = null;
	
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
	public bool bSelectingTactics = false;
	 
	// Use this for initialization
	void Start () {
		//playerFaction = this.GetComponent<Faction>();
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		unitsList = new List<Entity>();
		deadUnitsList = new List<Entity>();
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
		}
		
		if (SelectedUnits.Count > 0) {
			if (Input.GetMouseButtonDown(1)) {
				if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
					moveUnit();
				}
				else if (SelectedUnits[0].GetIsUnit()) {
					DisplayFeedbackMessage("You cannot move units, unless in the Build Phase.");	
				}
			}
		}
		
		float yMarginFactor = 0.25f,
			  xMarginFactor = 0.3f;
		Rect disallowedRect = new Rect((xMarginFactor/2f) * screenWidth, screenHeight - (screenHeight * yMarginFactor), xMarginFactor * screenWidth, screenHeight * yMarginFactor);
		Rect disallowedRect2 = new Rect((xMarginFactor*1.5f) * screenWidth, screenHeight - (screenHeight * yMarginFactor), xMarginFactor * screenWidth, screenHeight * yMarginFactor);
		
		Vector2 mousePos = new Vector2(Input.mousePosition.x, screenHeight - Input.mousePosition.y);
		bool disallowedClick = disallowedRect.Contains(mousePos) || disallowedRect2.Contains(mousePos);
		if (!disallowedClick && !bSelectingTactics) {
			handleUnitSelection();
		}
		else {
			clearMarqueeRect();
		}
	}
	
	private void respawnUnits() {
		if (deadUnitsList.Count > 0) {
			Debug.Log(_gameController.GameTime + ": " + "Respawn units");
			foreach (Entity go in deadUnitsList) {				
				UnitController unit = go.GetComponent<UnitController>();
				
				unitsList.Add(go);
				unit.SetIsNotDead();
				
				go.gameObject.SetActive(true);
			}
			deadUnitsList.Clear();
		}			
		
		if (unitsList.Count > 0) {
			foreach (Entity go in unitsList) {
				UnitController unit = go.GetComponent<UnitController>();
				unit.StopMoving();
				unit.StopAllAnimations();
				unit.transform.position = unit.LastBuildLocation;	
				unit.currentUnitState = UnitController.UnitState.PLACED;
				unit.SetIsNotDead();
			}
		}
	}
					
	private void moveUnit() {
		if (SelectedUnits.Count > 0) {			
			Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
			RaycastHit[] hits = Physics.RaycastAll(mouseRay);
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					Vector3 clickedPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);

					if (SelectedUnits[0].GetIsPosWalkable(clickedPos)) {
						foreach (Entity ent in SelectedUnits) {
							if (!ent.IsDead && ent.GetIsUnit() && ent.GetComponent<UnitController>().playerOwner == this) {
								ent.MoveTo(clickedPos);
							}
						}
						break;
					}
					else {
						DisplayFeedbackMessage("You cannot move units to that location.");
					}
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
				bool unitFound = false;
				foreach (Entity unit in unitsList) {
				    //Convert the world position of the unit to a screen position and then to a GUI point
				    Vector3 _screenPos = playerCam.WorldToScreenPoint(unit.transform.position);
				    Vector2 _screenPoint = new Vector2(_screenPos.x, screenHeight - _screenPos.y);
				    //Ensure that any units not within the marquee are currently unselected
				    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
						unit.Deselect(SelectedUnits);
				    }
				    
					if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
						unit.Select(SelectedUnits);
						if (!unitFound) {
							unitFound = true;
						}
				    }
				}
				
				if (!unitFound) {
					foreach (Entity enemy in _gameController.enemies) {
					    //Convert the world position of the unit to a screen position and then to a GUI point
					    Vector3 _screenPos = playerCam.WorldToScreenPoint(enemy.transform.position);
					    Vector2 _screenPoint = new Vector2(_screenPos.x, screenHeight - _screenPos.y);
					    //Ensure that any units not within the marquee are currently unselected
					    if (!marqueeRect.Contains(_screenPoint) || !backupRect.Contains(_screenPoint)) {
							enemy.Deselect(SelectedUnits);
					    }
					    
						if (marqueeRect.Contains(_screenPoint) || backupRect.Contains(_screenPoint)) {
							enemy.Select(SelectedUnits);
					    }							
					}
				}
				else {
					foreach (Entity entity in SelectedUnits) {
						if (entity.GetIsEnemy() || (entity.GetIsUnit() && entity.GetComponent<UnitController>().playerOwner != this)) {
							entity.Deselect(SelectedUnits);
						}
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
		 if (_gameController.CurrentGameState == GameController.GameState.PLAY) {			
			if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				createSpawnShortcuts();
			}
			
			renderTopHUD();
			renderBottomHUD();
			
			renderSelectedUnitHealthbar();
			
			renderFeedbackMessage();			
			renderMarqueeSelection();
			
			if (isDebugging) {
				renderSelectedDebugFeedback();	
			}
		}
		else if (_gameController.CurrentGameState == GameController.GameState.PAUSED) {
			renderPauseGUI();
		}
	}

	private void renderPauseGUI() {
		float width = screenWidth * 0.4f,
		height = 50f;
		float x = (screenWidth/2f) - (width/2f),
		y = (screenHeight/2f) - (height/2f);
		
		GUILayout.BeginArea(new Rect(x, y, width, height));
		
		GUILayout.Box("PAUSED\n Press 'P' or 'Pause|Break' to resume game.");
		
		GUILayout.EndArea();
	}

	private void renderSelectedUnitHealthbar() {
		if (SelectedUnits.Count > 0) {
			float width = 100f, height = 30f;
			foreach (Entity selected in SelectedUnits) {
				Vector3 healthBarPos = playerCam.WorldToScreenPoint(selected.transform.position);
				float barWidth = width * (selected.CurrentHitPoints / selected.MaxHitPoints);
				
				GUI.BeginGroup(new Rect(healthBarPos.x - (width/2f), screenHeight - healthBarPos.y - (width/2f), barWidth, height));
					GUI.Label(new Rect(0f, 0f, width, height), healthBarHUD);
				GUI.EndGroup();
			}
		}
	}
	
	private void renderSelectedDebugFeedback() {
		if (SelectedUnits.Count > 0) {
			if (SelectedUnits[0].GetIsUnit() || SelectedUnits[0].GetIsEnemy()) {
				Entity selectedUnit = SelectedUnits[0];
				
				string debugLabel = "DEBUG FEEDBACK\n";
				
				debugLabel += "\nSelected: " + selectedUnit.Class + ": " + selectedUnit.Name;
	
				if (selectedUnit.lastAttacker != null) {
					debugLabel += "\nLast attacker: " + selectedUnit.lastAttacker.Name;
				}
				else {
					debugLabel += "\nLast attacker: None";
				}
				if (selectedUnit.attackTarget != null) {
					debugLabel += "\nAttack target: " + selectedUnit.attackTarget.Name;
				}
				else {
					debugLabel += "\nAttack target: None"; 
				}
				debugLabel += "\nAttack count: " + selectedUnit.attackCount;
				debugLabel += "\nKill count: " + selectedUnit.killCount;
				debugLabel += "\nAttacked count: " + selectedUnit.attackedCount;
				
				float unitScoreSum = 0f;
				foreach (Entity unit in unitsList) {
					unitScoreSum += unit.GetTotalScore();
				}
				debugLabel += "\n\nTotal Unit Score: " + unitScoreSum;
				
				debugLabel += "\nCurrently Selected Units Count: " + SelectedUnits.Count;
				
				unitScoreSum = 0f;
				foreach(Entity unit in SelectedUnits) {
					unitScoreSum += unit.GetTotalScore();
				}
				debugLabel += "\nCurrently Selected Units Total Score: " + unitScoreSum;
				
				float x = 10f, y = 50f, width = 300f, height = 300f;
				GUI.Box(new Rect(x, y, width, height), "");
				GUI.Label(new Rect(x+5f, y+5f, width-10f, height-6f), debugLabel);
			}
		}			
	}
	
	private void renderMarqueeSelection() {
		marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);	
		GUI.color = new Color(0, 0, 0, .3f);
		GUI.DrawTexture(marqueeRect, marqueeGraphics);
		GUI.color = Color.white;
	}
	
	private void renderTopHUD() {
		float width = screenWidth * 0.99f,
			height = screenHeight * 0.05f;
		float x = 1f,
			y = 5f;
		
		GUILayout.BeginArea(new Rect(x, y, width, height));		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Main Menu (ESC)", GUILayout.Height(height))) {
			_gameController.CurrentGameState = GameController.GameState.MENU;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("Last wave: " + _gameController.WaveCount, GUILayout.Height(height));
		
		if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			float maxBuildTime = _gameController.GetMaxBuildTime();
			GUILayout.Box("Build time left: " + Mathf.Round(_gameController.BuildTime) + " / " + Mathf.Round(maxBuildTime), GUILayout.Height(height));	
			
			if (GUILayout.Button(new GUIContent("Spawn Now (Space)"), GUILayout.Height(height)) || Input.GetKeyDown(KeyCode.Space)) {
				_gameController.ForceSpawn = true;	
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			GUI.color = Color.red;
			GUILayout.Box("Combat! Creeps: " + _gameController.enemies.Count + " / " + _gameController.WaveSize, GUILayout.Height(height));	
			GUI.color = Color.white;
		}
		
		GUILayout.FlexibleSpace();
		
		GUILayout.Box("Unit count: " + unitsList.Count + " / " + _gameController.MaxUnitCount, GUILayout.Height(height));
		
		GUILayout.Box("Gold: " + PlayerGold + "g", GUILayout.Height(height));		
		
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
			if (selectedUnit.GetIsUnit() && _gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				UnitController selectedUnitController = selectedUnit.GetComponent<UnitController>();	
				
				if (selectedUnitController != null) { // Sell & Upgrade buttons if unit
					string sellTip = "Sell Value: " + selectedUnitController.GetSellAmount() + "g",
							sellLabel = "Sell (Cost: " + selectedUnitController.GoldCost + "g)";
					sellTip += "\nSelling will remove the unit permanently.";
					if (GUI.Button(new Rect(0f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent(sellLabel, sellTip))) {
						selectedUnitController.SellUnit();	
					}
					
					if (selectedUnitController.CanUpgrade()) {
						string upgradeTip = "Upgrade Cost: " + selectedUnitController.UpgradesInto.GoldCost + "g",
								upgradeLabel = "Upgrade (Cost: " + selectedUnitController.UpgradesInto.GoldCost + "g)"; 
						upgradeTip += "\n Upgraded Unit Score: " + selectedUnitController.UpgradesInto.GetTotalScore();
						if (GUI.Button(new Rect(elementWidth/2f, 0f, elementWidth/2f, unitButtonsHeight), new GUIContent(upgradeLabel, upgradeTip))) {
							selectedUnitController.UpgradeUnit();	
						}
					}
				}
			}
			
			// Health bar
			float hpWidth = elementWidth * (selectedUnit.CurrentHitPoints / selectedUnit.MaxHitPoints);		
			string hpLabel = selectedUnit.CurrentHitPoints.ToString("F0") + " / " + selectedUnit.MaxHitPoints;
			GUI.BeginGroup(new Rect(0f, unitButtonsHeight, hpWidth, healthBarHeight));				
				GUI.DrawTexture(new Rect(0f, 0f, elementWidth, healthBarHeight), healthBarHUD, ScaleMode.StretchToFill);
			GUI.EndGroup();

			GUI.DrawTexture(new Rect(0f, unitButtonsHeight, elementWidth, healthBarHeight), healthContainerHUD, ScaleMode.StretchToFill);
			GUI.Label(new Rect(elementWidth/2f, unitButtonsHeight+healthBarHeight/3f, elementWidth, healthBarHeight), new GUIContent(hpLabel));
			
			
			// Class: Name
			float unitTitleHeight = elementHeight * 0.15f;			
			GUI.Box(new Rect(1f, unitButtonsHeight + healthBarHeight, elementWidth, unitTitleHeight), selectedUnit.Class + ": " + selectedUnit.Name);

			
			// Unit details
			float detailsHeight = elementHeight - healthBarHeight - unitButtonsHeight - unitTitleHeight;
			float detailsWidth = elementWidth/3f;
			GUI.BeginGroup(new Rect(0f, healthBarHeight + unitButtonsHeight + unitTitleHeight, elementWidth, detailsHeight));
				// Profile picture
				if (selectedUnit.ProfilePicture != null) {
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), "");
					GUI.DrawTexture(new Rect(0f, 0f, detailsWidth, detailsHeight), selectedUnit.ProfilePicture, ScaleMode.ScaleToFit);
				}
				else {
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), "No picture");
				}			
			
				// Sword
				string swordTip = "Damage: " + selectedUnit.Damage + "\n";
				swordTip += "Accuracy: " + selectedUnit.Accuracy + "\n";
				swordTip += "Attacks per Second: " + selectedUnit.AttacksPerSecond;
				GUI.BeginGroup(new Rect(detailsWidth, 0f, elementWidth, detailsHeight));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight), new GUIContent(swordHUD, swordTip));
					
					string dpsLabel = "DPS: " + selectedUnit.GetDamagePerSecond().ToString("F0");
					GUI.Label(new Rect(5f, detailsHeight/3f, detailsWidth, detailsHeight), new GUIContent(dpsLabel));
				GUI.EndGroup();		
			
				// Shield
				string shieldTip = "Armor: " + selectedUnit.Armor + "\n";
				shieldTip += "Evasion: " + selectedUnit.Evasion;
				GUI.BeginGroup(new Rect(detailsWidth*2f, 0f, elementWidth, detailsHeight/2f));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight/2f), new GUIContent(shieldHUD, shieldTip));
			
					string defenseLabel = "Armor: " + selectedUnit.Armor;
					GUI.Label(new Rect(5f, 5f, detailsWidth, detailsHeight), new GUIContent(defenseLabel));				
				GUI.EndGroup();
				
				// Boot
				string bootTip = "Movement Speed: " + selectedUnit.MovementSpeed + "\n";
				bootTip += "Flee Chance: " + (selectedUnit.FleeThreshold*100f) + "%";
				GUI.BeginGroup(new Rect(detailsWidth*2f, detailsHeight/2f, detailsWidth, detailsHeight/2f));
					GUI.Box(new Rect(0f, 0f, detailsWidth, detailsHeight/2f), new GUIContent(bootHUD, bootTip));
			
					string moveLabel = "Speed: " + selectedUnit.MovementSpeed;
					GUI.Label(new Rect(5f, 5f, detailsWidth, detailsHeight), new GUIContent(moveLabel));
				GUI.EndGroup();
				
			GUI.EndGroup();
		}
		else {
			GUI.Box(new Rect(0f, unitButtonsHeight + healthBarHeight, elementWidth, elementHeight), "No unit selected");	
		}
				
		GUI.EndGroup(); // End unit details
		
		elementHeight -= unitButtonsHeight + healthBarHeight;		
		
		// Tactical AI System
		GUI.BeginGroup(new Rect(elementWidth+2.5f, unitButtonsHeight + healthBarHeight, elementWidth-5f, elementHeight)); 
		if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {			
			UnitController selectedUnitController = SelectedUnits[0].GetComponent<UnitController>();
			
			float columnWidth = (elementWidth-5f)/3f,
				rowHeight = elementHeight * 0.25f;
			
			GUI.BeginGroup(new Rect(0f, 0f, columnWidth, elementHeight)); // Tactics
			
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Tactics");
			
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						string tacticsString = selectedUnitController.GetTacticsName(selectedUnitController.currentTactic);
						string tacticsTip = "Set tactical orders for this unit. Changing the tactics will affect the units behaviour.";
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), new GUIContent(tacticsString, tacticsTip))) {
							if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
								if (!bSelectingTactics) 
									bSelectingTactics = true;
							}
							else {
								DisplayFeedbackMessage("You can only set Tactics in the Build phase.");
							}
						}
					}
			
				GUI.EndGroup();
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(columnWidth, 0f, columnWidth, elementHeight)); // Target
				
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Target");
				
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						string targetString = selectedUnitController.GetTargetName(selectedUnitController.currentTarget);
						string targetTip = "Set the tactical target for this unit. Unit's tactics will be applied to the chosen target.";
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), new GUIContent(targetString, targetTip))) {
							if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
								if (!bSelectingTactics) 
									bSelectingTactics = true;
							}
							else {
								DisplayFeedbackMessage("You can only set Targets in the Build phase.");
							}
						}
					}
			
				GUI.EndGroup();
			
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(columnWidth*2f, 0f, columnWidth, elementHeight)); // Condition
			
				GUI.Box(new Rect(0f, 0f, columnWidth, rowHeight), "Condition");
			
				GUI.BeginGroup(new Rect(0f, rowHeight+5f, columnWidth, elementHeight-rowHeight+5f));
			
					if (selectedUnitController != null) {
						string conditionString = selectedUnitController.GetConditionName(selectedUnitController.currentCondition);
						string conditionTip = "Set the tactical condition for this unit. The condition will affect when the unit's tactic is executed.";
						if (GUI.Button(new Rect(0f, 0f, columnWidth, rowHeight), new GUIContent(conditionString, conditionTip))) {
							if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
								if (!bSelectingTactics) 
									bSelectingTactics = true;
							}
							else {
								DisplayFeedbackMessage("You can only set Conditions in the Build phase.");
							}
						}
					}
				GUI.EndGroup();
			
			GUI.EndGroup();
			
		}
		else if (SelectedUnits.Count > 0 && !SelectedUnits[0].GetIsUnit()) {
			GUI.Box(new Rect(0f, 0f, elementWidth, elementHeight), "You can only set Tactics on your own units.");
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
		
		if (SelectedUnits.Count > 0 && SelectedUnits[0].GetIsUnit()) {
			renderTacticsInterface();
		}
		
		if (GUI.tooltip != "") {
			GUI.skin.box.wordWrap = true;
			Vector2 mousePos = Input.mousePosition;
			float tipWidth = 250f, tipHeight = 60f;
			GUI.Box(new Rect(mousePos.x - tipWidth, screenHeight - mousePos.y - tipHeight, tipWidth, tipHeight), GUI.tooltip);
		}		
	}
	
	private void renderTacticsInterface() {
		UnitController selectedUnit = SelectedUnits[0].GetComponent<UnitController>();

		if (selectedUnit == null) {
			Debug.LogWarning("Could not find selected unit in renderTacticsInterface");
		}		
		else if (bSelectingTactics) {
			float width = screenWidth * 0.6f,
			height = screenHeight * 0.3f;
			float x = (screenWidth - width)/2f,
			y = height/2f;

			float elementWidth = width/3f, elementHeight = height/8f;

			System.Array arr;
			int count;

			GUI.Box(new Rect(x, y, width, height), "");

			GUILayout.BeginArea(new Rect(x, y, width, height));	
			GUILayout.BeginHorizontal();

			// Tactics
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));
				
				GUILayout.Box("Current Tactic: " + selectedUnit.GetTacticsName(selectedUnit.currentTactic), GUILayout.Height(elementHeight));

				arr = System.Enum.GetValues(typeof(UnitController.Tactics));				
				count = arr.Length;				

				for (int i = 0; i < count; i++) {
					UnitController.Tactics tactic = (UnitController.Tactics) i;
					string tacticName = selectedUnit.GetTacticsName(tactic);

					if (GUILayout.Button(new GUIContent(tacticName, selectedUnit.GetTacticsTip(tactic)))) {
						selectedUnit.currentTactic = tactic;
					}
				}

			GUILayout.EndVertical();

			// Targets
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));

				GUILayout.Box("Current Target: " + selectedUnit.GetTargetName(selectedUnit.currentTarget), GUILayout.Height(elementHeight));

				arr = System.Enum.GetValues(typeof(UnitController.Target));			
				count = arr.Length;

				for (int i = 0; i < count; i++) {
					UnitController.Target target = (UnitController.Target) i;
					string targetName = selectedUnit.GetTargetName(target);

					if (GUILayout.Button(new GUIContent(targetName, selectedUnit.GetTargetTip(target)))) {
						selectedUnit.currentTarget = target;
					}
				}

			GUILayout.EndVertical();

			// Conditions
			GUILayout.BeginVertical(GUILayout.Width(elementWidth));

				GUILayout.Box("Current Condition: " + selectedUnit.GetConditionName(selectedUnit.currentCondition), GUILayout.Height(elementHeight));

				arr = System.Enum.GetValues(typeof(UnitController.Condition));
				count = arr.Length;

				for (int i = 0; i < count; i++) {
					UnitController.Condition condition = (UnitController.Condition) i;
					string conditionName = selectedUnit.GetConditionName(condition);

					if (GUILayout.Button(new GUIContent(conditionName))) {
						selectedUnit.currentCondition = condition;
					}
				}

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Confirm ('Q' key)", GUILayout.Width(width), GUILayout.Height(elementHeight)) || Input.GetKeyDown(KeyCode.Q)) {
				if (bSelectingTactics)
					bSelectingTactics = false;	
			}

			GUILayout.EndArea();
		}
	}
	
	private void createSpawnButton(int index, float elementWidth, float elementHeight) {
		UnitController unit = playerFaction.FactionUnits[index].GetComponent<UnitController>();
		string tip = "Gold Cost: " + unit.GoldCost + "g\n"; 
		tip += "Unit Class: " + unit.Class + "\n";
		tip += "Unit Score: " + unit.GetTotalScore();
		
		GUIContent btn = new GUIContent((index+1) + " : " + unit.Name + " (" + unit.Class + ")", tip);
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
	
	private UnitController getCurrentlyPlacingUnit() {
		UnitController unit = null;
		foreach (Entity go in unitsList) {
			if ((go != null) && go.GetComponent<UnitController>().currentUnitState == UnitController.UnitState.PLACING) {
				unit = go.GetComponent<UnitController>();
				break;
			}
		}		
		
		return unit;
	}
	
	private void spawnUnit(int index) {
		UnitController currentlyPlacing = getCurrentlyPlacingUnit();
		if (currentlyPlacing != null) {
			currentlyPlacing.DestroySelf();
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
			unitsList.Add(newUnit.GetComponent<Entity>());		
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
			
			Color guiColor = GUI.color;
			GUI.color = feedbackColor;
			GUILayout.Box(feedbackText, GUILayout.Width(width), GUILayout.Height(height));
			GUI.color = guiColor;
			
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

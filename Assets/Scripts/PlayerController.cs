﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	[HideInInspector]
	public List<GameObject> unitsList;
	
	private GameObject selectedUnit = null;
	
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

		if (Input.GetMouseButtonDown(0)) {			
			selectUnit();
		}
		
		if (Input.GetMouseButtonDown(1)) {
			if (selectedUnit != null) {
				moveUnit();
			}
		}
	
		
	}
					
	private void moveUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		RaycastHit hit;
		if (Physics.Raycast(mouseRay, out hit)) {
			Vector3 clickedPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
			selectedUnit.GetComponent<UnitController>().moveToPosition = clickedPos;
		}
	}
	
	private void selectUnit() {
		Ray mouseRay = playerCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
		
		if (selectedUnit != null) {
			selectedUnit.GetComponent<UnitController>().selected = false;
			selectedUnit = null;
		}
		
		RaycastHit[] hits = Physics.RaycastAll(mouseRay);
		foreach (RaycastHit hit in hits) {
			if (hit.transform.CompareTag("Unit") ||	hit.transform.tag == "Unit") {
				UnitController selectedController = hit.transform.GetComponent<UnitController>();
				if (selectedController.playerOwner == this) {
					selectedUnit = hit.transform.gameObject;
					selectedController.selected = true;
					break;
					
				}
			}
			
		}	
		
	}
	
	void OnGUI() {
		renderSpawnGUIButtons();
		renderSelectedUnitGUI();
		renderGameTime();
	}
	
	private void renderSelectedUnitGUI() {
		if (selectedUnit != null && selectedUnit.GetComponent<UnitController>().selected) {
			float width = screenWidth * 0.2f,
				height = screenHeight * 0.6f;
			float x = screenWidth - width - 5f,
				y = (screenHeight - height)/4f;
			
			GUILayout.BeginArea(new Rect(x, y, width, height));
			GUILayout.BeginVertical();
			
			UnitController selectedController = selectedUnit.GetComponent<UnitController>();
			string unitString = "Selected unit: " + selectedController.Name;
			unitString += "\nUnit hitpoints: " + Mathf.Round(selectedController.CurrentHitPoints) + " / " + Mathf.Round(selectedController.MaxHitPoints);
			unitString += "\nUnit stats: ";
			unitString += "\nDamage: " + selectedController.Damage;
			unitString += "\nAccuracy: " + selectedController.Accuracy;
			unitString += "\nEvasion: " + selectedController.Evasion;
			unitString += "\nArmor: " + selectedController.Armor;
			unitString += "\nPerception Range: " + selectedController.PerceptionRange;
			unitString += "\nAttacking Range: " + selectedController.AttackingRange;
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
		
		GUILayout.BeginArea(new Rect(5f, 5f, 150f, 25f));
		//GUILayout.BeginHorizontal();
		
		GUILayout.Box("Elapsed time: " + Mathf.Round(time).ToString());
		
		//GUILayout.EndHorizontal();
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

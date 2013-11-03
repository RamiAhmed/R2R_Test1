using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	[HideInInspector]
	public List<GameObject> enemies = new List<GameObject>();
	[HideInInspector]
	public List<GameObject> players = new List<GameObject>();
	
	[HideInInspector]
	public float GameTime = 0f;
	
	[HideInInspector]
	public float BuildTime = 0f;
	
	[HideInInspector]
	public int WaveCount = 0;
	
	[HideInInspector]
	public bool ForceSpawn = false;
	
	public int MaxUnitCount = 50;
	
	public float MaxBuildTime = 30f;
	public int WaveSize = 15;
	
	public enum GameState {
		LOADING,
		MENU,
		PLAY,
		PAUSED,
		ENDING
	};
	
	public GameState CurrentGameState = GameState.LOADING;
	
	public enum PlayState {
		BUILD,
		COMBAT,
		NONE
	};
	
	public PlayState CurrentPlayState = PlayState.NONE;
	
	private bool hasSpawnedThisWave = false;
	
	private GameObject miniMapCam;
	
	// Use this for initialization
	void Start () {
		GameObject player = Instantiate(Resources.Load("Player/PlayerObject")) as GameObject;
		GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
		foreach (GameObject point in points) {
			if (point.transform.name.Contains("End")) {
				Vector3 targetPos = point.transform.position;
				targetPos.y += 30f;
				player.transform.position = targetPos;
				break;
			}
		}
		players.Add(player);
	
		miniMapCam = GameObject.FindGameObjectWithTag("MiniMapCam");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.Pause) || Input.GetKeyUp(KeyCode.P)) {
			if (CurrentGameState == GameState.PAUSED) {
				CurrentGameState = GameState.PLAY;
			}
			else if (CurrentGameState == GameState.PLAY) {
				CurrentGameState = GameState.PAUSED;					
			}
		}
		
		if (Input.GetKeyUp(KeyCode.Escape)) {
			if (CurrentGameState == GameState.PLAY || CurrentGameState == GameState.PAUSED) {
				CurrentGameState = GameState.MENU;	
			}
			else if (CurrentGameState == GameState.MENU && GameTime > 0f) {
				CurrentGameState = GameState.PLAY;
			}					
		}
		
		if (CurrentGameState == GameState.PLAY) {
			GameTime += Time.deltaTime;
			
			if (!miniMapCam.activeSelf) {
				miniMapCam.SetActive(true);
			}
			
			if (CurrentPlayState == PlayState.BUILD) {
				BuildTime += Time.deltaTime;
				
				float maxTime = WaveCount <= 0 ? MaxBuildTime * 2f : MaxBuildTime;
				
				if (BuildTime >= maxTime || ForceSpawn) {
					ForceSpawn = false;
					WaveCount++;
					BuildTime = 0f;
					CurrentPlayState = PlayState.COMBAT;
				}
			}
			else if (CurrentPlayState == PlayState.COMBAT) {
				if (!hasSpawnedThisWave) {
					hasSpawnedThisWave = true;
					SpawnWave();
				}				
				else if (CheckForWaveEnd()) {
					CurrentPlayState = PlayState.BUILD;
					hasSpawnedThisWave = false;
				}
			}
		}
		else if (CurrentGameState == GameState.ENDING) {
			Application.Quit();	
		}
		else {
			if (miniMapCam.activeSelf) {
				miniMapCam.SetActive(false);
			}
		}
	}
	
	private bool CheckForWaveEnd() {
		if (enemies.Count <= 0) {	
			return true;
		}
		else {
			return false;
		}
	}
	
	private void SpawnWave() {
		for (int i = 0; i < WaveSize; i++) {
			Invoke("SpawnEnemy", (float)i/3f);	
		}
	}
	
	private void SpawnEnemy() {
		GameObject enemy = Instantiate(Resources.Load("Enemies/Enemy")) as GameObject;
		enemies.Add(enemy);
	}
}

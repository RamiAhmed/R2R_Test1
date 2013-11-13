using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	public List<Entity> enemies;
	public List<GameObject> players;
	
	public float GameTime = 0f;
	public float BuildTime = 0f;
	public int WaveCount = 0;
	
	[HideInInspector]
	public bool ForceSpawn = false;
	
	public int MaxUnitCount = 50;
	
	public float MaxBuildTime = 30f;
	public int WaveSize = 10;
	
	public float StartYPosition = 30f;
	
	public bool GameEnded = false;
	
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
	
	private bool isRestarting = false;
	
	// Use this for initialization
	void Start () {
		enemies = new List<Entity>();
		players = new List<GameObject>();
		
		GameObject player = Instantiate(Resources.Load("Player/PlayerObject")) as GameObject;
		GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
		foreach (GameObject point in points) {
			if (point.transform.name.Contains("End")) {
				Vector3 targetPos = point.transform.position;
				targetPos.y += StartYPosition;
				player.transform.position = targetPos;
				break;
			}
		}
		players.Add(player);
	
		miniMapCam = GameObject.FindGameObjectWithTag("MiniMapCam");
	}
	
	public float GetMaxBuildTime() {
		return WaveCount <= 0 ? MaxBuildTime * 2f : MaxBuildTime;	
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
				
				float maxTime = GetMaxBuildTime();
				
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
			EndGame(isRestarting);
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
			Invoke("SpawnEnemy", (float)i/2f);	
		}
	}
	
	private void SpawnEnemy() {
		GameObject enemy = Instantiate(Resources.Load("Enemies/Enemy")) as GameObject;
		enemies.Add(enemy.GetComponent<Entity>());
	}
	
	public void RestartGame() {
		isRestarting = true;
		this.CurrentGameState = GameState.ENDING;
		Application.LoadLevel(0);	
	}
	
	public void EndGame(bool bRestarting) {
		if (!bRestarting) {
			//Application.Quit();	
			GameEnded = true;
		}
	}
	
	public void QuitGame() {
		Application.Quit();	
	}
	
}
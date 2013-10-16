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
		ENDING
	};
	
	public GameState CurrentGameState = GameState.LOADING;
	
	public enum PlayState {
		BUILD,
		COMBAT,
		ARENA,
		NONE
	};
	
	public PlayState CurrentPlayState = PlayState.NONE;
	
	private bool hasSpawnedThisWave = false;
	
	// Use this for initialization
	void Start () {
		if (CurrentGameState == GameState.LOADING) {
			GameObject player = Instantiate(Resources.Load("PlayerObject")) as GameObject;
			GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
			foreach (GameObject point in points) {
				if (point.transform.name.Contains("End")) {
					Vector3 targetPos = point.transform.position;
					targetPos.y += 40f;
					player.transform.position = targetPos;
					break;
				}
			}
			players.Add(player);
			
			CurrentGameState = GameState.PLAY;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (CurrentGameState == GameState.PLAY) {
			GameTime += Time.deltaTime;
			
			if (CurrentPlayState == PlayState.NONE) {
				CurrentPlayState = PlayState.BUILD;	
			}
			else if (CurrentPlayState == PlayState.BUILD) {
				BuildTime += Time.deltaTime;
				
				if (BuildTime >= MaxBuildTime || ForceSpawn) {
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
		GameObject enemy = Instantiate(Resources.Load("Enemy")) as GameObject;
		enemies.Add(enemy);
	}
}

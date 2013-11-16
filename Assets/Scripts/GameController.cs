using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	public List<Entity> enemies;
	public List<PlayerController> players;
	
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

	public AudioClip BuildMusic, CombatMusic, BackgroundMusic;

	//public AudioSource audioSource;

	private Dictionary<string, AudioSource> audioSources;
	
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
		players = new List<PlayerController>();
		
		PlayerController player = (Instantiate(Resources.Load("Player/PlayerObject")) as GameObject).GetComponent<PlayerController>();
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

		audioSources = new Dictionary<string, AudioSource>();

		addAudioSource("Background", BackgroundMusic);
		addAudioSource("Build", BuildMusic, 0.5f);
		addAudioSource("Combat", CombatMusic, 0.1f);
	}

	private void stopBuildMusic() {
		stopAudioSource("Build");
	}

	private void stopCombatMusic() {
		stopAudioSource("Combat");
	}

	private void stopAudioSource(string type) {
		if (audioSources.ContainsKey(type)) {
			audioSources[type].Stop();
		}
	}

	private void addAudioSource(string type, AudioClip audioClip) {
		addAudioSource(type, audioClip, 1.0f);
	}

	private void addAudioSource(string type, AudioClip audioClip, float volume) {
		if (audioClip != null) {
			audioSources.Add(type, this.gameObject.AddComponent<AudioSource>());
			audioSources[type].playOnAwake = false;
			audioSources[type].clip = audioClip;
			audioSources[type].volume = volume;
		}
	}

	private void playBackgroundMusic() {
		playAudioClip("Background");
	}

	private void playCombatMusic() {
		playAudioClip("Combat");
	}

	private void playBuildMusic() {
		playAudioClip("Build");
	}

	private void playAudioClip(string type) {
		if (audioSources.ContainsKey(type)) {
			if (audioSources[type].clip != null) {
				if (!audioSources[type].isPlaying) {
					audioSources[type].Play();
				}
			}
		}
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
				playBuildMusic();

				BuildTime += Time.deltaTime;
				
				float maxTime = GetMaxBuildTime();
				
				if (BuildTime >= maxTime || ForceSpawn) {
					OnCombatStart();
				}
			}
			else if (CurrentPlayState == PlayState.COMBAT) {
				playCombatMusic();

				if (!hasSpawnedThisWave) {
					hasSpawnedThisWave = true;
					SpawnWave();
				}				
				else if (CheckForWaveEnd()) {
					OnBuildStart();
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

	private void OnBuildStart() {
		CurrentPlayState = PlayState.BUILD;
		hasSpawnedThisWave = false;
		stopCombatMusic();

		foreach (PlayerController player in players) {
			player.DisplayFeedbackMessage("Build Phase is starting.", Color.white);
		}
	}

	private void OnCombatStart() {
		ForceSpawn = false;
		WaveCount++;
		BuildTime = 0f;
		CurrentPlayState = PlayState.COMBAT;
		stopBuildMusic();

		foreach (PlayerController player in players) {
			player.DisplayFeedbackMessage("Combat Phase is starting.", Color.white);
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
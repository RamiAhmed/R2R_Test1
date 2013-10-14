using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	
	public List<GameObject> enemies = new List<GameObject>();
	public List<GameObject> players = new List<GameObject>();
	
	public float GameTime = 0f;
	
	// Use this for initialization
	void Start () {
		/*for (int i = 0; i < 10; i++) {
			GameObject enemy = Instantiate(Resources.Load("Enemy")) as GameObject;
			enemies.Add(enemy);
		}*/
		GameObject enemy = Instantiate(Resources.Load("Enemy")) as GameObject;
		enemies.Add(enemy);
		
		GameObject player = Instantiate(Resources.Load("PlayerObject")) as GameObject;
		GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
		foreach (GameObject point in points) {
			if (point.transform.name.Contains("End")) {
				player.transform.position = point.transform.position;
				break;
			}
		}
		players.Add(player);
	}
	
	// Update is called once per frame
	void Update () {
		GameTime += Time.deltaTime;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : Enemy {

	private enum EnemyState {
		SPAWNING,
		MOVING,
		ATTACKING,
		FLEEING,
		DEAD
	};
	
	private EnemyState currentEnemyState = EnemyState.SPAWNING;
	
	private GameObject counterPlayer;
	
	private Transform endPoint = null;
	
	
	// Use this for initialization
	protected override void Start () {
		base.Start();
		if (currentEnemyState == EnemyState.SPAWNING) {
			
			GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
			foreach (GameObject point in points) {
				if (point.transform.name.Contains("Start")) {
					this.transform.position = point.transform.position;
				}
				else if (point.transform.name.Contains("End")) {
					endPoint = point.transform;	
				}
			}

			counterPlayer = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().players[0];
		
			currentEnemyState = EnemyState.MOVING;
		}
		
	}
	
	protected override void Update() {
		base.Update();
		
		if (IsDead) {
			this.currentEnemyState = EnemyState.DEAD;
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		base.FixedUpdate();
		
		if (currentEnemyState == EnemyState.MOVING) {
			
			if (!MoveTowards(endPoint)) {
				Debug.Log(this.ToString() + " reached the end point!");
				endPoint = null;
			}
			
			foreach (GameObject unit in counterPlayer.GetComponent<PlayerController>().unitsList) {
				if (Vector3.Distance(unit.transform.position, this.transform.position) < PerceptionRange) {
					currentEnemyState = EnemyState.ATTACKING;
					break;
				}
			}

		}
		else if (currentEnemyState == EnemyState.ATTACKING) {
			foreach (GameObject unit in counterPlayer.GetComponent<PlayerController>().unitsList) {
				if (Vector3.Distance(unit.transform.position, this.transform.position) < AttackingRange) {
					Attack(unit.GetComponent<UnitController>());
					break;
				}
				else {
					MoveTowards(unit.transform);
					break;
				}
			}

		}
		else if (currentEnemyState == EnemyState.FLEEING) {
			
		}
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();
		
		if (currentEnemyState == EnemyState.DEAD) {
			_gameController.enemies.Remove(this.gameObject);
			Destroy(this.gameObject);	
		}
	}
	
}

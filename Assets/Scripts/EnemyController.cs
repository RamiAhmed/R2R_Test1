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
	
	private PlayerController counterPlayer;
	
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

			counterPlayer = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().players[0].GetComponent<PlayerController>();
		
			currentEnemyState = EnemyState.MOVING;
		}
		
	}
	
	protected override void Update() {
		base.Update();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (IsDead) {
			this.currentEnemyState = EnemyState.DEAD;
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		base.FixedUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentEnemyState == EnemyState.MOVING) {
			
			Entity nearest = GetNearestUnit(counterPlayer.unitsList);
			if (nearest != null) {
				attackTarget = nearest;
				currentEnemyState = EnemyState.ATTACKING;
				return;
			}
			
			if (gateRef != null) {
				if (Vector3.Distance(this.transform.position, gateRef.transform.position) < AttackingRange) {
					attackTarget = gateRef;
					currentEnemyState = EnemyState.ATTACKING;
				}
			}			
			
			if (Vector3.Distance(this.transform.position, endPoint.position) < AttackingRange) {
				counterPlayer.PlayerLives--;
				this.currentEnemyState = EnemyState.DEAD;
			}
			else {
				MoveTowards(endPoint);	
			}

		}
		else if (currentEnemyState == EnemyState.ATTACKING) {
			if (this.CurrentHitPoints <= this.MaxHitPoints*FleeThreshold) {
				this.currentEnemyState = EnemyState.FLEEING;
				return;
			}
			
			if (attackTarget != null) {
				if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
					Attack(attackTarget);	
				}
				else {
					MoveTowards(attackTarget.transform);
				}
			}
			else {
				attackTarget = GetWeakestUnit(counterPlayer.unitsList);
				if (attackTarget == null) {
					attackTarget = GetNearestUnit(counterPlayer.unitsList);
					if (attackTarget == null) {
						currentEnemyState = EnemyState.MOVING;	
					}
				}
			}
		}
		else if (currentEnemyState == EnemyState.FLEEING) {
			if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < PerceptionRange) {
				FleeFrom(attackTarget.transform);	
			}
			else {
				this.currentEnemyState = EnemyState.MOVING;
			}
		}
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentEnemyState == EnemyState.DEAD) {
			if (IsDead) {
				counterPlayer.PlayerGold += this.GoldReward;
			}
			_gameController.enemies.Remove(this.gameObject);
			Destroy(this.gameObject);	
		}
	}
	
	protected override void RemoveSelf() {
		base.RemoveSelf();
		
		_gameController.enemies.Remove(this.gameObject);
		Destroy(this.gameObject);
	}

}

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
	
	// Use this for initialization
	protected override void Start () {
		base.Start();
		if (currentEnemyState == EnemyState.SPAWNING) {
			
			GameObject redDot = Instantiate(Resources.Load("Misc Objects/RedDot")) as GameObject;
			redDot.transform.parent = this.transform;
			
			GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
			foreach (GameObject point in points) {
				if (point.transform.name.Contains("Start")) {
					this.transform.position = point.transform.position;
					break;
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
			}
			else if (gateRef != null) {
				if (Vector3.Distance(this.transform.position, gateRef.transform.position) < AttackingRange) {
					StopMoving();
					attackTarget = gateRef;
					currentEnemyState = EnemyState.ATTACKING;
				}
				else {
					if (!isMoving) {
						MoveTo(gateRef.transform);
					}
				}
			}
		}
		else if (currentEnemyState == EnemyState.ATTACKING) {
			if (this.CurrentHitPoints < this.MaxHitPoints * this.FleeThreshold && (fGetD20() * 5f) < (this.FleeThreshold * 100f)) {
				this.currentEnemyState = EnemyState.FLEEING;
			}
			
			if (attackTarget != null) {
				if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
					StopMoving();
					Attack(attackTarget);	
				}
				else {
					MoveTo(attackTarget.transform);
				}
			}
			else {
				currentEnemyState = EnemyState.MOVING;	
			}
		}
		else if (currentEnemyState == EnemyState.FLEEING) {
			if (attackTarget != null) {
				if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < PerceptionRange) {
					FleeFrom(attackTarget.transform);	
				}
				else {
					StopMoving();
					this.currentEnemyState = EnemyState.MOVING;
					this.FleeThreshold /= 2f;
				}
			}
			else {
				StopMoving();
				this.currentEnemyState = EnemyState.MOVING;
				this.FleeThreshold /= 2f;
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

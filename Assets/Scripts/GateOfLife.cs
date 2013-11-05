using UnityEngine;
using System.Collections;

public class GateOfLife : Entity {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Gate of Life";
		MaxHitPoints = 200f;
		CurrentHitPoints = MaxHitPoints;
		Armor = 15f;
		
		MovementSpeed = 0f;
//		MaxForce = 0f;
		AttacksPerSecond = 0.5f;
		Damage = 10f;		
		Evasion = 0f;
		Accuracy = 5f;
		PerceptionRange = 5f;
		AttackingRange = 5f;
		FleeThreshold = 0f;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		if (IsDead) {
			PlayerController player = _gameController.players[0].GetComponent<PlayerController>();
			player.PlayerLives = 0;	
		}
		else {
			if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
				if (attackTarget != null) {
					if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
						Attack(attackTarget);	
					}
					else {
						attackTarget = null;
					}
				}
				else {
					Entity enemy = GetNearestUnit(_gameController.enemies);
					if (enemy != null && Vector3.Distance(enemy.transform.position, this.transform.position) < AttackingRange) {
						attackTarget = enemy;
					}
				}
			}
		}
	}
	
}

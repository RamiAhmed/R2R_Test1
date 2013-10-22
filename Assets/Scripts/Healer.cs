using UnityEngine;
using System.Collections;

public class Healer : UnitController {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Healer";
		
		GoldCost = 8;
		
		MaxHitPoints = 125f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 15f;
		Accuracy = 15f;
		Evasion = 10f;
		Armor = 3f;
		
		MovementSpeed = 120f;
	//	MaxForce = 14f;
		
		PerceptionRange = 25f;
		AttackingRange = 10f;
		
		AttacksPerSecond = 1.2f;
		FleeThreshold = 0.25f;	
	}
	
	protected override void PlacedBehaviour() {
		if (attackTarget != null) {
			this.currentUnitState = UnitState.ATTACKING;
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {	
			if (gateRef != null && gateRef.CurrentHitPoints < gateRef.MaxHitPoints) {
				StopMoving();
				GuardOther(gateRef);
			}
			else {
				Entity damagedUnit = GetMostDamagedUnit(playerOwner.unitsList);
				if (damagedUnit.CurrentHitPoints < damagedUnit.MaxHitPoints) {
					if (Vector3.Distance(damagedUnit.transform.position, this.transform.position) < AttackingRange) {
						StopMoving();
						Heal(damagedUnit, this.Damage + fGetD20()/2f);
					}
					else {
						MoveTo(damagedUnit.transform.position);
					}
				}
				else {
					Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
					if (nearestEnemy != null) {
						if (Vector3.Distance(nearestEnemy.transform.position, this.transform.position) < PerceptionRange || gateRef == null) {
							attackTarget = nearestEnemy;
							this.currentUnitState = UnitState.ATTACKING;
							StopMoving();
						}
					}		
				}
			}
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			saveLocation();
		}
	}
	
	protected override void AttackingBehaviour() {
		if (_gameController.CurrentPlayState != GameController.PlayState.COMBAT) {
			currentUnitState = UnitState.PLACED;
			return;
		}
		
		if (attackTarget != null) {
			Entity damagedUnit = GetMostDamagedUnit(playerOwner.unitsList);
			if (this.CurrentHitPoints < this.MaxHitPoints * this.FleeThreshold) {
				if ((fGetD20() * 5f) < (this.FleeThreshold * 100f)) {
					this.currentUnitState = UnitState.FLEEING;
				}
			}
			else if (damagedUnit != null && Vector3.Distance(damagedUnit.transform.position, this.transform.position) < AttackingRange && damagedUnit.CurrentHitPoints < damagedUnit.MaxHitPoints) {
				StopMoving();
				Heal(damagedUnit, this.Damage + fGetD20()/2f);	
			}
			else if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
				Attack(attackTarget);
			}
			else {
				MoveTo(damagedUnit.transform);
			}
		}
		else {
			StopMoving();
			
			this.currentUnitState = UnitState.PLACED;
		}		
	}
}

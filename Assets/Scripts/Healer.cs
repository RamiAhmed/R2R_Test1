using UnityEngine;
using System.Collections;

public class Healer : UnitController {
	
	public float HealThreshold = 0.75f;
	private Entity healTarget = null;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Healer";
		
		GoldCost = 8;
		
		MaxHitPoints = 125f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 10f;
		Accuracy = 15f;
		Evasion = 10f;
		Armor = 3f;
		
		MovementSpeed = 120f;
		
		PerceptionRange = 25f;
		AttackingRange = 10f;
		
		AttacksPerSecond = 0.75f;
		FleeThreshold = 0.25f;	
	}
	
	protected override void HealingBehaviour() {
		if (healTarget != null && healTarget.CurrentHitPoints < healTarget.MaxHitPoints) {
			if (Vector3.Distance(healTarget.transform.position, this.transform.position) < AttackingRange) {
				StopMoving();
				Heal(healTarget, this.Damage + fGetD20()/2f);	
			}
			else {
				MoveTo(healTarget.transform);
			}
		}	
		else {
			healTarget = null;
			this.currentUnitState = UnitController.UnitState.PLACED;			
		}
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
				if (damagedUnit != null && (damagedUnit.CurrentHitPoints < damagedUnit.MaxHitPoints * HealThreshold)) {
					StopMoving();
					healTarget = damagedUnit;
					this.currentUnitState = UnitController.UnitState.HEALING;					
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

}

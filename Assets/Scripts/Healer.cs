using UnityEngine;
using System.Collections;

public class Healer : UnitController {
	
	public float HealThreshold = 0.75f;
	private Entity healTarget = null;
	
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
		if (!Selected) {
			disableRenderCircle();
		}
		
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
			
			if (lookAtPos == Vector3.zero || lookAtPos.sqrMagnitude < 0f) {
				foreach (GameObject waypoint in GameObject.FindGameObjectsWithTag("Waypoint")) {
					if (waypoint.name.Contains("Start")) {
						lookAtPos = waypoint.transform.position;
						lookAtTarget(lookAtPos);
						Debug.Log("lookAtPOs: " + lookAtPos);
						break;
					}
				}			
			}
		}
	}

}

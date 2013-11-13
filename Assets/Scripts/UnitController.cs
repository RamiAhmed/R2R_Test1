using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UnitController : Unit {

	public Unit UpgradesInto = null;

	[HideInInspector]
	public PlayerController playerOwner;

	[HideInInspector]
	public Vector3 moveToPosition = Vector3.zero;

	public enum UnitState {
		PLACING,
		PLACED,
		HEALING,
		ATTACKING,
		FLEEING,
		DEAD
	};

	public UnitState currentUnitState = UnitState.PLACING;

	[HideInInspector]
	public Vector3 LastBuildLocation = Vector3.zero;

	private bool allowedBuildLocation = false;
	private GameObject attackingCircle = null,
					   perceptionCircle = null;
	
	private Entity healTarget = null;
	
	protected Vector3 lookAtPos = Vector3.zero;
	
	// Tactical AI System
	public enum Tactics {
		Attack,
		Follow,
		Guard,	
		HoldTheLine		
	};
	
	public Tactics currentTactic = Tactics.Attack;
	
	public enum Target {
		Nearest,
		Strongest,
		Weakest,
		LowestHP,
		HighestHP
	};
	
	public Target currentTarget = Target.Nearest;
	
	public enum Condition {
		Always,
		HP_75, 
		HP_50,
		HP_25,
		HP_less
	};
	
	public Condition currentCondition = Condition.Always;
	
	public string GetTacticsTip(Tactics tactic) {
		string tip = "";
		switch (tactic) {
			case Tactics.Attack: tip = "Attacking: Unit will attack target at will."; break;
			case Tactics.Guard: tip = "Guard: Unit will attempt to protect target."; break;
			case Tactics.Follow: tip = "Assist: Unit will attempt to follow and assist target."; break;
			case Tactics.HoldTheLine: tip = "Stand Ground: Unit will attempt to stand ground and not move far."; break;
		}
		return tip;			
	}
	
	public string GetTacticsName(Tactics tactic) {
		string name = "";
		switch (tactic) {
			case Tactics.Attack: name = "Attack"; break;
			case Tactics.Guard: name = "Guard"; break;
			case Tactics.Follow: name = "Assist"; break;
			case Tactics.HoldTheLine: name = "Stand Ground"; break;
		}
		return name;
	}
	
	public string GetTargetTip(Target target) {
		string tip = "";
		switch (target) {
			case Target.Nearest: tip = "Nearest: Target will be the nearest enemy."; break;
			case Target.Strongest: tip = "Strongest: Target will be the strongest enemy."; break;
			case Target.Weakest: tip = "Weakest: Target will be the weakest enemy."; break;
			case Target.LowestHP: tip = "Most Damaged: Target will be the most damaged enemy."; break;
			case Target.HighestHP: tip = "Least Damaged: Target will be the least damaged enemy"; break;
		}
		return tip;		
	}
	
	public string GetTargetName(Target target) {
		string name = "";
		switch (target) {
			case Target.Nearest: name = "Nearest"; break;
			case Target.Strongest: name = "Strongest"; break;
			case Target.Weakest: name = "Weakest"; break;
			case Target.LowestHP: name = "Most Damaged"; break;
			case Target.HighestHP: name = "Least Damaged"; break;
		}
		return name;
	}
	
	public Entity GetTacticalTarget() {
		List<Entity> list = null;
		
		switch (currentTactic) {
			case Tactics.Attack: 
			case Tactics.HoldTheLine: list = _gameController.enemies; break;
			case Tactics.Follow:
			case Tactics.Guard: list = playerOwner.unitsList; break;
		}
		
		return list != null ? GetTacticalTarget(list) : null;
	}
	
	public Entity GetTacticalTarget(List<Entity> list) {
		Entity obj = null;	
		if (list != null) {
			switch (currentTarget) {
				case Target.Strongest: obj = GetStrongestUnit(list); break;
				case Target.Weakest: obj = GetWeakestUnit(list); break;
				case Target.LowestHP: obj = GetMostDamagedUnit(list); break;
				case Target.HighestHP: obj = GetLeastDamagedUnit(list); break;
			}
			
			if (obj == null) {
				obj = GetNearestUnit(list);
			}
		}
		else {
			obj = this;
		}		
		
		return obj;
	}
	
	public string GetConditionName(Condition condition) {
		string name = "";
		switch (condition) {
			case Condition.Always: name = "Always"; break;
			case Condition.HP_75: name = "Over 75% HP"; break;
			case Condition.HP_50: name = "Over 50% HP"; break;
			case Condition.HP_25: name = "Over 25% HP"; break;
			case Condition.HP_less: name = "Less than 25% HP"; break;
		}
		return name;
	}
	
	public bool GetIsCurrentConditionTrue() {
		bool result = false;	
		switch (currentCondition) {
			case Condition.Always: result = true; break;
			case Condition.HP_75: result = this.CurrentHitPoints / this.MaxHitPoints > 0.75f; break;
			case Condition.HP_50: result = this.CurrentHitPoints / this.MaxHitPoints > 0.50f; break;
			case Condition.HP_25: result = this.CurrentHitPoints / this.MaxHitPoints > 0.25f; break;
			case Condition.HP_less: result = this.CurrentHitPoints / this.MaxHitPoints <= 0.25f; break;
		}
		return result;
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();

		GameObject greenDot = Instantiate(Resources.Load("Misc Objects/GreenDot")) as GameObject;
		greenDot.transform.parent = this.transform;
		greenDot.transform.localPosition = Vector3.zero;
		
		setupRenderCircle();
	}

	// Update is called once per frame
	protected override void Update () {
		base.Update();

	}

	protected void saveLocation() {
		if ((this.transform.position - LastBuildLocation).sqrMagnitude > 0.1f) {
			LastBuildLocation = this.transform.position;
		}
	}

	private bool buildUnit() {
		bool buildResult = false;
		if (allowedBuildLocation && GetIsPosWalkable(this.transform.position)) {
			if (playerOwner.PlayerGold >= this.GoldCost) {
				playerOwner.PlayerGold -= this.GoldCost;
				this.renderer.material.color = originalMaterialColor;
				currentUnitState = UnitState.PLACED;
				buildResult = true;
			}
			else {
				playerOwner.DisplayFeedbackMessage("You do not have enough gold.");
			}
		}
		else {
			playerOwner.DisplayFeedbackMessage("You cannot build at that location.");
		}

		return buildResult;
	}

	private void checkForCollisions() {
		bool collisions = false;
		float radius = (this.collider.bounds.extents.x + this.collider.bounds.extents.y) / 2f;
		Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, radius);
		foreach (Collider coll in colliderHits) {
			if (coll.GetType() != typeof(TerrainCollider) && coll.gameObject != this.gameObject) {
				//Debug.Log("Colliding with: " + coll);
				toggleRenderMaterial(true);
				collisions = true;
				break;
			}
		}

		if (!collisions) {
			toggleRenderMaterial(false);
		}
	}

	private void toggleRenderMaterial(bool bToggle) {
		if (bToggle) {
			if (allowedBuildLocation) {
				allowedBuildLocation = false;
			}
		}
		else {
			if (!allowedBuildLocation) {
				allowedBuildLocation = true;
			}
		}
	}

	protected override void FixedUpdate() {
		base.FixedUpdate();

		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		if (IsDead) {
			this.currentUnitState = UnitState.DEAD;
		}
		else if (currentUnitState == UnitState.PLACING) {
			PlacingBehaviour();
		}
		else if (currentUnitState == UnitState.PLACED) {
			PlacedBehaviour();
		}
		else if (currentUnitState == UnitState.ATTACKING) {
			AttackingBehaviour();
		}
		else if (currentUnitState == UnitState.FLEEING) {
			FleeingBehaviour();
		}
		else if (currentUnitState == UnitState.HEALING && isHealer) {
			HealingBehaviour();
		}
	}

	protected virtual void HealingBehaviour() {
		if (_gameController.CurrentPlayState != GameController.PlayState.COMBAT) {
			StopMoving();
			currentUnitState = UnitState.PLACED;
			attackTarget = null;
			healTarget = null;
			lastAttacker = null;
		}
		else if (healTarget != null && !healTarget.IsDead && healTarget.CurrentHitPoints < healTarget.MaxHitPoints * HealThreshold) {
			if (GetIsWithinAttackingRange(healTarget)) {
				StopMoving();
				Heal(healTarget, this.Damage + fGetD20()/10f);	
			}
			else {
				if (!GetIsWithinAttackingRange(healTarget)) {
					MoveTo(healTarget.transform);
				}
			}
		}	
		else {
			healTarget = null;
			this.currentUnitState = UnitController.UnitState.PLACED;			
		}
	}

	protected virtual void PlacingBehaviour() {
		if (this.playerOwner != null) {
			if (Input.GetMouseButtonDown(0)) {
				if (buildUnit())
					return;
			}

			if (Input.GetMouseButtonDown(1)) {
				playerOwner.unitsList.Remove(this);
				Destroy(this.gameObject);
				return;
			}

			if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
				playerOwner.unitsList.Remove(this);
				Destroy(this.gameObject);
				return;
			}

			if (this.name != this.Name) {
				this.name = this.Name;
			}

			Ray ray = playerOwner.GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);

			foreach (RaycastHit hit in Physics.RaycastAll(ray)) {
				if (hit.collider.GetType() == typeof(TerrainCollider)) {
					float height = Terrain.activeTerrain.SampleHeight(new Vector3(hit.point.x, 0f, hit.point.z));
					height += this.transform.collider.bounds.size.y/2f + 0.1f;
					this.transform.position = new Vector3(hit.point.x, height, hit.point.z);
					break;
				}
			}

			checkForCollisions();
			
			DrawRangeCircles();
		}
	}

	protected virtual void PlacedBehaviour() {
		if (!Selected) {
			disableRenderCircle();
		}
		
		if (attackTarget != null) {
			StopMoving();
			this.currentUnitState = UnitState.ATTACKING;
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
			if (gateRef != null && gateRef.CurrentHitPoints < gateRef.MaxHitPoints) {
				GuardOther(gateRef);
			}
			else {
				if (isHealer) {
					Entity damagedUnit = GetMostDamagedUnit(playerOwner.unitsList);
					if (damagedUnit != null && (damagedUnit.CurrentHitPoints < damagedUnit.MaxHitPoints * HealThreshold) && 
						GetIsWithinPerceptionRange(damagedUnit)) {
						StopMoving();
						healTarget = damagedUnit;
						this.currentUnitState = UnitController.UnitState.HEALING;	
						return;
					}
				}
				
				Entity tacticalTarget = GetTacticalTarget();
				if (tacticalTarget != null) {
					if (currentTactic == Tactics.Guard) {
						GuardOther(tacticalTarget);
					}
					else if (currentTactic == Tactics.Follow) {
						FollowOther(tacticalTarget);
					}
					else if (currentTactic == Tactics.HoldTheLine) {
						if (GetIsWithinAttackingRange(tacticalTarget)) {
							attackTarget = tacticalTarget; 
							this.currentUnitState = UnitState.ATTACKING;
							StopMoving();
						}
						else if (GetIsWithinAttackingRange(GetNearestUnit(_gameController.enemies))) {
							attackTarget = GetNearestUnit(_gameController.enemies);
							this.currentUnitState = UnitState.ATTACKING;
							StopMoving();
						}
					}
					else {
						if (GetIsWithinPerceptionRange(tacticalTarget)) {
							attackTarget = tacticalTarget;
							this.currentUnitState = UnitState.ATTACKING;
							StopMoving();
						}
					}
					
					if ((attackTarget == null && lastAttacker != null) && GetIsWithinPerceptionRange(lastAttacker)) {
						attackTarget = lastAttacker;
						this.currentUnitState = UnitState.ATTACKING;
						StopMoving();
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
						break;
					}
				}			
			}
		}
	}

	protected virtual void AttackingBehaviour() {
		if (!Selected) {
			disableRenderCircle();
		}
		
		if (_gameController.CurrentPlayState != GameController.PlayState.COMBAT) {
			StopMoving();
			currentUnitState = UnitState.PLACED;
			attackTarget = null;
			lastAttacker = null;
		}
		else if (attackTarget != null) {
			if (this.CurrentHitPoints < this.MaxHitPoints * this.FleeThreshold && (fGetD20() * 5f) < (this.FleeThreshold * 100f)) {
				// Flee by chance
				this.currentUnitState = UnitState.FLEEING;
			}
			else if (GetIsWithinAttackingRange(attackTarget)) {
				// Attack
				Attack(attackTarget);
			}
			else {
				MoveTo(attackTarget.transform);
			}
		}
		else {
			StopMoving();

			this.currentUnitState = UnitState.PLACED;
		}
	}

	protected virtual void FleeingBehaviour() {
		if (!Selected) {
			disableRenderCircle();
		}
		
		if (_gameController.CurrentPlayState != GameController.PlayState.COMBAT) {
			StopMoving();
			currentUnitState = UnitState.PLACED;
			attackTarget = null;
			lastAttacker = null;
		}
		else if (attackTarget != null) {
			if (this.MaxHitPoints - this.CurrentHitPoints > this.FleeThreshold * this.MaxHitPoints) {
				this.currentUnitState = UnitState.PLACED;
				this.FleeThreshold /= 2f;
				attackTarget = null;
				StopMoving();
			}
			else if (GetIsWithinPerceptionRange(attackTarget)) {
				FleeFrom(attackTarget.transform);
			}
			else {
				this.currentUnitState = UnitState.PLACED;
				this.FleeThreshold /= 2f;
				attackTarget = null;
				StopMoving();
			}
		}
		else {
			StopMoving();
			this.currentUnitState = UnitState.PLACED;
			this.FleeThreshold /= 2f;
		}
	}
	
	private void setupRenderCircle() {
		if (attackingCircle == null) {
			attackingCircle = Instantiate(Resources.Load("Misc Objects/Circles/AttackingCircle")) as GameObject;
			attackingCircle.transform.parent = this.transform;
			attackingCircle.transform.localScale = new Vector3(AttackingRange, 1f, AttackingRange);
			attackingCircle.transform.localPosition = Vector3.zero;
			attackingCircle.GetComponentInChildren<MeshRenderer>().enabled = false;
		}
		
		if (perceptionCircle == null) {
			perceptionCircle = Instantiate(Resources.Load("Misc Objects/Circles/PerceptionCircle")) as GameObject;
			perceptionCircle.transform.parent = this.transform;
			perceptionCircle.transform.localScale = new Vector3(PerceptionRange, 1f, PerceptionRange);
			perceptionCircle.transform.localPosition = Vector3.zero;
			perceptionCircle.GetComponentInChildren<MeshRenderer>().enabled = false;
		}
	}
	
	protected void DrawRangeCircles() {
		drawAttackingRange();
		drawPerceptionRange();
	}
	
	protected void drawAttackingRange() {
		if (attackingCircle != null) {
			if (!attackingCircle.GetComponentInChildren<MeshRenderer>().enabled) {
				attackingCircle.GetComponentInChildren<MeshRenderer>().enabled = true;
			}	
		}
	}
	
	protected void drawPerceptionRange() {
		if (perceptionCircle != null) {
			if (!perceptionCircle.GetComponentInChildren<MeshRenderer>().enabled) {
				perceptionCircle.GetComponentInChildren<MeshRenderer>().enabled = true;
			}	
		}
	}
	
	protected void disableRenderCircle() {
		if (attackingCircle != null) {
			if (attackingCircle.GetComponentInChildren<MeshRenderer>().enabled) {
				attackingCircle.GetComponentInChildren<MeshRenderer>().enabled = false;
			}
		}
		
		if (perceptionCircle != null) {
			if (perceptionCircle.GetComponentInChildren<MeshRenderer>().enabled) {
				perceptionCircle.GetComponentInChildren<MeshRenderer>().enabled = false;
			}
		}
	}

	public bool CanUpgrade() {
		return UpgradesInto != null && _gameController.CurrentPlayState == GameController.PlayState.BUILD;
	}

	public void UpgradeUnit() {
		Debug.Log("Upgrade Unit");

		if (CanUpgrade()) {
			StopMoving();

			if (playerOwner.PlayerGold >= UpgradesInto.GoldCost) {
				playerOwner.PlayerGold -= UpgradesInto.GoldCost;

				GameObject newUnit = Instantiate(UpgradesInto.gameObject) as GameObject;

				newUnit.transform.position = this.transform.position;
				playerOwner.unitsList.Add(newUnit.GetComponent<Entity>());
				UnitController unitCont = newUnit.GetComponent<UnitController>();
				unitCont.playerOwner = this.playerOwner;
				unitCont.currentUnitState = UnitState.PLACED;
				unitCont.Select(playerOwner.SelectedUnits);

				playerOwner.DisplayFeedbackMessage("You have upgraded " + this.Name + " into " + UpgradesInto.Name + " for " + UpgradesInto.GoldCost + " gold.", Color.green);

				this.Deselect(playerOwner.SelectedUnits);

				playerOwner.unitsList.Remove(this);
				Destroy(this.gameObject);
			}
			else {
				playerOwner.DisplayFeedbackMessage("You cannot afford to upgrade " + this.Name);
				Debug.Log("You cannot afford to upgrade " + this.Name);
			}
		}
		else {
			Debug.LogWarning("Could not find UpgradesInto for " + this.Name);
		}
	}

	public int GetSellAmount() {
		return Mathf.RoundToInt(this.GoldCost * SellGoldPercentage);
	}

	public void SellUnit() {
		Debug.Log("SellUnit");
		StopMoving();

		int goldReturned = GetSellAmount();
		playerOwner.DisplayFeedbackMessage("You sold " + this.Name + " for " + goldReturned + " gold.", Color.yellow);

		playerOwner.PlayerGold += goldReturned;

		DestroySelf();
	}

	public override void Select(List<Entity> list) {
		if (this.currentUnitState != UnitState.DEAD && this.currentUnitState != UnitState.PLACING) {
			base.Select(list);
			
			if (_gameController.CurrentGameState == GameController.GameState.PLAY) {
				DrawRangeCircles();
			}
		}
	}
	
	public override void Deselect(List<Entity> list) {
		base.Deselect(list);
		disableRenderCircle();	
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();

		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		else if (currentUnitState == UnitState.DEAD) {
			OnDeath();
		}
	}
	
	private void OnDeath() {
		StopMoving();
		StopAllAnimations();
		Debug.Log("Unit dead");
		lookAtPos = Vector3.zero;
		Deselect(playerOwner.SelectedUnits);
		playerOwner.unitsList.Remove(this);
		playerOwner.deadUnitsList.Add(this);

		this.gameObject.SetActive(false);		
	}

	protected override void RemoveSelf() {
		base.RemoveSelf();

		GameObject[] points = GameObject.FindGameObjectsWithTag("Waypoint");
		foreach (GameObject point in points) {
			if (point.transform.name.Contains("End")) {
				LastBuildLocation = point.transform.position;
				break;
			}
		}
		LastBuildLocation.x += Random.Range(-1f, 1f);
		LastBuildLocation.z += Random.Range(-1f, 1f);
		currentUnitState = UnitState.DEAD;
		Deselect(playerOwner.SelectedUnits);
	}
	
	public void DestroySelf() {
		if (playerOwner.unitsList.Contains(this)) {
			playerOwner.unitsList.Remove(this);
		}
		
		Deselect(playerOwner.SelectedUnits);

		Destroy(this.gameObject);		
	}
}

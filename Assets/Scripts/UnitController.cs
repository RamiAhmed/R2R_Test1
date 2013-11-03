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
	
	// Tactical AI System
	public enum Tactics {
		Attack,
		Backstab,
		Charge,
		Flank,
		Flee,
		Follow,
		Guard,
		Heal,	
		HoldTheLine
		
	};
	
	public Tactics currentTactic = Tactics.Attack;
	
	public enum Target {
		Nearest,
		Strongest,
		Weakest,
		LowestHP,
		HighestHP,
		Select
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
	
	public string GetTacticsName(Tactics tactic) {
		string name = "";
		switch (tactic) {
			case Tactics.Attack: name = "Attack"; break;
			case Tactics.Charge: name = "Charge"; break;
			case Tactics.Guard: name = "Guard"; break;
			case Tactics.Heal: name = "Heal"; break;
			case Tactics.Follow: name = "Follow"; break;
			case Tactics.Flank: name = "Flank"; break;
			case Tactics.Backstab: name = "Backstab"; break;
			case Tactics.HoldTheLine: name = "Hold the Line"; break;
			case Tactics.Flee: name = "Flee"; break;
		}
		return name;
	}
	
	public string GetTargetName(Target target) {
		string name = "";
		switch (target) {
			case Target.Nearest: name = "Nearest"; break;
			case Target.Strongest: name = "Strongest"; break;
			case Target.Weakest: name = "Weakest"; break;
			case Target.LowestHP: name = "Most Damaged"; break;
			case Target.HighestHP: name = "Least Damaged"; break;
			case Target.Select: name = "Custom Selection"; break;
		}
		return name;
	}
	
	public Entity GetTacticalTarget(List<GameObject> list) {
		Entity obj = null;	
		switch (currentTarget) {
			case Target.Strongest: obj = GetStrongestUnit(list); break;
			case Target.Weakest: obj = GetWeakestUnit(list); break;
			case Target.LowestHP: obj = GetMostDamagedUnit(list); break;
			case Target.HighestHP: obj = GetLeastDamagedUnit(list); break;
			case Target.Select: obj = null; break; // Not implemented yet
		}
		
		if (obj == null) {
			obj = GetNearestUnit(list);
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
		
		setupRenderCircle(0.15f);
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
		if (allowedBuildLocation) {
			if (playerOwner.PlayerGold >= this.GoldCost) {
				playerOwner.PlayerGold -= this.GoldCost;
				this.renderer.material.color = originalMaterialColor;
				currentUnitState = UnitState.PLACED;
				return true;
			}
			else {
				playerOwner.DisplayFeedbackMessage("You do not have enough gold.");
				return false;
			}
		}
		else {
			playerOwner.DisplayFeedbackMessage("You cannot build at that location.");
			return false;
		}
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
				this.renderer.material.color = Color.red;
				allowedBuildLocation = false;
			}
		}
		else {
			if (!allowedBuildLocation) {
				this.renderer.material.color = Color.green;
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
		else if (currentUnitState == UnitState.HEALING) {
			HealingBehaviour();
		}
	}

	protected virtual void HealingBehaviour() {}

	protected virtual void PlacingBehaviour() {
		if (this.playerOwner != null) {
			if (Input.GetMouseButtonDown(0)) {
				if (buildUnit())
					return;
			}

			if (Input.GetMouseButtonDown(1)) {
				playerOwner.unitsList.Remove(this.gameObject);
				Destroy(this.gameObject);
				return;
			}

			if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
				playerOwner.unitsList.Remove(this.gameObject);
				Destroy(this.gameObject);
				return;
			}

			if (this.name != this.Name) {
				this.name = this.Name;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				float height = Terrain.activeTerrain.SampleHeight(new Vector3(hit.point.x, 0f, hit.point.z));
				height += this.transform.collider.bounds.size.y/2f + 0.1f;
				this.transform.position = new Vector3(hit.point.x, height, hit.point.z);
			}


			checkForCollisions();
			
			drawAttackingRange();
			drawPerceptionRange();
		}
	}

	protected virtual void PlacedBehaviour() {
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
				Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
				if (nearestEnemy != null) {
					if (Vector3.Distance(nearestEnemy.transform.position, this.transform.position) < PerceptionRange) {
						attackTarget = nearestEnemy;
						this.currentUnitState = UnitState.ATTACKING;
						StopMoving();
					}
				}
			}	
			
		}
		else if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
			saveLocation();
		}
	}

	protected virtual void AttackingBehaviour() {
		if (_gameController.CurrentPlayState != GameController.PlayState.COMBAT) {
			currentUnitState = UnitState.PLACED;
			return;
		}

		if (attackTarget != null) {
			if (this.CurrentHitPoints < this.MaxHitPoints * this.FleeThreshold && (fGetD20() * 5f) < (this.FleeThreshold * 100f)) {
				this.currentUnitState = UnitState.FLEEING;
			}
			else if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
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
		if (attackTarget != null) {
			if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < PerceptionRange) {
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
	
	private void setupRenderCircle(float width) {
		attackingCircle = Instantiate(Resources.Load("Misc Objects/Circles/AttackingCircle")) as GameObject;
		attackingCircle.transform.parent = this.transform;
		attackingCircle.transform.localScale = new Vector3(AttackingRange/5f, 1f, AttackingRange/5f);
		attackingCircle.renderer.enabled = false;
		
		perceptionCircle = Instantiate(Resources.Load("Misc Objects/Circles/PerceptionCircle")) as GameObject;
		perceptionCircle.transform.parent = this.transform;
		perceptionCircle.transform.localScale = new Vector3(PerceptionRange/5f, 1f, PerceptionRange/5f);
		perceptionCircle.renderer.enabled = false;
	}
	
	protected void drawAttackingRange() {
		if (attackingCircle != null) {
			if (!attackingCircle.renderer.enabled) {
				attackingCircle.renderer.enabled = true;
			}	
		}
	}
	
	protected void drawPerceptionRange() {
		if (perceptionCircle != null) {
			if (!perceptionCircle.renderer.enabled) {
				perceptionCircle.renderer.enabled = true;
			}	
		}
	}
	
	protected void disableRenderCircle() {
		if (attackingCircle != null) {
			if (attackingCircle.renderer.enabled) {
				attackingCircle.renderer.enabled = false;
			}
		}
		
		if (perceptionCircle != null) {
			if (perceptionCircle.renderer.enabled) {
				perceptionCircle.renderer.enabled = false;
			}
		}
	}

	public bool CanUpgrade() {
		return UpgradesInto != null;
	}

	public void UpgradeUnit() {
		Debug.Log("Upgrade Unit");

		if (CanUpgrade()) {
			StopMoving();

			if (playerOwner.PlayerGold >= UpgradesInto.GoldCost) {
				playerOwner.PlayerGold -= UpgradesInto.GoldCost;

				GameObject newUnit = Instantiate(UpgradesInto.gameObject) as GameObject;

				newUnit.transform.position = this.transform.position;
				playerOwner.unitsList.Add(newUnit);
				UnitController unitCont = newUnit.GetComponent<UnitController>();
				unitCont.playerOwner = this.playerOwner;
				unitCont.currentUnitState = UnitState.PLACED;
				unitCont.Select(playerOwner.SelectedUnits);

				playerOwner.DisplayFeedbackMessage("You have upgraded " + this.Name + " into " + UpgradesInto.Name + " for " + UpgradesInto.GoldCost + " gold.", Color.green);

				this.Deselect(playerOwner.SelectedUnits);

				playerOwner.unitsList.Remove(this.gameObject);
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
		playerOwner.unitsList.Remove(this.gameObject);

		Deselect(playerOwner.SelectedUnits);

		Destroy(this.gameObject);
	}

	public override void Select(List<Entity> list) {
		if (this.currentUnitState != UnitState.DEAD && this.currentUnitState != UnitState.PLACING) {
			base.Select(list);
			
			if (_gameController.CurrentPlayState == GameController.PlayState.BUILD &&
				_gameController.CurrentGameState == GameController.GameState.PLAY) {
				drawAttackingRange();
				drawPerceptionRange();
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

		if (currentUnitState == UnitState.DEAD) {
			StopMoving();
			Debug.Log("Unit dead");
			Deselect(playerOwner.SelectedUnits);
			playerOwner.unitsList.Remove(this.gameObject);
			playerOwner.deadUnitsList.Add(this.gameObject);

			this.gameObject.SetActive(false);
		}
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
}

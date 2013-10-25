using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UnitController : Unit {
	
	public List<Unit> UpgradesList = new List<Unit>();
	public int CurrentUpgrade = 0;
	
	[HideInInspector]
	public PlayerController playerOwner;

	[HideInInspector]
	public Vector3 moveToPosition = Vector3.zero;
	
	public enum UnitState {
		PLACING,
		PLACED,
		MOVING,
		HEALING,
		ATTACKING,
		FLEEING,
		DEAD
	};
	
	public UnitState currentUnitState = UnitState.PLACING;
	
	[HideInInspector]
	public Vector3 LastBuildLocation = Vector3.zero;
	
	private bool allowedBuildLocation = false;
	
	
	// Use this for initialization
	protected override void Start () {	
		base.Start();
		
		GameObject greenDot = Instantiate(Resources.Load("Misc Objects/GreenDot")) as GameObject;
		greenDot.transform.parent = this.transform;
		
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
			//Debug.LogWarning("Cannot build at that location");
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
		}
	}
	
	protected virtual void PlacedBehaviour() {
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
	
	public bool CanUpgrade() {
		return CurrentUpgrade+1 <= UpgradesList.Count;	
	}
	
	public void UpgradeUnit() {
		Debug.Log("Upgrade Unit");		
		
		if (CanUpgrade()) {
			StopMoving();
			
			CurrentUpgrade++;
			Unit upgradesInto = UpgradesList[CurrentUpgrade-1];
			
			if (upgradesInto == null) {
				Debug.LogWarning("Could not find upgrades into unit for " + this.Name);
			}			
			else if (playerOwner.PlayerGold >= upgradesInto.GoldCost) {
				playerOwner.PlayerGold -= upgradesInto.GoldCost;
		
				GameObject newUnit = Instantiate(upgradesInto.gameObject) as GameObject;

				newUnit.transform.position = this.transform.position;
				playerOwner.unitsList.Add(newUnit);
				UnitController unitCont = newUnit.GetComponent<UnitController>();
				unitCont.playerOwner = this.playerOwner;
				unitCont.currentUnitState = UnitState.PLACED;
				
				playerOwner.DisplayFeedbackMessage("You have upgraded " + this.Name + " into " + upgradesInto.Name + " for " + upgradesInto.GoldCost + " gold.", Color.green);

				playerOwner.unitsList.Remove(this.gameObject);
				Destroy(this.gameObject);
			}
			else {
				playerOwner.DisplayFeedbackMessage("You cannot afford to upgrade " + this.Name);
			}
		}
	}
	
	public void SellUnit() {
		Debug.Log("SellUnit");
		StopMoving();
		
		int goldReturned = Mathf.RoundToInt(this.GoldCost * SellGoldPercentage);
		playerOwner.DisplayFeedbackMessage("You sold " + this.Name + " for " + goldReturned + " gold.", Color.yellow);		
		
		playerOwner.PlayerGold += goldReturned;
		playerOwner.unitsList.Remove(this.gameObject);		
		
		//this.gameObject.SetActive(false);
		Destroy(this.gameObject);
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentUnitState == UnitState.DEAD) {
			StopMoving();
			Debug.Log("Unit dead");
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
	}
}

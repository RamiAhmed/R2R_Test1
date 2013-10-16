using UnityEngine;
using System.Collections;

public class UnitController : Unit {
	
	[HideInInspector]
	public PlayerController playerOwner;

	[HideInInspector]
	public Vector3 moveToPosition = Vector3.zero;
	
	[HideInInspector]
	public enum UnitState {
		PLACING,
		PLACED,
		ATTACKING,
		DEAD
	};
	
	public UnitState currentUnitState;
	
	[HideInInspector]
	public Vector3 LastBuildLocation = Vector3.zero;
	
	private bool savedLocation = false;
	private Color originalMaterialColor = Color.white;
	private bool allowedBuildLocation = false;
	
	
	// Use this for initialization
	protected override void Start () {	
		base.Start();
		currentUnitState = UnitState.PLACING;
		originalMaterialColor = this.renderer.material.color;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (IsDead) {
			this.currentUnitState = UnitState.DEAD;
		}		
		else if (currentUnitState == UnitState.PLACING) {
			if (Input.GetMouseButtonDown(0)) {
				if (buildUnit())
					return;
			}
			
			if (Input.GetMouseButtonDown(1)) {
				playerOwner.unitsList.Remove(this.gameObject);
				Destroy(this.gameObject);
				return;
			}
			
			if (!this.rigidbody.IsSleeping()) {
				this.rigidbody.isKinematic = true;
				this.collider.isTrigger = true;
				this.rigidbody.Sleep();
			}
		
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				float height = Terrain.activeTerrain.SampleHeight(new Vector3(hit.point.x, hit.point.y, hit.point.z));
				height += this.transform.collider.bounds.size.y;
				this.transform.position = new Vector3(hit.point.x, height, hit.point.z);
			}
			
			checkForCollisions();
			
		}
		else if (currentUnitState == UnitState.PLACED) {
			if (this.rigidbody.IsSleeping()) {
				this.rigidbody.WakeUp();
				this.rigidbody.isKinematic = false;
				this.collider.isTrigger = false;
			}
			
			if (_gameController.CurrentPlayState == GameController.PlayState.COMBAT) {
				if (!savedLocation) {
					savedLocation = true;
					LastBuildLocation = this.transform.position;
				}
			}
			else if (_gameController.CurrentPlayState == GameController.PlayState.BUILD) {
				if (savedLocation) {
					savedLocation = false;
				}
			}
		}
	}
	
	private bool buildUnit() {
		if (allowedBuildLocation) {
			playerOwner.PlayerGold -= this.GoldCost;
			this.renderer.material.color = originalMaterialColor;
			currentUnitState = UnitState.PLACED;
			return true;
		}
		else {
			Debug.LogWarning("Cannot build at that location");
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
		
		if (currentUnitState == UnitState.PLACED) {
			if (moveToPosition.sqrMagnitude > 0f) {
				if (!MoveTowards(moveToPosition)) {
					moveToPosition = Vector3.zero;
				}
			}				

			Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
			if (nearestEnemy != null) {
				attackTarget = nearestEnemy;
				this.currentUnitState = UnitState.ATTACKING;
			}
		}
		else if (currentUnitState == UnitState.ATTACKING) {
			if (attackTarget != null) {
				if (Vector3.Distance(attackTarget.transform.position, this.transform.position) < AttackingRange) {
					Attack(attackTarget);
				}
				else {
					MoveTowards(attackTarget.transform);
				}
			}
			else {
				attackTarget = GetNearestUnit(_gameController.enemies);
				if (attackTarget == null) {
					this.currentUnitState = UnitState.PLACED;
				}
			}
		}
	}
	
	protected override void LateUpdate() {
		base.LateUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentUnitState == UnitState.DEAD) {
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

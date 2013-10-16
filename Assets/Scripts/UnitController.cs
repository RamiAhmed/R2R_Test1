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
	
	[HideInInspector]
	public UnitState currentUnitState;
	
	[HideInInspector]
	public Vector3 LastBuildLocation = Vector3.zero;
	
	private bool savedLocation = false;
	
	
	// Use this for initialization
	protected override void Start () {	
		base.Start();
		currentUnitState = UnitState.PLACING;
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
				playerOwner.PlayerGold -= this.GoldCost;
				currentUnitState = UnitState.PLACED;
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
			//if (this.gameObject.renderer.enabled) {
				playerOwner.unitsList.Remove(this.gameObject);		
				playerOwner.deadUnitsList.Add(this.gameObject);
				
				//this.gameObject.SetActive(false);
				//this.gameObject.renderer.enabled = false;
				this.gameObject.SetActive(false);
			//}
		}
	}
}

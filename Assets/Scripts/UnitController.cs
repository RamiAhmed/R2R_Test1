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
		BUILT,
		ATTACKING,
		DEAD
	};
	
	[HideInInspector]
	public UnitState currentUnitState;
	
	
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
				currentUnitState = UnitState.BUILT;
				return;
			}
		
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				float height = Terrain.activeTerrain.SampleHeight(new Vector3(hit.point.x, hit.point.y, hit.point.z));
				height += this.transform.collider.bounds.size.y;
				this.transform.position = new Vector3(hit.point.x, height, hit.point.z);
			}
		}
	}
	
	protected override void FixedUpdate() {
		base.FixedUpdate();
		
		if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			return;
		}
		
		if (currentUnitState == UnitState.BUILT) {
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
					this.currentUnitState = UnitState.BUILT;
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
			playerOwner.unitsList.Remove(this.gameObject);
			Destroy(this.gameObject);	
		}
	}
}

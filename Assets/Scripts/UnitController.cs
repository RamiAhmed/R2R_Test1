using UnityEngine;
using System.Collections;

public class UnitController : Unit {
	
	[HideInInspector]
	public PlayerController playerOwner;
	[HideInInspector]
	public bool selected;
	[HideInInspector]
	public Vector3 moveToPosition = Vector3.zero;
	
	[HideInInspector]
	public enum UnitState {
		PLACING,
		BUILT,
		ATTACKING
	};
	
	[HideInInspector]
	public UnitState currentUnitState;
	
	private GameController _gameController;
	
	// Use this for initialization
	protected override void Start () {	
		base.Start();
		currentUnitState = UnitState.PLACING;
		
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		
		if (currentUnitState == UnitState.PLACING) {
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
		if (currentUnitState == UnitState.BUILT) {
			if (moveToPosition.sqrMagnitude > 0f) {
				if (!MoveTowards(moveToPosition)) {
					moveToPosition = Vector3.zero;
				}
			}				
			
			foreach (GameObject enemy in _gameController.enemies) {
				if (Vector3.Distance(enemy.transform.position, this.transform.position) < PerceptionRange) {
					this.currentUnitState = UnitState.ATTACKING;
					break;
				}
			}	
		}
		else if (currentUnitState == UnitState.ATTACKING) {
			foreach (GameObject enemy in _gameController.enemies) {
				if (Vector3.Distance(enemy.transform.position, this.transform.position) < AttackingRange) {
					Attack(enemy.GetComponent<EnemyController>());
					break;
				}
				else {
					MoveTowards(enemy.transform);
					break;
				}
			}	
		}
	}
	
}

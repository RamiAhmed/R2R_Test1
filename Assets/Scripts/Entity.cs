using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class Entity : MonoBehaviour {
	
	public string Name = "Entity";
	public float Damage = 1f,
				Accuracy = 1f,
				Evasion = 1f,
				Armor = 1f,
				CurrentHitPoints = 100f,
				MaxHitPoints = 100f,
				MovementSpeed = 2f,
		//		MaxForce = 10f,
				PerceptionRange = 10f,
				AttackingRange = 2f,
				AttacksPerSecond = 1f,
				FleeThreshold = 0.1f; // = 10 % health
	
	public bool Selected = false,
				IsDead = false;
	
	protected GameController _gameController = null;
	protected Entity attackTarget = null;
	protected Entity lastAttacker = null;
	protected GateOfLife gateRef = null;
	protected bool isMoving = false;
	protected Color originalMaterialColor = Color.white;
	
	private float lastAttack = 0f;
	private float killY = -100f;
	
	//private float totalMoveDistance = 0f;
	//private Vector3 _velocity = Vector3.zero;
	private Vector3 targetPosition = Vector3.zero;
	
	private Seeker seeker;
	private Path path;
	private CharacterController controller;
	
	private int currentWaypoint = 0;
	private float nextWaypointDistance = 3f;
	
	private float repathRate = 1.5f;
	private float lastRepath = -1f;

	void Awake() {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		gateRef = GameObject.FindGameObjectWithTag("GateOfLife").GetComponent<GateOfLife>();
		seeker = this.GetComponent<Seeker>();
		controller = this.GetComponent<CharacterController>();
		originalMaterialColor = this.renderer.material.color;
	}
	
	protected virtual void Start() {}
	
	protected virtual void Update() {
		if (this.Selected) {
			this.renderer.material.color = Color.blue;	
		}
		else {
			this.renderer.material.color = originalMaterialColor;
		}
	}

	protected virtual void FixedUpdate() {
		MoveEntity();
	}
	
	protected virtual void LateUpdate() {
		if (this.transform.position.y <= killY) {
			RemoveSelf();
		}
	}
	
	protected virtual void RemoveSelf() {}
	
	public float GetTotalScore() {
		return (Damage + Accuracy + Evasion + Armor + (MaxHitPoints/10f) + MovementSpeed + PerceptionRange + AttackingRange);
	}
	
	public void Heal(Entity target, float healAmount) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in Heal method");
			return;
		}	
		else {
			if ((GetIsUnit(this.gameObject) && GetIsUnit(target.gameObject)) ||
				(GetIsEnemy(this.gameObject) && GetIsEnemy(target.gameObject))) {
				target.CurrentHitPoints = target.CurrentHitPoints + healAmount > MaxHitPoints ? MaxHitPoints : target.CurrentHitPoints + healAmount;
			}
		}
		
	}
	
	public void GuardOther(Entity target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in GuardOther method");
			return;
		}	
		else {
			if (target.lastAttacker != null) {
				this.attackTarget = target.lastAttacker;
			}
			else if (target.attackTarget != null) {
				this.attackTarget = target.attackTarget;
			}
			else {
				MoveTo(target.transform);
			}
		}
	}
	
	public void FleeFrom(Transform target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in FleeFrom method");
			return;
		}
		
		FleeFrom(target.position);
	}
	
	public void FleeFrom(Vector3 target) {
		if (target == Vector3.zero || target.sqrMagnitude <= 0f) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in FleeFrom method");
			return;
		}

		Vector3 direction = (this.transform.position - target).normalized * MovementSpeed;
		MoveTo(direction);
	}
	
	public void MoveTo(Transform target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in MoveTo method");
			return;
		}	
		
		MoveTo(target.position);
	}
	
	public void MoveTo(Vector3 position) {
		if (position == Vector3.zero || position.sqrMagnitude <= 0f) {
			Debug.LogWarning("Could not find target (" + position.ToString() + ") in MoveTo method");
		}
		else {
			if (Time.time - lastRepath > repathRate) {
				if (!seeker.IsDone()) {
					StopMoving();
				}
				
				lastRepath = Time.time + Random.value * repathRate * 0.5f;
				targetPosition = position;
				isMoving = true;
				seeker.StartPath(this.transform.position, targetPosition, OnPathComplete);
				
				//Debug.Log("Move to: " + position);
			}
		}
		
	}
	
	private void OnPathComplete(Path p) {
		p.Claim(this);
		if (!p.error) {
			if (path != null) {
				path.Release(this);
			}
			
			path = p;
			currentWaypoint = 0;
		}
		else {
			p.Release(this);
		}
	}
	
	private void MoveEntity() {
		if (path == null) {
			return;
		}
		
		List<Vector3> vectorPath = path.vectorPath;
		
        if (currentWaypoint >= vectorPath.Count) {
//            Debug.Log("End of Path reached");
            StopMoving();
            return;
        }
        
        Vector3 direction = (path.vectorPath[currentWaypoint] - this.transform.position).normalized;
        direction *= MovementSpeed * Time.fixedDeltaTime * 2f;
        controller.SimpleMove(direction);
        
		if ((this.transform.position - vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance) {
            currentWaypoint++;
            return;
        }
	}
	
	protected void StopMoving() {
		if (path != null && isMoving) {
			path.Release(this);
			path = null;
			isMoving = false;
		}
	}
	
	protected int GetD20() {
		return Random.Range(1, 20);	
	}
	
	protected float fGetD20() {
		return Random.Range(1f, 20f);	
	}
	
	public void ReceiveDamage(float damage) {
		if (damage <= 0f) {
			Debug.LogWarning("Receive Damage cannot damage 0 or less");
			return;
		}
		
		this.CurrentHitPoints -= damage;
		if (this.CurrentHitPoints <= 0f) {
			//Debug.Log(this.ToString() + " is dead");
			this.IsDead = true;
		}
	}
	
	public void SetIsNotDead(bool fullHealth) {
		this.IsDead = false;
		if (fullHealth) {
			this.CurrentHitPoints = this.MaxHitPoints;
		}
		else {
			this.CurrentHitPoints = 1f;
		}
	}
	
	protected bool Attack(Entity opponent) {
		if (opponent.IsDead) {
			attackTarget = null;
			return false;
		}
		
		StopMoving();
		
		bool hitResult = false;
		float currentTime = Time.time;
		if (currentTime - lastAttack > AttacksPerSecond) {
			lastAttack = currentTime;
			
			this.transform.LookAt(attackTarget.transform.position);
			
			if (this.Accuracy + fGetD20() > opponent.Evasion + fGetD20()) {
				float damage = (this.Damage - opponent.Armor) + fGetD20();
				opponent.ReceiveDamage(damage);
				hitResult = true;
				Debug.Log(this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
			}
			else {
				Debug.Log(this.Name + " missed " + opponent.Name);	
			}
			
			opponent.lastAttacker = this.gameObject.GetComponent<Entity>();
		}
		return hitResult;
	}
	
	public bool GetIsUnit(GameObject go) {
		return go.GetComponent<UnitController>() != null;
	}
	
	public bool GetIsEnemy(GameObject go) {
		return go.GetComponent<EnemyController>() != null;	
	}
	
	public bool GetIsGate(GameObject go) {
		return go.GetComponent<GateOfLife>() != null;
	}
	
	protected Entity GetNearestUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;
		
		GameObject nearest = null;
		float shortestDistance = PerceptionRange;
		foreach (GameObject unit in list) {
			float distance = Vector3.Distance(unit.transform.position, this.transform.position);
			if (distance < shortestDistance) {
				nearest = unit;
				shortestDistance = distance;
			}
		}
		
		return nearest != null ? nearest.GetComponent<Entity>() : null;
	}
	
	protected Entity GetWeakestUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;
		
		GameObject weakest = null;
		float weakestScore = list[0].GetComponent<Entity>().GetTotalScore();
		foreach (GameObject unit in list) {
			float score = unit.GetComponent<Entity>().GetTotalScore();
			if (score < weakestScore)	{
				weakest = unit;
				weakestScore = score;
			}
		}
		
		return weakest != null ? weakest.GetComponent<Entity>() : null;
	}
	
	protected Entity GetMostDamagedUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;
		
		GameObject mostDamaged = null;
		float damage = list[0].GetComponent<Entity>().CurrentHitPoints;
		foreach (GameObject unit in list) {
			float hp = unit.GetComponent<Entity>().MaxHitPoints - unit.GetComponent<Entity>().CurrentHitPoints;
			if (hp < damage) {
				mostDamaged = unit;
				damage = hp;
			}
		}
		
		return mostDamaged != null ? mostDamaged.GetComponent<Entity>() : null;
	}		
}

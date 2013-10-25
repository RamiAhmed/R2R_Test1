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
				PerceptionRange = 10f,
				AttackingRange = 2f,
				AttacksPerSecond = 1f,
				FleeThreshold = 0.1f; // = 10 % health
	
	public GameObject Bullet;
	
	[HideInInspector]
	public bool Selected = false,
				IsDead = false;
		
	[HideInInspector]
	public Entity lastAttacker = null;
	
	protected GameController _gameController = null;
	protected Entity attackTarget = null;
	protected GateOfLife gateRef = null;
	protected bool isMoving = false;
	protected Color originalMaterialColor = Color.white;
	protected float lastAttack = 0f;
	
	private float killY = -100f;
	
	private Vector3 targetPosition = Vector3.zero;
	
	private Seeker seeker;
	private Path path;
	private CharacterController controller;
	
	private int currentWaypoint = 0;
	private float nextWaypointDistance = 3f;
	
	private float repathRate = 1.5f,
				  lastRepath = -1f;

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
		else if (_gameController.CurrentGameState != GameController.GameState.PLAY) {
			StopMoving();
		}
	}
	
	protected virtual void RemoveSelf() {
		StopMoving();
	}
	
	public void Select(List<Entity> list) {
		if (!this.Selected && !this.IsDead) {
			this.Selected = true;
			list.Add(this);
		}
	}
	
	public void Deselect(List<Entity> list) {
		if (this.Selected || this.IsDead) {
			this.Selected = false;
			if (list.Contains(this)) {
				list.Remove(this);
			}
		}
	}
	
	public float GetTotalScore() {
		return (Damage + Accuracy + Evasion + Armor + (MaxHitPoints/10f) + (MovementSpeed/10f) + PerceptionRange + AttackingRange);
	}
	
	public void Heal(Entity target, float healAmount) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in Heal method");
		}	
		else {
			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;
				
				if ((GetIsUnit(this.gameObject) && GetIsUnit(target.gameObject)) || (GetIsEnemy(this.gameObject) && GetIsEnemy(target.gameObject))) {
					target.CurrentHitPoints = target.CurrentHitPoints + healAmount > MaxHitPoints ? MaxHitPoints : target.CurrentHitPoints + healAmount;
					Debug.Log(this.Name + " healed " + target.Name + " for " + healAmount + " hitpoints");
				}
			}
		}
		
	}
	
	public void GuardOther(Entity target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in GuardOther method");
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
		}
		else {
			FleeFrom(target.position);
		}
	}
	
	public void FleeFrom(Vector3 target) {
		if (target == Vector3.zero || target.sqrMagnitude <= 0f) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in FleeFrom method");
		}
		else {
			Vector3 direction = (this.transform.position - target).normalized * MovementSpeed;
			MoveTo(direction);
		}
	}
	
	public void MoveTo(Transform target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in MoveTo method");
		}	
		else {
			MoveTo(target.position);
		}
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
			StopMoving();
			
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
	
	public void StopMoving() {
		if (path != null) {
			path.Release(this);
			path = null;
		}
		
		if (isMoving) {
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
	
	protected virtual bool Attack(Entity opponent) {
		bool hitResult = false;
		StopMoving();
		
		if (opponent.IsDead || opponent == null) {
			attackTarget = null;	
		}		
		else {
			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;
				
				this.transform.LookAt(opponent.transform.position);
				
				if (Bullet != null && Bullet) {
					ShootBullet(opponent);
				}
				
				if (this.Accuracy + fGetD20() > opponent.Evasion + fGetD20()) {
					float damage = (this.Damage - opponent.Armor) + fGetD20();
					opponent.ReceiveDamage(damage);
					hitResult = true;
					Debug.Log(this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
				}
				else {
					Debug.Log(this.Name + " missed " + opponent.Name);	
				}
				
				opponent.lastAttacker = this;
			}
		}
		return hitResult;
	}
	
	protected virtual void ShootBullet(Entity opponent) {
		GameObject newBullet = Instantiate(Bullet) as GameObject;
		Physics.IgnoreCollision(newBullet.collider, this.transform.collider);
		Bullet bullet = newBullet.GetComponent<Bullet>();
		bullet.Target = opponent.transform.position;		
		bullet.Owner = this.gameObject;
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
	
	protected Entity GetStrongestUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;
		
		GameObject strongest = null;
		float strongestScore = 0;
		foreach (GameObject unit in list) {
			float score = unit.GetComponent<Entity>().GetTotalScore();
			if (score > strongestScore) {
				strongest = unit;
				strongestScore = score;
			}
		}
		
		return strongest != null ? strongest.GetComponent<Entity>() : null;
	}
	
	protected Entity GetMostDamagedUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;
		
		GameObject mostDamaged = null;
		float damage = list[0].GetComponent<Entity>().CurrentHitPoints;
		foreach (GameObject unit in list) {
			float hp = unit.GetComponent<Entity>().MaxHitPoints - unit.GetComponent<Entity>().CurrentHitPoints;
			if (hp < damage && hp > 1f) {
				mostDamaged = unit;
				damage = hp;
			}
		}
		
		return mostDamaged != null ? mostDamaged.GetComponent<Entity>() : null;
	}		
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour {
	
	public string Name = "Entity";
	public float Damage = 1f,
				Accuracy = 1f,
				Evasion = 1f,
				Armor = 1f,
				CurrentHitPoints = 100f,
				MaxHitPoints = 100f,
				MovementSpeed = 5f,
				MaxForce = 10f,
				PerceptionRange = 10f,
				AttackingRange = 2f,
				AttacksPerSecond = 1f,
				FleeThreshold = 0.1f; // = 10 % health
	
	public bool Selected = false,
				IsDead = false;
	
	protected GameController _gameController = null;
	protected Entity attackTarget = null;
	protected GateOfLife gateRef = null;
	
	private float lastAttack = 0f;
	private float killY = -100f;
	
	private float totalMoveDistance = 0f;
	
	void Awake() {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		gateRef = GameObject.FindGameObjectWithTag("GateOfLife").GetComponent<GateOfLife>();
	}
	
	protected virtual void Start() {}
	
	protected virtual void Update() {}

	protected virtual void FixedUpdate() {}
	
	protected virtual void LateUpdate() {
		if (this.transform.position.y <= killY) {
			RemoveSelf();
		}
	}
	
	protected virtual void RemoveSelf() {}
	
	public float GetTotalScore() {
		return (Damage + Accuracy + Evasion + Armor + (MaxHitPoints/10f) + MovementSpeed + MaxForce + PerceptionRange + AttackingRange);
	}
	
	public void FleeFrom(Transform target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in MoveTowards method");
			return;
		}
		
		FleeFrom(target.position);
	}
	
	public void FleeFrom(Vector3 target) {
		if (target == Vector3.zero || target.sqrMagnitude <= 0f) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") to flee from");
			return;
		}
		
		Vector3 direction = -(target - this.transform.position).normalized * MovementSpeed;
		
		Vector3 velocity = Vector3.ClampMagnitude(direction - this.rigidbody.velocity, MaxForce);
		velocity.y = 0f;
		
		this.rigidbody.AddForce(velocity);		
	}
	
	public bool MoveTowards(Transform target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in MoveTowards method");
			return false;
		}
		
		return MoveTowards(target.position);
	}
	
	public bool MoveTowards(Vector3 target) {
		bool result = false;
		if (target == Vector3.zero || target.sqrMagnitude <= 0f) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in MoveTowards method");
			return result;
		}
		
		if (totalMoveDistance == 0f) {
			totalMoveDistance = Vector3.Distance(this.transform.position, target);
		}
			
		float distance = Vector3.Distance(this.transform.position, target);
		if (distance > 1f) {
			this.transform.LookAt(target);
			
			Vector3 direction = this.transform.forward * MovementSpeed * ((distance/totalMoveDistance)*10f);
			Vector3 velocity = Vector3.ClampMagnitude(direction - this.rigidbody.velocity, MaxForce);
			velocity.y = 0f;
			
			this.rigidbody.AddForce(velocity);	
			result = true;			
		}
		else {
			this.rigidbody.velocity = Vector3.zero;
			//this.rigidbody.angularVelocity = Vector3.zero;
			totalMoveDistance = 0f;
		}
		
		return result;
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
		
		bool hitResult = false;
		float currentTime = Time.time;
		if (currentTime - lastAttack > AttacksPerSecond) {
			lastAttack = currentTime;
			
			if (this.Accuracy + fGetD20() > opponent.Evasion + fGetD20()) {
				float damage = (this.Damage - opponent.Armor) + fGetD20();
				opponent.ReceiveDamage(damage);
				hitResult = true;
				Debug.Log(this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
			}
			else {
				Debug.Log(this.Name + " missed " + opponent.Name);	
			}
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
			float hp = unit.GetComponent<Entity>().CurrentHitPoints;
			if (hp < damage) {
				mostDamaged = unit;
				damage = hp;
			}
		}
		
		return mostDamaged != null ? mostDamaged.GetComponent<Entity>() : null;
	}		
}

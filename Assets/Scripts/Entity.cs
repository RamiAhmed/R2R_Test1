using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {
	
	public string Name = "Entity";
	public float Damage = 0f,
				Accuracy = 0f,
				Evasion = 0f,
				Armor = 0f,
				CurrentHitPoints = 100f,
				MaxHitPoints = 100f,
				MovementSpeed = 1f,
				MaxForce = 1f,
				PerceptionRange = 5f,
				AttackingRange = 1f,
				AttacksPerSecond = 1f;
	
	public bool Selected = false,
				IsDead = false;
	
	protected GameController _gameController;
	
	private float lastAttack = 0f;
	
	void Awake() {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
	}
	
	protected virtual void Start() {}
	
	protected virtual void Update() {}

	protected virtual void FixedUpdate() {}
	
	protected virtual void LateUpdate() {}
	
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
			
		float distance = Vector3.Distance(this.transform.position, target);
		if (distance > AttackingRange/2f) {
			this.transform.LookAt(target);
			
			Vector3 direction = this.transform.forward * MovementSpeed * distance;
			Vector3 velocity = Vector3.ClampMagnitude(direction - this.rigidbody.velocity, MaxForce);
			velocity.y = 0f;
			
			this.rigidbody.AddForce(velocity);	
			result = true;			
		}
		else {
			this.rigidbody.velocity = Vector3.zero;
			//this.rigidbody.angularVelocity = Vector3.zero;
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
			Debug.Log(this.ToString() + " is dead");
			this.IsDead = true;
		}
	}
	
	protected bool Attack(Entity opponent) {
		bool result = false;
		float currentTime = Time.time;
		if (currentTime - lastAttack > AttacksPerSecond) {
			lastAttack = currentTime;
			
			if (this.Accuracy + fGetD20() > opponent.Evasion + fGetD20()) {
				float damage = (this.Damage - opponent.Armor) + fGetD20();
				opponent.ReceiveDamage(damage);
				result = true;
				Debug.Log(this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
			}
			else {
				Debug.Log(this.Name + " missed " + opponent.Name);	
			}
		}
		return result;
	}
}

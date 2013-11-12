using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class Entity : MonoBehaviour {

	public string Name = "Entity",
				Class = "Entity";
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
	public Texture2D ProfilePicture = null;

	public GameObject Bullet = null,
					AlternateBullet = null;
	
	public int attackCount = 0, killCount = 0, attackedCount = 0;
	
	public string WalkAnimation = "", 
				  AttackAnimation = "";
	
	[HideInInspector]
	public bool Selected = false,
				IsDead = false;

	[HideInInspector]
	public Entity lastAttacker = null,
				attackTarget = null;
	
	protected GameController _gameController = null;	
	protected GateOfLife gateRef = null;
	protected bool isMoving = false;
	protected Color originalMaterialColor = Color.white;
	protected float lastAttack = 0f;

	protected float meleeDistance = 5f;

	private float killY = -100f;

	private Vector3 targetPosition = Vector3.zero;

	private Seeker seeker;
	private Path path;
	private CharacterController controller;

	private int currentWaypoint = 0;
	private float nextWaypointDistance = 1.5f;

	private float repathRate = 1.5f,
				  lastRepath = -1f;
	
	private Animation animation;
	
	
	void Awake() {
		_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		gateRef = GameObject.FindGameObjectWithTag("GateOfLife").GetComponent<GateOfLife>();
		seeker = this.GetComponent<Seeker>();
		controller = this.GetComponent<CharacterController>();
		originalMaterialColor = this.renderer.material.color;
		
		animation = this.GetComponent<Animation>();
		if (animation == null) {
			animation = this.GetComponentInChildren<Animation>();
		}
	}

	protected virtual void Start() {}

	protected virtual void Update() {
		/*if (this.Selected) {
			this.renderer.material.color = Color.blue;
		}
		else {
			this.renderer.material.color = originalMaterialColor;
		}*/
		
		if (animation != null) {
			if (isMoving) {				
				if (!animation.IsPlaying(GetWalkAnimation())) {
					animation.Play(GetWalkAnimation());
				}
			}
		}
	}
	
	protected string GetWalkAnimation() {
		return WalkAnimation;
	}
	
	protected string GetAttackAnimation() {
		return AttackAnimation;	
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

	protected bool GetIsMelee() {
		return AttackingRange <= meleeDistance;
	}

	public virtual void Select(List<Entity> list) {
		if (!this.Selected && !this.IsDead) {
			this.Selected = true;
			if (!list.Contains(this)) {
				list.Add(this);
			}
		}
	}

	public virtual void Deselect(List<Entity> list) {
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
	
	public bool GetIsAlly(Entity target) {
		return (this.GetIsUnit() && target.GetIsUnit()) || (this.GetIsEnemy() && target.GetIsEnemy());
	}
	
	public void Heal(Entity target, float healAmount) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in Heal method");
		}
		else {
			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;

				if (GetIsAlly(target)) {
					target.CurrentHitPoints = target.CurrentHitPoints + healAmount > target.MaxHitPoints ? target.MaxHitPoints : target.CurrentHitPoints + healAmount;
					ShootBullet(target, true);
					if (animation != null) {
						animation.Play(GetAttackAnimation());	
					}
					Debug.Log(_gameController.GameTime + ": " + this.Name + " healed " + target.Name + " for " + healAmount + " hitpoints");
				}
				else {
					Debug.LogWarning(_gameController.GameTime + ": " + this.Name + " tried to heal non-ally : " + target.Name);	
				}
			}
		}

	}

	public void GuardOther(Entity target) {
		if (target == null || !target) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in GuardOther method");
		}
		else {
			Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
			if (target.lastAttacker != null) {
				this.attackTarget = target.lastAttacker;
			}
			else if (nearestEnemy != null && Vector3.Distance(nearestEnemy.transform.position, this.transform.position) < AttackingRange) {
				this.attackTarget = nearestEnemy;
			}
			else {
				MoveTo(target.transform);
			}
		}
	}
	
	public void FollowOther(Entity target) {
		if (target == null) {
			Debug.LogWarning("Could not find target (" + target.ToString() + ") in FollowOther method");	
		}
		else {
			Entity nearestEnemy = GetNearestUnit(_gameController.enemies);
			if (target.attackTarget != null) {
				this.attackTarget = target.attackTarget;	
			}
			else if (nearestEnemy != null && Vector3.Distance(nearestEnemy.transform.position, this.transform.position) < AttackingRange) {
				this.attackTarget = nearestEnemy;
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
				
				seeker.StartPath(this.transform.position, targetPosition, OnPathComplete);
				
				lookAtTarget(position);
				//Debug.Log("Move to: " + position);
			}
		}

	}

	private void OnPathComplete(Path p) {
		p.Claim(this);
		if (!p.error) {
			StopMoving();
			isMoving = true;

			path = p;
			currentWaypoint = 0;
		}
		else {
			p.Release(this);
			isMoving = false;
		}
	}

	private void MoveEntity() {
		if (path == null) {
			return;
		}

		List<Vector3> vectorPath = path.vectorPath;

        if (currentWaypoint >= vectorPath.Count) {
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
		//Debug.Log("STOP MOVING");
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
	
	public float GetDamagePerSecond() {
		return (this.Damage + 9f) * AttacksPerSecond;
		// +9 = assumed mean value of d20
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
	
	public void StopAllAnimations() {
		if (animation != null) {
			animation.Stop();
		}
	}

	public void SetIsNotDead() {
		SetIsNotDead(true);
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
	
	protected void lookAtTarget(Vector3 target) {
		if (!this.GetIsGate()) {
			Vector3 targetLookPos = target;
			targetLookPos.y = this.collider.bounds.extents.y;
			this.transform.LookAt(targetLookPos);
		}		
	}

	protected virtual bool Attack(Entity opponent) {
		bool hitResult = false;
		StopMoving();

		if (opponent.IsDead || opponent == null) {
			attackTarget = null;
			this.killCount += 1;
		}
		else {
			float currentTime = Time.time;
			if (currentTime - lastAttack > 1f/AttacksPerSecond) {
				lastAttack = currentTime;
				
				lookAtTarget(opponent.transform.position);

				if (Bullet != null) {
					ShootBullet(opponent);
				}

				if (this.Accuracy + fGetD20() > opponent.Evasion + fGetD20()) {
					float damage = (this.Damage - opponent.Armor) + fGetD20();
					opponent.ReceiveDamage(damage);
					hitResult = true;
					Debug.Log(_gameController.GameTime + ": " + this.Name + " hit " + opponent.Name + " with " + damage.ToString() + " damage");
				}
				else {
					Debug.Log(_gameController.GameTime + ": " + this.Name + " missed " + opponent.Name);
				}
				
				this.attackCount += 1;
				opponent.attackedCount += 1;

				if (opponent.lastAttacker == null) {
					opponent.lastAttacker = this;
				}
				
				if (animation != null) {
					animation.Play(GetAttackAnimation());
				}					
			}
		}
		return hitResult;
	}
	
	protected virtual void ShootBullet(Entity opponent) {
		ShootBullet(opponent, false);	
	}

	protected virtual void ShootBullet(Entity opponent, bool bAlternate) {
		GameObject newBullet = null;
		if (!bAlternate) {
			if (Bullet != null) {
				newBullet = Instantiate(Bullet) as GameObject;
			}
			else {
				Debug.LogWarning("Could not find Bullet");
			}
		}
		else {
			if (AlternateBullet != null) {
				newBullet = Instantiate(AlternateBullet) as GameObject;	
			}
			else {
				Debug.LogWarning("Could not find Alternate Bullet");
			}
		}
			
		if (newBullet.collider != null) {
			Physics.IgnoreCollision(newBullet.collider, this.transform.collider);
		}
		Bullet bullet = newBullet.GetComponent<Bullet>();
		bullet.Target = opponent.transform.position;
		bullet.Owner = this.gameObject;
	}

	public bool GetIsUnit() {
		return this != null && this.transform.GetComponent<UnitController>() != null;
	}

	public bool GetIsEnemy() {
		return this != null && this.transform.GetComponent<EnemyController>() != null;
	}

	public bool GetIsGate() {
		return this != null && this.transform.GetComponent<GateOfLife>() != null;
	}

	protected Entity GetNearestUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;

		GameObject nearest = null;
		float shortestDistance = PerceptionRange;
		foreach (GameObject unit in list) {
			float distance = Vector3.Distance(unit.transform.position, this.transform.position);
			if (distance < shortestDistance && unit != this.gameObject) {
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
			if (score < weakestScore && unit != this.gameObject)	{
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
			if (score > strongestScore && unit != this.gameObject) {
				strongest = unit;
				strongestScore = score;
			}
		}

		return strongest != null ? strongest.GetComponent<Entity>() : null;
	}

	protected Entity GetLeastDamagedUnit(List <GameObject> list) {
		if (list.Count <= 0) {
			return null;
		}

		GameObject leastDamaged = null;
		float damage = 100;
		foreach (GameObject unit in list) {
			float hpDiff = unit.GetComponent<Entity>().MaxHitPoints - unit.GetComponent<Entity>().CurrentHitPoints;
			if (hpDiff < damage || hpDiff <= 0f && unit != this.gameObject) {
				leastDamaged = unit;
				damage = hpDiff;
			}
		}

		return leastDamaged != null ? leastDamaged.GetComponent<Entity>() : null;
	}

	protected Entity GetMostDamagedUnit(List<GameObject> list) {
		if (list.Count <= 0)
			return null;

		GameObject mostDamaged = null;
		float damage = 0;
		foreach (GameObject unit in list) {
			float hpDiff = unit.GetComponent<Entity>().MaxHitPoints - unit.GetComponent<Entity>().CurrentHitPoints;
			if (hpDiff > damage && hpDiff >= 0f && unit != this.gameObject) {
				mostDamaged = unit;
				damage = hpDiff;
			}
		}

		return mostDamaged != null ? mostDamaged.GetComponent<Entity>() : null;
	}
}

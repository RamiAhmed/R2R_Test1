using UnityEngine;
using System.Collections;

public class Enemy : Entity {

	// Use this for initialization
	protected override void Start () {
		MovementSpeed = 5f;
		MaxForce = 10f;
		Name = "Enemy";
		AttackingRange = 5f;
		PerceptionRange = 100f;
		Debug.Log ("Enemy created");
	}
}

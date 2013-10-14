using UnityEngine;
using System.Collections;

public class Unit : Entity {


	// Use this for initialization
	protected override void Start () {
		Name = "Unit";
		MovementSpeed = 5f;
		MaxForce = 20f;
		AttackingRange = 5f;
		PerceptionRange = 100f;
		Debug.Log ("Unit created");
	}
	
	
}

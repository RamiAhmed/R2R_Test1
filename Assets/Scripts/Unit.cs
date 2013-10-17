using UnityEngine;
using System.Collections;

public class Unit : Entity {
	
	public int GoldCost = 1;

	// Use this for initialization
	protected override void Start () {
		//Name = "Unit";
	//	MaxForce = 25f;
	//	MovementSpeed = 10f;
		Debug.Log ("Unit created");
	}
	
	
}

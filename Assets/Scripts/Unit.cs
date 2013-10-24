using UnityEngine;
using System.Collections;

public class Unit : Entity {
	
	public int GoldCost = 1;
	public float SellGoldPercentage = 0.5f;

	// Use this for initialization
	protected override void Start () {
		Debug.Log ("Unit created");
	}
	
	
}

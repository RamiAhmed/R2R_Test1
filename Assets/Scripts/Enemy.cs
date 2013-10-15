using UnityEngine;
using System.Collections;

public class Enemy : Entity {
	
	public int GoldReward = 2;

	// Use this for initialization
	protected override void Start () {
		Name = "Enemy";
		Debug.Log ("Enemy created");
	}
}

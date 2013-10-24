using UnityEngine;
using System.Collections;

public class Guard : UnitController {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Guard";
		
		GoldCost = 4;
		
		MaxHitPoints = 150f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 2f;
		Accuracy = 4f;
		Evasion = 6f;
		Armor = 10f;
		
		MovementSpeed = 75f;
	//	MaxForce = 8f;
		
		PerceptionRange = 12f;
		AttackingRange = 2.5f;
		
		AttacksPerSecond = 0.9f;
		FleeThreshold = 0.05f;
	}
}

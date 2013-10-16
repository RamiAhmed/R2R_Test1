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
		
		Damage = 3f;
		Accuracy = 5f;
		Evasion = 10f;
		Armor = 10f;
		
		MovementSpeed = 4f;
		MaxForce = 8f;
		
		PerceptionRange = 12f;
		AttackingRange = 2f;
		
		AttacksPerSecond = 0.9f;
		FleeThreshold = 0.05f;
	}
}

using UnityEngine;
using System.Collections;

public class Soldier : UnitController {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Soldier";
		
		GoldCost = 2;
		
		MaxHitPoints = 100f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 10f;
		Accuracy = 10f;
		Evasion = 5f;
		Armor = 3f;
		
		MovementSpeed = 100f;
	//	MaxForce = 10f;
		
		PerceptionRange = 10f;
		AttackingRange = 2f;
		
		AttacksPerSecond = 1f;
		FleeThreshold = 0.1f;
	}
}

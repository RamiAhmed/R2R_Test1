using UnityEngine;
using System.Collections;

public class Healer : UnitController {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Healer";
		
		GoldCost = 8;
		
		MaxHitPoints = 125f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 15f;
		Accuracy = 15f;
		Evasion = 10f;
		Armor = 3f;
		
		MovementSpeed = 120f;
	//	MaxForce = 14f;
		
		PerceptionRange = 25f;
		AttackingRange = 10f;
		
		AttacksPerSecond = 1.2f;
		FleeThreshold = 0.25f;
	}
}

﻿using UnityEngine;
using System.Collections;

public class Ranged : UnitController {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Ranged";
		
		GoldCost = 6;
		
		MaxHitPoints = 75f;
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 15f;
		Accuracy = 10f;
		Evasion = 7f;
		Armor = 3f;
		
		MovementSpeed = 6f;
		MaxForce = 12f;
		
		PerceptionRange = 20f;
		AttackingRange = 10f;
		
		AttacksPerSecond = 1.25f;
		FleeThreshold = 0.25f;
	}
	
}

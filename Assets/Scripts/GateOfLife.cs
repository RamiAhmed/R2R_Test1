using UnityEngine;
using System.Collections;

public class GateOfLife : Entity {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Gate of Life";
		MaxHitPoints = 200f;
		CurrentHitPoints = MaxHitPoints;
		Armor = 15f;
		
		MovementSpeed = 0f;
//		MaxForce = 0f;
		AttacksPerSecond = 0.5f;
		Damage = 10f;		
		Evasion = 0f;
		Accuracy = 5f;
		PerceptionRange = 5f;
		AttackingRange = 5f;
		FleeThreshold = 0f;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		if (IsDead) {
			PlayerController player = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().players[0].GetComponent<PlayerController>();
			//player.DisplayFeedbackMessage("You have lost your " + this.Name);
			player.PlayerLives = 0;
			Destroy(this.gameObject);	
		}
	}
}

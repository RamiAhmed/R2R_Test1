using UnityEngine;
using System.Collections;

public class GateOfLife : Entity {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		Name = "Gate of Life";
		MaxHitPoints = 200f;
		CurrentHitPoints = MaxHitPoints;
		Armor = 10f;
		
		MovementSpeed = 0f;
		MaxForce = 0f;
		AttacksPerSecond = 0f;
		Damage = 0f;		
		Evasion = 0f;
		Accuracy = 0f;
		PerceptionRange = 0f;
		AttackingRange = 0f;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		if (IsDead) {
			Destroy(this.gameObject);	
			GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().players[0].GetComponent<PlayerController>().DisplayFeedbackMessage("You have lost your " + this.Name);
		}
	}
}

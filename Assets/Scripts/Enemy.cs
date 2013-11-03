using UnityEngine;
using System.Collections;

public class Enemy : Entity {
	
	public int GoldReward = 1;

	// Use this for initialization
	protected override void Start () {
		Debug.Log ("Enemy created");
		
		int wave = _gameController.WaveCount;
		
		GoldReward = Mathf.Clamp(wave, 1, 10);
		
		MaxHitPoints = 100f + (wave*20f);
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 10f + (wave*2f);
		Accuracy = 5f + (wave*3f);
		Evasion = 5f + (wave*3f);
		Armor = 3f + (wave*2f);
		
		MovementSpeed = 140f + (wave*20f);
	//	MaxForce = 10f + (wave-1f);
		
		PerceptionRange = 10f + (wave/1.5f);
		AttackingRange = 2f + (wave/5f);
		
		AttacksPerSecond = 1f + (wave/5f);
		FleeThreshold = 0.1f - (wave/100f);
	}
}

using UnityEngine;
using System.Collections;

public class Enemy : Entity {
	
	public int GoldReward = 1;

	// Use this for initialization
	protected override void Start () {
		Name = "Enemy";
		Debug.Log ("Enemy created");
		
		int wave = _gameController.WaveCount;
		
		GoldReward = Mathf.Clamp(wave, 1, 10);
		
		MaxHitPoints = 100f + (wave*15f);
		CurrentHitPoints = MaxHitPoints;
		
		Damage = 10f + (wave*1.5f);
		Accuracy = 5f + (wave*2f);
		Evasion = 5f + (wave*2f);
		Armor = 3f + (wave*1.5f);
		
		MovementSpeed = 5f + (wave-1f);
		MaxForce = 10f + (wave-1f);
		
		PerceptionRange = 10f + (wave/2f);
		AttackingRange = 2f + (wave/10f);
		
		AttacksPerSecond = 1f + (wave/10f);
		FleeThreshold = 0.1f - (wave/100f);
	}
}

using UnityEngine;
using System.Collections;

public class Enemy : Entity {
	
	public int GoldReward = 1;
	
	public float HitPointsScaleFactor = 10f,
				DamageScaleFactor = 1f,
				AccuracyScaleFactor = 1f,
				EvasionScaleFactor = 1f,
				ArmorScaleFactor = 1f,
				MovementSpeedScaleFactor = 1.5f,
				PerceptionRangeScaleFactor = 0.5f,
				AttackingRangeScaleFactor = 0.3f,
				AttacksPerSecondScaleFactor = 0.1f,
				FleeThresholdScaleFactor = 0.01f,
				WaveSizeScaleFactor = 0f;
				

	// Use this for initialization
	protected override void Start () {
		Debug.Log ("Enemy created");
		
		float height = Terrain.activeTerrain.SampleHeight(this.transform.position);
		height += this.transform.collider.bounds.size.y/2f + 0.1f;
		this.transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);
		
		int wave = _gameController.WaveCount-1;
		
		GoldReward = Mathf.Clamp(wave, 1, 10);
		
		MaxHitPoints += (wave*HitPointsScaleFactor);
		CurrentHitPoints = MaxHitPoints;
		
		Damage += wave*DamageScaleFactor;
		Accuracy += wave*AccuracyScaleFactor;
		Evasion += wave*EvasionScaleFactor;
		Armor += wave*ArmorScaleFactor;
		
		MovementSpeed += (wave*MovementSpeedScaleFactor);
		
		PerceptionRange += (wave*PerceptionRangeScaleFactor);
		AttackingRange += (wave*AttackingRangeScaleFactor);
		
		AttacksPerSecond += (wave*AttacksPerSecondScaleFactor);
		FleeThreshold -= (wave*FleeThresholdScaleFactor);
		
		_gameController.WaveSize += Mathf.RoundToInt(wave * WaveSizeScaleFactor);
	}
}

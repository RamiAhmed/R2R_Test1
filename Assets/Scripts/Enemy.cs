using UnityEngine;
using System.Collections;

public class Enemy : Entity {
	
	public int GoldReward = 1;

	// Use this for initialization
	protected override void Start () {
		Debug.Log ("Enemy created");
		
		float height = Terrain.activeTerrain.SampleHeight(this.transform.position);
		height += this.transform.collider.bounds.size.y/2f + 0.1f;
		this.transform.position = new Vector3(this.transform.position.x, height, this.transform.position.z);
		
		int wave = _gameController.WaveCount-1;
		
		GoldReward = Mathf.Clamp(wave, 1, 10);
		
		MaxHitPoints += (wave*20f);
		CurrentHitPoints = MaxHitPoints;
		
		Damage += wave;
		Accuracy += wave;
		Evasion += wave;
		Armor += wave;
		
		MovementSpeed += (wave*2);
		
		PerceptionRange += (wave/2f);
		AttackingRange += (wave/3f);
		
		AttacksPerSecond += (wave/10f);
		FleeThreshold -= (wave/100f);
	}
}

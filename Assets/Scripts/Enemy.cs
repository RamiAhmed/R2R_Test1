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
		
		int wave = _gameController.WaveCount;
		
		GoldReward = Mathf.Clamp(wave, 1, 10);
		
		MaxHitPoints += (wave*20f);
		CurrentHitPoints = MaxHitPoints;
		
		Damage += (wave*2f);
		Accuracy += (wave*3f);
		Evasion += (wave*3f);
		Armor += (wave*2f);
		
		MovementSpeed += (wave*20f);
		
		PerceptionRange += (wave/1.5f);
		AttackingRange += (wave/3f);
		
		AttacksPerSecond += (wave/5f);
		FleeThreshold -= (wave/100f);
	}
}

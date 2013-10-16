using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction : MonoBehaviour {
	
	public List<Unit> FactionUnits;
	public int NumberOfTiers = 4;
	
	//private PlayerController playerRef;

	// Use this for initialization
	void Start () {
		//playerRef = this.gameObject.GetComponent<PlayerController>();
		
		FactionUnits = new List<Unit>(4);
		
		for (int i = 0; i < NumberOfTiers; i++) {
			addFactionUnit(i);	
		}
	}
	
	private Unit addFactionUnit(int index) {
		string unitName = "";
		switch (index) {
			case 0: unitName = "Soldier"; break;
			case 1: unitName = "Guard"; break;
			case 2: unitName = "Ranged"; break;
			case 3: unitName = "Healer"; break;			
		}
		
		Object obj = Resources.Load("Units/" + unitName);
		if (obj != null) {
			GameObject unit = Instantiate(obj) as GameObject;
			if (unit != null) {
				Debug.Log("AddFactionUnit : " + unitName + ", index: " + index);
				FactionUnits.Add(unit.GetComponent<Unit>());
				unit.name = unitName;
				unit.SetActive(false);
			}
			else {
				Debug.LogWarning("unit is null. Could not find " + unitName + " by index " + index);
			}
		}
		else {
			Debug.LogWarning("obj is null. Could not find " + unitName + " by index " + index);
		}
		
		return FactionUnits[index] != null ? FactionUnits[index] : null;
	}
}

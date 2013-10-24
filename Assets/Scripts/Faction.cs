using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Faction : MonoBehaviour {
	
	public List<Unit> FactionUnits;
	public int NumberOfTiers = 4;
	
	// Use this for initialization
	void Start () {
		FactionUnits = new List<Unit>(4);
		
		for (int i = 0; i < NumberOfTiers; i++) {
			addFactionUnit(i);	
		}
		
		Invoke("inactivateFactionUnits", 0.1f);
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
				FactionUnits.Add(unit.GetComponent<Unit>());
				unit.name = unitName;
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
	
	private void inactivateFactionUnits() {
		foreach (Unit unit in FactionUnits) {
			unit.gameObject.SetActive(false);
		}		
	}
}

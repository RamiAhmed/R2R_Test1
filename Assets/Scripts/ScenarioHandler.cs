using UnityEngine;
using System.Collections;

public class ScenarioHandler : MonoBehaviour {

	public enum ScenarioState {
		WITH_TAIS,
		WITHOUT_TAIS,
		NONE
	}

	public ScenarioState CurrentScenario = ScenarioState.NONE;
	public ScenarioState LastScenario = ScenarioState.NONE;

	// Use this for initialization
	void Start () {	
		DontDestroyOnLoad(this.gameObject);

		if (LastScenario == ScenarioState.NONE) {
			if (Random.Range(0, 2) == 0) {
				CurrentScenario = ScenarioState.WITH_TAIS;
			}
			else {
				CurrentScenario = ScenarioState.WITHOUT_TAIS;
			}
		}
	}

	public void IterateScenario() {
		if (LastScenario == ScenarioState.NONE) {
			LastScenario = CurrentScenario;
			CurrentScenario = LastScenario == ScenarioState.WITH_TAIS ? ScenarioState.WITHOUT_TAIS : ScenarioState.WITH_TAIS;
		}
	}
}

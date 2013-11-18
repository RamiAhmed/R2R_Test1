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

	public bool DoneTesting = false;

	// Use this for initialization
	void Start () {	
		DontDestroyOnLoad(this.gameObject);

		CurrentScenario = Random.Range(0, 2) == 0 ? ScenarioState.WITH_TAIS : ScenarioState.WITHOUT_TAIS;
	}

	public void IterateScenario() {
		if (LastScenario == ScenarioState.NONE) {
			LastScenario = CurrentScenario;
			CurrentScenario = LastScenario == ScenarioState.WITHOUT_TAIS ? ScenarioState.WITH_TAIS : ScenarioState.WITHOUT_TAIS;
		}
		else {
			DoneTesting = true;
			CurrentScenario = ScenarioState.WITH_TAIS;
		}

		Invoke("reloadLevel", 3f);
	}

	private void reloadLevel() {
		Application.LoadLevel(0);
	}
}

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

	void Awake() {
		DontDestroyOnLoad(transform.gameObject);

		GameObject[] scenarioHandlers = GameObject.FindGameObjectsWithTag("ScenarioHandler");
		if (scenarioHandlers.Length > 1) {
			for (int i = 1; i < scenarioHandlers.Length; i++) {
				Destroy(scenarioHandlers[i]);
			}
		}
	}

	void Start () {	
		if (CurrentScenario == ScenarioState.NONE) {
			CurrentScenario = Random.Range(0, 2) == 0 ? ScenarioState.WITH_TAIS : ScenarioState.WITHOUT_TAIS;
		}
	}

	public void IterateScenario() {
		if (LastScenario == ScenarioState.NONE) {
			LastScenario = CurrentScenario;
			CurrentScenario = LastScenario == ScenarioState.WITHOUT_TAIS ? ScenarioState.WITH_TAIS : ScenarioState.WITHOUT_TAIS;
			Debug.Log("Second Scenario commencing");
		}
		else {
			DoneTesting = true;
			CurrentScenario = ScenarioState.WITH_TAIS;
			Debug.Log("Done Scenario testing");
		}
	}
}

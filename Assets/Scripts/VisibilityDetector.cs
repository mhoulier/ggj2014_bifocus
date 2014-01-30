using UnityEngine;
using System.Collections;

public class VisibilityDetector : MonoBehaviour {
	
	private bool viewByPlayer1 = false;
	private bool viewByPlayer2 = false;
	private bool wasVisibleByP1 = false;
	private bool wasVisibleByP2 = false;
	NpcController controller;

	bool firstUpdate = true;

	// Use this for initialization
	void Start () {
	
		//Debug.Log("Visibility started");

		controller = GetComponent<NpcController>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (firstUpdate)
		{
			// set the default display
			controller.UpdateVisibility(viewByPlayer1, viewByPlayer2);
			firstUpdate = false;
		}
	}

	void OnTriggerEnter(Collider other) {
	
		if (other.gameObject.tag == "visibilityCone1")
			viewByPlayer1 = true;
		else if (other.gameObject.tag == "visibilityCone2")
			viewByPlayer2 = true;
	
		// update the NPC visibility
		UpdateVisibility();
	}

	void OnTriggerExit(Collider other) {

		if (other.gameObject.tag == "visibilityCone1")
			viewByPlayer1 = false;
		else if (other.gameObject.tag == "visibilityCone2")
			viewByPlayer2 = false;

		// update the NPC visibility
		UpdateVisibility();
	}

	void UpdateVisibility()
	{
		// Update only if necessary
		if (wasVisibleByP1 != viewByPlayer1 || wasVisibleByP2 != viewByPlayer2)
		{
			//Debug.Log("VISIBILITY UPDTATED");

			controller.UpdateVisibility(viewByPlayer1, viewByPlayer2);
		}

		wasVisibleByP1 = viewByPlayer1;
		wasVisibleByP2 = viewByPlayer2;
	}

}

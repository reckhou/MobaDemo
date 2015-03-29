using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {
	public GameObject DestIndicator;
	public GameObject Character;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector3 mousePos = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(mousePos);
			RaycastHit[] hitArray = (Physics.RaycastAll(ray));
			if (hitArray.Length > 0) {
				// only the ground is possible to be hit in this case
				Vector3 hitPoint = hitArray[0].point;
				DestIndicator.transform.position = hitPoint;
				Character.GetComponent<CharacterAction>().Move(hitPoint);
			}
		}
	}
}

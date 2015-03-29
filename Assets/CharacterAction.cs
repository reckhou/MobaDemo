using UnityEngine;
using System.Collections;

public class CharacterAction : MonoBehaviour {
	public float RotateSpeed; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Move(Vector3 destination) {
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		agent.destination = destination;
	}
}

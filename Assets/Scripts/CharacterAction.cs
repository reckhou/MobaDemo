﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAction : MonoBehaviour {
	public enum Status {
		Running,
		Rotating,
		Idle
	};

	Status stat;
	Vector3 moveDestination;
	Vector3 moveVector;
	Quaternion destRotation;
	NavMeshAgent agent;

	public float RotateSpeed;
    public float Accleration;
	public float BrakeDistance;
  
	public List<Vector3> TestPointList;
	int CurTestPoint;
	public bool TestCaseEnabled;

	Animator animator;

	// Use this for initialization
	void Start () {
		stat = Status.Idle;
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();

		// override agent position
		agent.updatePosition = false;

		if (TestCaseEnabled) {
			RunTestCase();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.T)) {
			TestCaseEnabled = !TestCaseEnabled;
		}

		if (stat == Status.Rotating) {
			DoRotation();
		} else if (stat == Status.Running) {
			float curSpeed = animator.GetFloat("Forward");
			if ((moveDestination - transform.position).magnitude < 0.1f) {
				// stop character
				stat = Status.Idle;
				GetComponent<Animator>().SetFloat("Forward", 0);
			} else {
				float distanceToDestination = (moveDestination - agent.nextPosition).magnitude;
				if (distanceToDestination < BrakeDistance) {
					// character braking
					float speed = Mathf.Lerp(distanceToDestination / BrakeDistance, 0, Time.deltaTime);
					GetComponent<Animator>().SetFloat("Forward", speed);
				} else if (curSpeed < 1.0f) {
					// character start running
					float speed = Mathf.Lerp(curSpeed, 1.0f, Time.deltaTime * Accleration);
					animator.SetFloat("Forward", speed);
				}
			}
		}

		if (TestCaseEnabled && stat == Status.Idle) {
			RunTestCase();
		}
	}

	public void Move(Vector3 destination) {
		// Optimization: Ignore tiny movement
		if ((destination - transform.position).magnitude < agent.stoppingDistance) {
			return;
		}

		stat = Status.Idle;
		moveDestination = destination;
		Vector3 curPos = transform.position;
		moveVector = destination - curPos;
		Vector3 faceTo = transform.rotation * Vector3.forward;

		// Optimization: Only override nav mesh agent over 60 degrees.
		float rotateDegree = getRotateDegree(faceTo, moveVector);
		if (rotateDegree > 60.0f) {
			agent.enabled = false;
			Rotate(moveVector);
		} else {
			ActivateNavMeshAgent();
		}
	}

	void Rotate(Vector3 rotateTo) {
		stat = Status.Rotating;
		destRotation = Quaternion.LookRotation(moveVector);

		// Optimization: Sometimes an x rotation on eula angles, remove it.
		Vector3 eularAngles = destRotation.eulerAngles;
		eularAngles.x = 0;
		destRotation.eulerAngles = eularAngles;

		// slow down animation when character is rotating.
		GetComponent<Animator>().speed = 0.4f;
	}

	void DoRotation() {
		if (getRotateDegree(transform.rotation * Vector3.forward, moveVector) < 20.0f) {
			transform.eulerAngles = destRotation.eulerAngles;
			GetComponent<Animator>().speed = 1.0f;
			ActivateNavMeshAgent();
			return;
		}

		transform.rotation = Quaternion.Lerp(transform.rotation, destRotation, Time.deltaTime * RotateSpeed);

	}

	void ActivateNavMeshAgent() {
		stat = Status.Running;
		agent.enabled = true;
		agent.destination = moveDestination;
	}

	void OnAnimatorMove() {
		transform.position = agent.nextPosition;
	}

	float getRotateDegree(Vector3 faceTo, Vector3 target) {
		return Mathf.Acos(Vector3.Dot(target.normalized, faceTo.normalized))*Mathf.Rad2Deg;
	}

	void RunTestCase() {
		CurTestPoint++;
		if (CurTestPoint > TestPointList.Count - 1) {
			CurTestPoint = 0;
		}

		Move(TestPointList[CurTestPoint]);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAction : MonoBehaviour {
	public float RotateSpeed;

	public enum Status {
		Running,
		Rotating,
		Idle
	};

	public Status stat;
	public Vector3 moveDestination;
	public Vector3 moveVector;
	public Quaternion destRotation;
	NavMeshAgent agent;

	public List<Vector3> TestPointList;
	public int CurTestPoint;
	public bool TestCaseEnabled;

	// Use this for initialization
	void Start () {
		stat = Status.Idle;
		agent = GetComponent<NavMeshAgent>();
		// override agent position & rotation
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
		} else if ((moveDestination - transform.position).magnitude < 0.1f) {
			stat = Status.Idle;
			GetComponent<Animator>().SetFloat("Forward", 0);
		}

		if (TestCaseEnabled && stat == Status.Idle) {
			RunTestCase();
		}
	}

	public void Move(Vector3 destination) {
		// Optimization: Ignore tiny movement
		if ((destination - transform.position).magnitude < 0.3f) {
			return;
		}

		stat = Status.Idle;
		moveDestination = destination;
		Vector3 curPos = transform.position;
		moveVector = destination - curPos;
		Vector3 faceTo = transform.rotation * Vector3.forward;

		// Optimization: only override nav mesh agent over 90 degrees.
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
		GetComponent<Animator>().StopPlayback();
	}

	void DoRotation() {
		if (getRotateDegree(transform.rotation * Vector3.forward, moveVector) < 20.0f) {
			transform.eulerAngles = destRotation.eulerAngles;
			ActivateNavMeshAgent();
			return;
		}

		transform.rotation = Quaternion.Lerp(transform.rotation, destRotation, Time.deltaTime * RotateSpeed);

	}

	void ActivateNavMeshAgent() {
		stat = Status.Running;
		agent.enabled = true;
		agent.destination = moveDestination;
		GetComponent<Animator>().SetFloat("Forward", 1.0f);
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

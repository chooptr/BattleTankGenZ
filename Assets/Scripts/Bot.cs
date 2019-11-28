using System.Linq;
using UnityEngine;

public class Bot : MonoBehaviour {
	public enum Team {
		Red,
		Blue
	}

	public enum RobotState {
		RandomWalk,
		Chase,
		Attack
	}

	private Transform t;

	public Team team;
	public LayerMask layerMask;
	public NeuralNetwork network;

	private float attackRange = 3f;
	private float rayDistance = 5.0f;
	private float stoppingDistance = 1.5f;

	private Vector3 destination;
	private Quaternion desiredRotation;
	private Vector3 direction;
	private Bot target;
	private RobotState currentState;

	private void Update() {
		switch (currentState) {
			case RobotState.RandomWalk: {
				if (NeedsDestination()) {
					GetDestination();
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 0.2f);

				transform.Translate(1f * Time.deltaTime * Vector3.forward);

				var rayColor = GetObstacles() ? Color.red : Color.green;
				Debug.DrawRay(transform.position, direction * rayDistance, rayColor);

				while (GetObstacles()) {
					GetDestination();
				}

				var ennemy = CheckForEnemy();
				if (ennemy) {
					target = ennemy.GetComponent<Bot>();
					currentState = RobotState.Chase;
				}

				break;
			}
			case RobotState.Chase: {
				var ennemy = CheckForEnemy();
				if (ennemy) {
					currentState = RobotState.RandomWalk;
					return;
				}


				Vector3 direction = target.transform.position - transform.position;
				Quaternion toRotation = Quaternion.FromToRotation(transform.forward, direction);
				transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 0.2f);

				transform.Translate(Vector3.forward * Time.deltaTime * 1f);

				if (Vector3.Distance(transform.position, target.transform.position) < attackRange) {
					currentState = RobotState.Attack;
				}

				break;
			}
			case RobotState.Attack: {
				var ennemy = CheckForEnemy();
				if (ennemy != null) {
					Destroy(target.gameObject);
				}

				currentState = RobotState.RandomWalk;
				break;
			}
		}
	}

	private bool GetObstacles() {
		Ray ray = new Ray(transform.position, direction);
		var hitSomething = Physics.RaycastAll(ray, rayDistance, layerMask);
		return hitSomething.Any();
	}

	private void GetDestination() {
		Vector3 testPosition = (transform.position + (transform.forward * 4f)) +
		                       new Vector3(UnityEngine.Random.Range(-4.5f, 4.5f), 0f,
			                       UnityEngine.Random.Range(-4.5f, 4.5f));

		destination = new Vector3(testPosition.x, 1f, testPosition.z);

		direction = Vector3.Normalize(destination - transform.position);
		direction = new Vector3(direction.x, 0f, direction.z);
		desiredRotation = Quaternion.LookRotation(direction);
	}

	private bool NeedsDestination() {
		if (destination == Vector3.zero)
			return true;

		var distance = Vector3.Distance(transform.position, destination);
		if (distance <= stoppingDistance) {
			return true;
		}

		return false;
	}

	private Transform CheckForEnemy() {
		float enemyRadius = 5f;

		Quaternion startingAngle = Quaternion.AngleAxis(-60, Vector3.up);
		Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);
		Quaternion stepAngle2 = Quaternion.AngleAxis(3, Vector3.up);

		RaycastHit hit;
		var angle = transform.rotation * startingAngle;
		var direction = angle * Vector3.forward;
		var pos = transform.position;

		for (int i = 0; i < 40; i++) {
			Debug.DrawRay(pos, direction * attackRange, new Color(1, 0.655f, 0.482f));
			direction = stepAngle2 * direction;
		}

		direction = angle * Vector3.forward;
		for (var i = 0; i < 24; i++) {
			if (Physics.Raycast(pos, direction, out hit, enemyRadius)) {
				var drone = hit.collider.GetComponent<Bot>();
				if (drone && drone.team != gameObject.GetComponent<Bot>().team) {
					Debug.DrawRay(pos, direction * hit.distance, Color.red);
					return drone.transform;
				}
				else {
					Debug.DrawRay(pos, direction * hit.distance, Color.yellow);
				}
			}
			else {
				Debug.DrawRay(pos, direction * enemyRadius, Color.white);
			}

			direction = stepAngle * direction;
		}

		return null;
	}

	public void UpdateFitness() {
		
	}
}
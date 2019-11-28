using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
	public float timeframe;
	public int populationSize; //creates population size
	public GameObject prefab; //holds bot prefab

	public int[] layers = {5, 3, 2}; //initializing network to the right size

	[Range(0.0001f, 1f)] public float mutationChance = 0.01f;

	[Range(0f, 1f)] public float mutationStrength = 0.5f;

	[Range(0.1f, 10f)] public float gameSpeed = 1f;

	//public List<Bot> Bots;
	public List<NeuralNetwork> networks;
	private List<Bot> cars;

	void Start() {
		if (populationSize % 2 != 0)
			populationSize = 50; //if population size is not even, sets it to fifty

		InitNetworks();
		InvokeRepeating("CreateBots", 0.1f, timeframe); //repeating function
	}

	public void InitNetworks() {
		networks = new List<NeuralNetwork>();
		for (int i = 0; i < populationSize; i++) {
			NeuralNetwork net = new NeuralNetwork(layers);
			net.Load("Assets/Save.txt"); //on start load the network save
			networks.Add(net);
		}
	}

	public void CreateBots() {
		Time.timeScale = gameSpeed; //sets gamespeed, which will increase to speed up training
		if (cars != null) {
			for (int i = 0; i < cars.Count; i++) {
				Destroy(cars[i].gameObject); //if there are Prefabs in the scene this will get rid of them
			}

			SortNetworks(); //this sorts networks and mutates them
		}

		cars = new List<Bot>();
		for (int i = 0; i < populationSize; i++) {
			Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), new Quaternion(0, 0, 1, 0))).GetComponent<Bot>(); //create botes
			car.network = networks[i]; //deploys network to each learner
			cars.Add(car);
		}
	}

	public void SortNetworks() {
		for (int i = 0; i < populationSize; i++) {
			cars[i].UpdateFitness(); //gets bots to set their corrosponding networks fitness
		}

		networks.Sort();
		networks[populationSize - 1].Save("Assets/Save.txt"); //saves networks weights and biases to file, to preserve network performance
		for (int i = 0; i < populationSize / 2; i++) {
			networks[i] = networks[i + populationSize / 2].DeepCopy(new NeuralNetwork(layers));
			networks[i].Mutate((int) (1 / mutationChance), mutationStrength);
		}
	}
}
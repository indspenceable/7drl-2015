using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour {
	public static float StandardDelay = 0.1f;
	public float standardDelay = 0.1f;

	[System.Serializable]
	public struct MapConfig {
		public int totalNumberOfLevels;
		public int width;
		public int height;
		public TileTerrain open;
		public TileTerrain wall;
		public TileTerrain pit;
		public TileTerrain stairs;
		public TileTerrain item;
		public TileTerrain trapped;
		public ItemDefinition[] items;
		public VaultDefinition[] vaults;
	}

	[System.Serializable]
	public struct PrefabConfig {
		public GameObject playerPrefab;
		public GameObject tilePrefab;
		public GameObject reticle;
		public GameObject monster;
		public Monster[] monsterdefs;
	}
		
	[SerializeField]
	GameObject instancePrefab;
	[SerializeField]
	GameInstance instance;
	[SerializeField]
	MapConfig mapConfig;
	[SerializeField] 
	PrefabConfig prefabConfig;

	[SerializeField]
	MainMenu mainMenu;

	// Use this for initialization
	void Start () {
		GameManager.StandardDelay = standardDelay;
		SwapToMainMenu();
	}
	public void SwapToMainMenu() {
		if (instance != null) {
			Destroy(instance.gameObject);
		}
		mainMenu.gameObject.SetActive(true);
		mainMenu.Startup();
	}

	public void Win() {
		Debug.Log("You win!");
		SwapToMainMenu();
	}
	public void StartNewGame() {
		mainMenu.gameObject.SetActive(false);
//		instance = new GameObject("GameInstance").AddComponent<GameInstance>();
		instance = Instantiate(instancePrefab).GetComponent<GameInstance>();
		instance.gameObject.SetActive(true);
		instance.Startup(this, mapConfig, prefabConfig);
	}
		
	public void SaveGameState() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.persistentDataPath + "/save.gd"); //you can call it anything you want
		bf.Serialize(file, instance);
		file.Close();
	}
	public void StartSavedGame() {
		if(File.Exists(Application.persistentDataPath + "/save.gd")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/save.gd", FileMode.Open);
			instance = (GameInstance)bf.Deserialize(file);
			file.Close();
			instance.Continue();
		}
	}
}

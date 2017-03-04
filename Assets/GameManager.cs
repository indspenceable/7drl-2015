using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[System.Serializable]
	public struct MapConfig {
		public int totalNumberOfLevels;
		public int width;
		public int height;
		public TileTerrain open;
		public TileTerrain wall;
		public TileTerrain pit;
	}

	[System.Serializable]
	public struct PrefabConfig {
		public GameObject playerPrefab;
		public GameObject tilePrefab;
	}
		
	GameInstance instance;
	[SerializeField]
	MapConfig mapConfig;
	[SerializeField] 
	PrefabConfig prefabConfig;

	// Use this for initialization
	void Start () {
		instance = new GameObject("GameInstance").AddComponent<GameInstance>();
		instance.Startup(mapConfig, prefabConfig);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

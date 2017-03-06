using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}

	[System.Serializable]
	public struct PrefabConfig {
		public GameObject playerPrefab;
		public GameObject tilePrefab;
		public GameObject reticle;
		public GameObject monster;
		public Monster[] monsterdefs;
	}
		
	GameInstance instance;
	[SerializeField]
	MapConfig mapConfig;
	[SerializeField] 
	PrefabConfig prefabConfig;

	// Use this for initialization
	void Start () {
		GameManager.StandardDelay = standardDelay;
		instance = new GameObject("GameInstance").AddComponent<GameInstance>();
		instance.Startup(mapConfig, prefabConfig);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

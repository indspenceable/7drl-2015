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
	}
		
	GameInstance instance;
	[SerializeField]
	MapConfig mapConfig;


	// Use this for initialization
	void Start () {
		instance = new GameObject("GameInstance").AddComponent<GameInstance>();
		instance.Startup(mapConfig);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

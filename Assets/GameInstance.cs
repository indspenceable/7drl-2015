using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour {
	private LevelMap[] levels;

	private MapTileComponent[][] tiles;

	private int currentLevelNumber;
	private LevelMap CurrentLevel {
		get {
			return levels[currentLevelNumber];
		}
	}

	private Player player;

	public void Startup(GameManager.MapConfig mapConfig, GameManager.PrefabConfig prefabs) {
		// Build the level maps
		levels = new LevelMap[mapConfig.totalNumberOfLevels];
		for (int i = 0; i < mapConfig.totalNumberOfLevels; i += 1) {
			levels[i] = new LevelMap(mapConfig);
		}


		// Now, actually build out the map gameobjects
		tiles = new MapTileComponent[mapConfig.width][];
		for ( int x = 0; x < mapConfig.width; x+=1) {
			tiles[x] = new MapTileComponent[mapConfig.height];
			for (int y = 0; y < mapConfig.height; y +=1) {
				tiles[x][y] = Instantiate(prefabs.tilePrefab).GetComponent<MapTileComponent>();
				tiles[x][y].SetCoords(x,y);
			}
		}

		// Set their terrain
		for ( int x = 0; x < mapConfig.width; x+=1) {
			for (int y = 0; y < mapConfig.height; y +=1) {
				tiles[x][y].SetTerrain(CurrentLevel.GetAt(x,y));
			}
		}

		player = Instantiate(prefabs.playerPrefab).GetComponent<Player>();
		player.SetCoords(1,1);
	}
}

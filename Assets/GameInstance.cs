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

	public void Startup(GameManager.MapConfig config) {
		// Build the level maps
		levels = new LevelMap[config.totalNumberOfLevels];
		for (int i = 0; i < config.totalNumberOfLevels; i += 1) {
			levels[i] = new LevelMap(config);
		}


		// Now, actually build out the map gameobjects
		tiles = new MapTileComponent[config.width][];
		for ( int x = 0; x < config.width; x+=1) {
			tiles[x] = new MapTileComponent[config.height];
			for (int y = 0; y < config.height; y +=1) {
				tiles[x][y] = new GameObject("Tile").AddComponent<MapTileComponent>();
				tiles[x][y].SetCoords(x,y);
			}
		}

		// Set their terrain
		for ( int x = 0; x < config.width; x+=1) {
			for (int y = 0; y < config.height; y +=1) {
				tiles[x][y].SetTerrain(CurrentLevel.GetAt(x,y));
			}
		}
	}
}

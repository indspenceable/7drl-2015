using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelMap {
	private TileTerrain[][] map;
	private string[] premadeMap = new string[]{
		"xxxxxxxxxx",
		"x........x",
		"x........x",
		"x........x",
		"x........x",
		"x........x",
		"x........x",
		"x........x",
		"x........x",
		"xxxxxxxxxx",
	};

	public LevelMap(GameManager.MapConfig defs) {
		GenerateFromPremadeMap(defs);
	}
	public TileTerrain GetAt(int x, int y) {
		return map[x][y];
	}
	private void GenerateFromPremadeMap(GameManager.MapConfig config) {
		// Normally, generate a level here.
		map = new TileTerrain[config.width][];
		for (int i = 0; i < config.width; i+=1) {
			map[i] = new TileTerrain[config.height];
			for (int j = 0; j < config.height; j += 1) {
				map[i][j] = TileTerrainForChar(config, premadeMap[i][j]);
			}
		}
	}
	private TileTerrain TileTerrainForChar(GameManager.MapConfig defs, char c) {
		if (c == 'x') {
			return defs.wall;
		} else if (c == '.') {
			return defs.open;
		} else {
			Debug.LogError("Unexpected Character - no terrain defintion for [" + c + "]");
			return null;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelMap {
	private TileTerrain[][] map;
	private string[] premadeMap = new string[]{
		"xxxxxxxxxxxxxxxxxxxx",
		"x..................x",
		"x..................x",
		"x..................x",
		"x..................x",
		"x...~~~~~~~~~......x",
		"x..................x",
		"x...........x......x",
		"x...........x......x",
		"x...........x......x",
		"x...........x.>....x",
		"x...........xxxxx.xx",
		"x...........x......x",
		"x.~~~.......x......x",
		"x....~~.....x....~.x",
		"x..~~.......x......x",
		"x..~........x......x",
		"x...~.......x......x",
		"x..~........x......x",
		"xxxxxxxxxxxxxxxxxxxx",
	};

	public LevelMap(GameManager.MapConfig defs) {
		GenerateFromPremadeMap(defs);
	}
	public TileTerrain GetAt(Coord c) {
		return map[c.x][c.y];
	}
	private void GenerateFromPremadeMap(GameManager.MapConfig config) {
		// Normally, generate a level here.
		map = new TileTerrain[config.width][];
		for (int i = 0; i < config.width; i+=1) {
			map[i] = new TileTerrain[config.height];
			for (int j = 0; j < config.height; j += 1) {
				map[i][j] = TileTerrainForChar(config, premadeMap[config.height-j-1][i]);
			}
		}
	}
	private TileTerrain TileTerrainForChar(GameManager.MapConfig defs, char c) {
		if (c == 'x') {
			return defs.wall;
		} else if (c == '.') {
			return defs.open;
		} else if (c == '~') {
			return defs.pit;
		} else if (c == '>') {
			return defs.stairs;
		} else {
			Debug.LogError("Unexpected Character - no terrain defintion for [" + c + "]");
			return null;
		}
	}
}
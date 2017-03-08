using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[System.Serializable]
public class LevelMap {
	private TileTerrain[][] map;
	private string[] premadeMap = new string[]{
		"xxxxxxxxxxxxxxxxxxx",
		"x.................x",
		"x........+........x",
		"x.................x",
		"x.................x",
		"xx^x~~~~~~~~~.....x",
		"x.................x",
		"x.......^...x.....x",
		"x...^.......x.....x",
		"x.....^.....x.....x",
		"x...........x.>...x",
		"x...........xxxxx.x",
		"x...........x.....x",
		"x.~~~.......x.....x",
		"x....~~.....x....~x",
		"x.+~~.......x.....x",
		"x..~........x.....x",
		"x.>.~.......x.....x",
		"x.+~........x.....x",
		"xxxxxxxxxxxxxxxxxxx",
	};

	public LevelMap(GameManager.MapConfig config) {
		GenerateFromStringMap(config, GenerateNewMap(config, config.vaults[0], config.vaults));
	}
	public TileTerrain GetAt(Coord c) {
		return map[c.x][c.y];
	}
	private void GenerateFromStringMap(GameManager.MapConfig config, string[] stringMap) {
		map = new TileTerrain[config.width][];
		for (int i = 0; i < config.width; i+=1) {
			map[i] = new TileTerrain[config.height];
			for (int j = 0; j < config.height; j += 1) {
				map[i][j] = TileTerrainForChar(config, stringMap[config.height-j-1][i]);
			}
		}
	}

	private string[] GenerateNewMap(GameManager.MapConfig config, VaultDefinition entry, VaultDefinition[] vaults) {
		StringBuilder[] tempMap = new StringBuilder[config.height];
		for (int i = 0; i < config.height; i+=1) {
			tempMap[i] = new StringBuilder(new string(' ', config.width));
		}
		// Place the outerwalls
		for (int x = 0; x < config.width; x+=1) {
			for (int y = 0; y < config.height; y+=1) {
				if (x == 0 || y == 0 || x == config.width-1 || y == config.height-1) {
					tempMap[y][x] = 'x';
				}
			}
		}
	

		AddVault(tempMap, entry.Process()[0], new Coord(2,2));
		// Move floor size out of here
		int totalTries = 0;
		for (int i = 0; i < 200; i+=1) {
			Debug.Log(i);
			VaultDefinition v = vaults[Random.Range(0, vaults.Length)];
//			VaultDefinition v = vaults[0];
			AttemptToPlaceAndAdd(config, tempMap, v);
		}
		foreach(Coord c in FindPossibleOutlets(config, tempMap)) {
			tempMap[c.y][c.x] = '~';
		}
		string[] rtn = new string[config.height];
		for (int i = 0; i < config.height; i+=1) {
			rtn[i] = tempMap[i].ToString().Replace(' ', 'x');
		}
		return rtn;
	}
	private bool AttemptToPlaceAndAdd(GameManager.MapConfig config, StringBuilder[] tempMap, VaultDefinition vd) {
		foreach (Vault v in vd.Process()) {
			Debug.Log("ya");
			List<Coord> possibleOutlets = FindPossibleOutlets(config, tempMap);
			List<Coord> vaultOutlets = FindAttachPointInVault(v);
			foreach (Coord p1 in possibleOutlets) {
				Debug.Log("bleh");
				foreach(Coord p2 in vaultOutlets) {
					Debug.Log("neh");
					Coord placeVaultAt= new Coord(p1.x - p2.x, p1.y - p2.y);

					if (CheckVault(tempMap, v, placeVaultAt)) {
						AddVault(tempMap, v, placeVaultAt);
						tempMap[p1.y][p1.x] = '.';
						return true;
					}
				}
			}
		}
		return false;
	}

	bool CheckVault(StringBuilder[] tempMap, Vault v, Coord pos) {
		for (int x = 0; x < v.size.x; x += 1) {
			for (int y = 0; y < v.size.y; y += 1) {
				Coord c = pos + new Coord(x, y);
				if (c.x < 0 || c.y < 0 || c.x >= tempMap[0].Length || c.y >= tempMap.Length) {
					return false;
				}
				if (tempMap[c.y][c.x] != ' ') {
					if (tempMap[c.y][c.x] == '@' && v.At(x,y) == '?')
					{
						// Nada
					} else if (tempMap[c.y][c.x] != v.At(x,y)) {
						return false;
					}
				}
			}
		}
		return true;
	}

	private void AddVault(StringBuilder[] m, Vault v, Coord pos) {
		for (int x = 0; x < v.size.x; x += 1) {
			for (int y = 0; y < v.size.y; y += 1) {
//				Debug.Log(pos);
//				Debug.Log("Setting at " + (pos.x + x) + ", " + (pos.y + y));
//				Debug.Log("Setting to " + v.At(x,y));
				m[pos.y+y][pos.x+x] = v.At(x,y);
			}
		}
	}
	private List<Coord> FindPossibleOutlets(GameManager.MapConfig config, StringBuilder[] m) {
		List<Coord> possibleOutlets = new List<Coord>();
		for (int x = 0; x < config.width; x += 1) {
			for (int y = 0; y < config.height; y += 1) {
				if (m[y][x] == '@') {
					possibleOutlets.Add(new Coord(x,y));
				}
			}
		}
		return possibleOutlets;
	}

	private List<Coord> FindAttachPointInVault(Vault v) {
		List<Coord> possibleOutlets = new List<Coord>();
		for (int x = 0; x < v.size.x; x += 1) {
			for (int y = 0; y < v.size.y; y += 1) {
				if (v.At(x,y) == '?') {
					possibleOutlets.Add(new Coord(x,y));
				}
			}
		}
		return possibleOutlets;
	}

	private TileTerrain TileTerrainForChar(GameManager.MapConfig defs, char c) {
		if (c == 'x') {
			return defs.wall;
		} else if (c == '.' || c=='<') {
			return defs.open;
		} else if (c == '~') {
			return defs.pit;
		} else if (c == '>') {
			return defs.stairs;
		} else if (c == '+') {
			return defs.item;
		} else if (c == '^') {
			return defs.trapped;
		} else {
			Debug.LogError("Unexpected Character - no terrain defintion for [" + c + "]");
			return null;
		}
	}
}
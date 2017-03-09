using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

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
	private string[] myMap;
	public Coroutine GenerateCallback;
	public Coord startPos = new Coord(0,0);
	public List<Coord> MonsterPositions = new List<Coord>();
	int difficulty;

	public IEnumerator Generate(int difficulty, GameManager.MapConfig config) {
		this.difficulty = difficulty;
		yield return GenerateNewMapAndSave(config, config.vaults[0], config.vaults);
		GenerateFromStringMap(config, myMap);
		yield return Cleanup(config);
		yield return PlaceFeatures(config);
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

	private IEnumerator Cleanup(GameManager.MapConfig config) {
		bool requiresAdditionalSweep = true;
		while (requiresAdditionalSweep) {
//			Debug.Log("Starting sweep.");
			requiresAdditionalSweep = false;
			List<Coord> newWalls = new List<Coord>();
			List<Coord> newOpens = new List<Coord>();
			for (int x = 0; x < config.width; x+=1) {
				for (int y = 0; y < config.height; y += 1) {
					int surroundingWallsCardinal = 0;
					int surroundingWallsAll = 0;
					for (int dx = -1; dx <=1; dx +=1 ) {
						for (int dy = -1; dy <=1; dy +=1 ) {
							// In bounds + Walls
							if (InBounds(x+dx, y+dy, config) && map[x+dx][y+dy] == config.wall) {
								if (Cardinal(dx, dy)) {
									surroundingWallsCardinal += 1;
								}
								surroundingWallsAll += 1;
							}
						}
					}
					if (surroundingWallsCardinal >= 3 && map[x][y] == config.open){ 
						newWalls.Add(new Coord(x,y));
					}
					if (surroundingWallsCardinal == 1 && map[x][y] == config.wall){ 
						newOpens.Add(new Coord(x,y));
					}
				}
			}
			foreach (Coord c in newWalls) {
				requiresAdditionalSweep = true;
				map[c.x][c.y] = config.wall;
			}

			foreach (Coord c in newOpens) {
				requiresAdditionalSweep = true;
				map[c.x][c.y] = config.open;
			}
			yield return null;
		}
	}
	private bool Cardinal(int x, int y) {
		return (x == 0) ^ (y == 0);
	}
	private bool InBounds(int x, int y, GameManager.MapConfig config) {
		return !(x < 0 || y < 0 || x >= config.width || y  >= config.height);
	}

	Coord SelectRandomOpenCoord(GameManager.MapConfig config) {
		Coord c = new Coord(0,0);
		while (map[c.x][c.y] != config.open) {
			c = new Coord(Random.Range(0, config.width), Random.Range(0, config.height));
		}
		return c;
	}

	private IEnumerator PlaceFeatures(GameManager.MapConfig config) {
		yield return null;
		DjikstraMap dm = new DjikstraMap(config.width, config.height);
		Coord startCoord = SelectRandomOpenCoord(config);
		List<Coord> targetPositions = new List<Coord>();
		targetPositions.Add(startCoord);

		bool first = true;
		// 3 monsters on first level, +1 each level
		// 3 additional features (stairs + item canister) per level
		while (targetPositions.Count < difficulty + 3 + 3) {
			foreach(Coord c in targetPositions) {
				yield return null;
//				Debug.Log("added a goal.");
				dm.SetGoal(c);
			}
				
			dm.Calculate((c,e) => map[c.x][c.y] == config.open, null);
			targetPositions.Add(dm.FindWorstPoint());
			yield return null;

			if (first) {
				targetPositions.RemoveAt(0);
				first = false;
			}
		}
			
		int i = 0;
		foreach (Coord c in targetPositions) {
			switch(i){
			case 0:
				startPos = c;
				break;
			case 1:	
				map[c.x][c.y] = config.stairs;
				break;
			case 2:
				map[c.x][c.y] = config.item;
				break;
			default:
				MonsterPositions.Add(c);
				break;
			}
			i+=1;
		}
	}

	private IEnumerator GenerateNewMapAndSave(GameManager.MapConfig config, VaultDefinition entry, VaultDefinition[] vaults) {
		int vaultCount = 0;
		StringBuilder[] tempMap = new StringBuilder[config.height];
		while (vaultCount < 5) {
			
			tempMap = new StringBuilder[config.height];
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
		
			Coord c1 = new Coord(-5, -5);
			Vault v1 = vaults[Random.Range(0,vaults.Length)].Process()[Random.Range(0,2)];
			int tries = 0;
			while (!CheckVault(tempMap, v1, c1) && tries < 100) {
				if (tries%10 == 0) yield return null;
				c1 = new Coord(Random.Range(0, 7), Random.Range(0, 7));
				tries += 1;
			}
			yield return null;
			if (tries >= 100) continue;
			AddVault(tempMap, v1, c1);
			// Move floor size out of here

			vaultCount = 0;
			for (int i = 0; i < 200; i+=1) {
//				Debug.Log("Trying to place a vault...");
				yield return null;
				VaultDefinition v = vaults[Random.Range(0, vaults.Length)];
				if (AttemptToPlaceAndAdd(config, tempMap, v)) {
					vaultCount += 1;
				}
			}
//			Debug.Log("Placed: " + vaultCount);
		}

		foreach(Coord c in FindPossibleOutlets(config, tempMap)) {
			tempMap[c.y][c.x] = 'x';
		}
		string[] rtn = new string[config.height];
		for (int i = 0; i < config.height; i+=1) {
			rtn[i] = tempMap[i].ToString().Replace(' ', 'x');
			rtn[i] = rtn[i].ToString().Replace('?', 'x');
		}
		myMap = rtn;
	}
	private bool AttemptToPlaceAndAdd(GameManager.MapConfig config, StringBuilder[] tempMap, VaultDefinition vd) {
		foreach (Vault v in vd.Process()) {
			List<Coord> possibleOutlets = FindPossibleOutlets(config, tempMap);
			List<Coord> vaultOutlets = FindAttachPointInVault(v);
			foreach (Coord p1 in possibleOutlets) {
				foreach(Coord p2 in vaultOutlets) {
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
		} else if (c == '.' || c== '<') {
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
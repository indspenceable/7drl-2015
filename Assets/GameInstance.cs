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
	private GameObject targettingReticle;

	private List<MonsterComponent> monsters = new List<MonsterComponent>();

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

		// Set up the player
		player = Instantiate(prefabs.playerPrefab).GetComponent<Player>();
		player.SetCoords(1,1);

		// Aaaand the reticle
		targettingReticle = Instantiate(prefabs.reticle);
		targettingReticle.SetActive(false);

		// Fill in the monsters for this level
		for (int i = 0; i < 3; i+=1) {
			int x = 0; int y = 0;

			while (CurrentLevel.GetAt(x,y).blocked || !CurrentLevel.GetAt(x,y).passable || (x == 1 && y == 1)) {
				x = Random.Range(0, 10);
				y = Random.Range(0, 10);
			}
			Monster monsterType = prefabs.monsterdefs[Random.Range(0, prefabs.monsterdefs.Length)];
			MonsterComponent mc = Instantiate(prefabs.monster).GetComponent<MonsterComponent>();
			mc.Setup(monsterType, x, y);
			monsters.Add(mc);
		}
//
		// Now that we have all this junk established, we can begin listening to input.
		StartCoroutine(ListenForPlayerInput());
	}


	private IEnumerator ListenForPlayerInput() {
		yield return null;
		if (Input.GetKeyDown(KeyCode.A)) {
			yield return AttemptMove(-1, 0);
		} else if (Input.GetKeyDown(KeyCode.D)) {
			yield return AttemptMove(1, 0);
		} else if (Input.GetKeyDown(KeyCode.S)) {
			yield return AttemptMove(0, -1);
		} else if (Input.GetKeyDown(KeyCode.W)) {
			yield return AttemptMove(0, 1);
		} else if (Input.GetKeyDown(KeyCode.I)) { 
			yield return SelectTarget(KeyCode.I, (dx, dy) => AttemptMove(dx-player.pos.x, dy-player.pos.y));
		} else if (Input.GetKeyDown(KeyCode.K)) { 
			yield return SelectCardinalDirection(KeyCode.K, HookInDirection);
		} else {
			yield return ListenForPlayerInput();
		}
	}

	private IEnumerator AttemptMove(int dx, int dy) {
		Coord dest = player.pos + new Coord(dx, dy);
		if (tiles[dest.x][dest.y].interactable) {
		} else if (tiles[dest.x][dest.y].passable) {
			yield return SlowMove(player.gameObject, new Vector3(player.pos.x+dx, player.pos.y + dy), 0.1f);
			player.SetCoords(dest.x, dest.y);
			yield return TakeAllMonstersTurn();
		} else {
			yield return ListenForPlayerInput();
		}
	}
	/* ------------------------------- *
	 * Monster related stuff goes here *
 	 * ------------------------------- */


	private int Comp(Coord a, Coord b) {
		int ad = a.DistanceTo(player.pos);
		int bd = b.DistanceTo(player.pos);
		if (ad < bd) {
			return -1;
		} else if (bd < ad) {
			return 1;
		} else {
			return 0;
		}
	}
	// Find a path from the monster to the player
	public List<Coord> AStarMonsterToPlayer(MonsterComponent m) {
		List<Coord> openList = new List<Coord>{ m.pos };
		Dictionary<Coord, List<Coord>> closedList = new Dictionary<Coord, List<Coord>> {{ m.pos, new List<Coord>() }};
		// 
		while (openList.Count > 0) {
			openList.Sort(Comp);
			Coord current = openList[0];
			openList.RemoveAt(0);
			List<Coord> currentPath = closedList[current];
		
			if (current.Equals(player.pos)) {
				return currentPath;
			}

			Coord[] offsets = new Coord[] {
				new Coord(-1, 0),
				new Coord( 1, 0),
				new Coord( 0, 1),
				new Coord( 0,-1)
			};
			foreach (Coord offset in offsets) {
				Coord c = current + offset;
				if(!closedList.ContainsKey(c) && CurrentLevel.GetAt(c.x, c.y).passable) {
					List<Coord> newPath = new List<Coord>(currentPath);
					newPath.Add(c);
					closedList[c] = newPath;
					openList.Add(c);
				}
			}
		}

		return null;
	}

	private IEnumerator TakeMonsterTurn(MonsterComponent m) {
		List<Coord> path = AStarMonsterToPlayer(m);
		if (path == null || path.Count == 0) {
			// do nothing - no way to approach player
		} else {
			yield return SlowMove(m.gameObject, path[0].toVec(), 0.1f);
			m.pos = path[0];
		}
	}

	private IEnumerator TakeAllMonstersTurn() {
		foreach( MonsterComponent m in monsters) {
			yield return TakeMonsterTurn(m);
		}

		yield return TakePassivesTurn();
	}

	private IEnumerator TakePassivesTurn() {
		yield return ListenForPlayerInput();
	}

	private IEnumerator SlowMove(GameObject go, Vector3 endPosition, float time) {
		float dt = 0;
		Vector3 startPosition = go.transform.position;
		while (dt < time) {
			yield return null;
			dt += Time.deltaTime;
			go.transform.position = Vector3.Lerp(startPosition, endPosition, dt/time);
		}
		go.transform.position = endPosition;
	}

	delegate IEnumerator TargettedAction(int x, int y);
	private IEnumerator SelectTarget(KeyCode selectKeyCode, TargettedAction callback) {
		yield return null;
		targettingReticle.transform.position = player.transform.position;
		// TODO these should probably be provided.
		int x = (int)player.transform.position.x;
		int y = (int)player.transform.position.y;
		targettingReticle.SetActive(true);
		while (true) {
			if (Input.GetKeyDown(KeyCode.W)) {
				y += 1;
				yield return SlowMove(targettingReticle, new Vector3(x, y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.A)) {
				x -= 1;
				yield return SlowMove(targettingReticle, new Vector3(x, y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.S)) {
				y -= 1;
				yield return SlowMove(targettingReticle, new Vector3(x, y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.D)) {
				x += 1;
				yield return SlowMove(targettingReticle, new Vector3(x, y), 0.1f);
			} else if (Input.GetKeyDown(selectKeyCode)) {
				break;
			}
			yield return null;
		}
		targettingReticle.SetActive(false);
		yield return callback(x, y);
	}

	private IEnumerator SelectCardinalDirection(KeyCode selectKeyCode, TargettedAction callback) {
		yield return null;
		targettingReticle.transform.position = player.transform.position;
		// TODO these should probably be provided.
		int oX = (int)player.transform.position.x;
		int oY = (int)player.transform.position.y;
		targettingReticle.SetActive(true);
		int x = 0;
		int y = -1;
		while (true) {
			if (Input.GetKeyDown(KeyCode.W)) {
				x = 0;
				y = 1;
				yield return SlowMove(targettingReticle, new Vector3(oX+x, oY+y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.A)) {
				x = -1;
				y = 0;
				yield return SlowMove(targettingReticle, new Vector3(oX+x, oY+y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.S)) {
				x = 0;
				y = -1;
				yield return SlowMove(targettingReticle, new Vector3(oX+x, oY+y), 0.1f);
			} else if (Input.GetKeyDown(KeyCode.D)) {
				x = 1;
				y = 0;
				yield return SlowMove(targettingReticle, new Vector3(oX+x, oY+y), 0.1f);
			} else if (Input.GetKeyDown(selectKeyCode)) {
				break;
			}
			yield return null;
		}
		targettingReticle.SetActive(false);
		yield return callback(x, y);
	}

	private IEnumerator HookInDirection(int x, int y) {
		int cX = player.pos.x;
		int cY = player.pos.y;
		while (!CurrentLevel.GetAt(cX,cY).blocked) {
			cX += x;
			cY += y;
		}
		cX -= x;
		cY -= y;
		yield return AttemptMove(cX-player.pos.x,cY-player.pos.y);
	}
}

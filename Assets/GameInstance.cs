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
			yield return SelectTarget(AttemptMove);
		} else {
			yield return ListenForPlayerInput();
		}
	}

	private IEnumerator AttemptMove(int dx, int dy) {
		if (tiles[player.x+dx][player.y+dy].passable) {
			yield return SlowMove(player.gameObject, new Vector3(player.x+dx, player.y + dy), 0.1f);
			player.SetCoords(player.x+dx, player.y + dy);
			yield return TakeBaddiesTurn();
		} else {
			yield return ListenForPlayerInput();
		}
	}

	private IEnumerator TakeBaddiesTurn() {
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
	private IEnumerator SelectTarget(TargettedAction callback) {
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
			} else if (Input.GetKeyDown(KeyCode.I)) {
				break;
			}
			yield return null;
		}
		targettingReticle.SetActive(false);
		yield return callback(x-player.x, y-player.y);
	}
}

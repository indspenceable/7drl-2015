using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameInstance : MonoBehaviour {

	private LevelMap[] levels;

	private MapTileComponent[][] tiles;
	public MapTileComponent GetTile(Coord c) {
		return tiles[c.x][c.y];
	}

	private int currentLevelIndex;
	private LevelMap CurrentLevel {
		get {
			return levels[currentLevelIndex];
		}
	}

	public Player player;
	private GameObject targettingReticle;

	private List<MonsterComponent> monsters = new List<MonsterComponent>();

	private GameManager.MapConfig mapConfig;
	private GameManager.PrefabConfig prefabConfig;
	private GameManager manager;

	// UI Related things:
	[SerializeField]
	private HealthMeterUI healthMeter;
	[SerializeField]
	private GearUI[] gearUIs;
	[SerializeField]
	private GameObject LevelGeneratingOverlay;
	[SerializeField]
	private GameObject statusArea;
	[SerializeField]
	private Text statusAreaText;

	[SerializeField]
	private GameObject overlayArea;
	[SerializeField]
	private Text overlayText;

	private void SetInstructions(string status) {
		statusAreaText.text = status;
		statusArea.SetActive(status != "");
	}

	private void SetOverlay(string status) {
		overlayText.text = status;
		overlayArea.SetActive(status != "");
	}

	public void Update() {
		if (!LevelGeneratingOverlay.activeSelf) {
			return;
		}
		foreach (MonsterComponent mc in monsters) {
			if (mc.hover) {
				SetOverlay(mc.DisplayName() + "\n\n" + mc.Desc());
				return;
			}
		}
		foreach(GearUI gui in gearUIs) {
			if (gui.hover) {
				SetOverlay(gui.ItemName() + "\n\n" + gui.ItemDesc());
				return;
			}
		}
		for ( int x = 0; x < mapConfig.width; x+=1) {
			for (int y = 0; y < mapConfig.height; y +=1) {
				MapTileComponent t = GetTile(new Coord(x, y));
				if (t.hover) {
					if (t.ShouldExplain()) {
						SetOverlay(t.Explain());
						return;
					}
				}
			}
		}
		SetOverlay("");
	}

	public IEnumerator Startup(GameManager manager, GameManager.MapConfig mapConfig, GameManager.PrefabConfig prefabs) {
		// Save the map config for use with making Djikstra maps
		this.mapConfig = mapConfig;
		// Save prefabs for future levels;
		this.prefabConfig = prefabs;
		// Save the manager so that we can transition back to the main menu when we win/lose
		this.manager = manager;
		SetInstructions("");

		// Build the level maps
		levels = new LevelMap[mapConfig.totalNumberOfLevels];
		for (int i = 0; i < mapConfig.totalNumberOfLevels; i += 1) {
			levels[i] = new LevelMap();
			levels[i].GenerateCallback = StartCoroutine(levels[i].Generate(i, mapConfig));
		}


		// Now, actually build out the map gameobjects
		tiles = new MapTileComponent[mapConfig.width][];
		GameObject tileContainer = new GameObject();
		tileContainer.transform.parent = transform;
		for ( int x = 0; x < mapConfig.width; x+=1) {
			tiles[x] = new MapTileComponent[mapConfig.height];
			for (int y = 0; y < mapConfig.height; y +=1) {
				tiles[x][y] = Instantiate(prefabs.tilePrefab, tileContainer.transform).GetComponent<MapTileComponent>();
				tiles[x][y].SetCoords(x,y);
			}
		}
			
		// Set up the player
		player = Instantiate(prefabs.playerPrefab, transform).GetComponent<Player>();
		healthMeter.InstallPlayer(player);
		foreach(GearUI gui in gearUIs) {
			gui.InstallPlayer(player);
		}

		yield return SetTerrain();
		player.SetCoords(CurrentLevel.startPos);

		// Aaaand the reticle
		targettingReticle = Instantiate(prefabs.reticle, transform);
		targettingReticle.SetActive(false);

		PopulateMonsters(prefabs);
//
		// Now that we have all this junk established, we can begin listening to input.
		yield return PreTurn();
	}

	private IEnumerator SetTerrain() {
		LevelGeneratingOverlay.SetActive(true);
		yield return CurrentLevel.GenerateCallback;
		LevelGeneratingOverlay.SetActive(false);
		// Set their terrain
		for ( int x = 0; x < mapConfig.width; x+=1) {
			for (int y = 0; y < mapConfig.height; y +=1) {
				tiles[x][y].SetTerrain(CurrentLevel.GetAt(new Coord(x,y)));
				if (tiles[x][y].interaction == TileTerrain.Interaction.GIVE_ITEM) {
					tiles[x][y].item = mapConfig.items[Random.Range(0, mapConfig.items.Length)];
				}
			}
		}
	}

	private void PopulateMonsters(GameManager.PrefabConfig prefabs) {
		foreach (MonsterComponent m in monsters) {
			Destroy(m.gameObject);
		}
		monsters.Clear();
		// Fill in the monsters for this level
//		for (int i = 0; i < 5; i+=1) {
//			Coord c = new Coord(0,0);
//
//			while (!EmptyAndPassable(c) || !Pathable(c) ) {
//				c = new Coord(Random.Range(0, mapConfig.width), Random.Range(0, mapConfig.height));
//			}
		foreach (Coord c in CurrentLevel.MonsterPositions) {
			Monster monsterType = SelectMonsterForCurrentFloor(prefabs);
//			Monster monsterType = prefabs.monsterdefs[3];
			MonsterComponent mc = Instantiate(prefabs.monster, transform).GetComponent<MonsterComponent>();
			mc.Setup(monsterType, c);
			monsters.Add(mc);
		}
	}

	public Monster SelectMonsterForCurrentFloor(GameManager.PrefabConfig prefabs) {
		while (true) {
			Monster m = prefabs.monsterdefs[Random.Range(0, prefabs.monsterdefs.Length)];
			if (m.minimumFloor <= currentLevelIndex) {
				return m;
			}
		}
	}

	public void Continue() {
		StartCoroutine(ListenForPlayerInput());
	}

	private IEnumerator ListenForPlayerInput() {
		yield return null;
		if (Input.GetKeyDown(KeyCode.A)) {
			yield return AttemptMove(new Coord(-1, 0) + player.pos, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.D)) {
			yield return AttemptMove(new Coord(1, 0) + player.pos, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.S)) {
			yield return AttemptMove(new Coord(0, -1) + player.pos, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.W)) {
			yield return AttemptMove(new Coord(0, 1) + player.pos, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.I)) { 
			yield return player.GetItem(0).BeginActivation(this, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.J)) { 
			yield return player.GetItem(1).BeginActivation(this, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.K)) {
			yield return player.GetItem(2).BeginActivation(this, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.L)) {
			yield return player.GetItem(3).BeginActivation(this, TakeAllMonstersTurn(), ListenForPlayerInput());
		} else if (Input.GetKeyDown(KeyCode.Period)) {
			yield return TakeAllMonstersTurn();
		} else {
			yield return ListenForPlayerInput();
		}
	}


	public bool InBounds(Coord c) {
		return c.x >= 0 && c.y >= 0 && c.x < mapConfig.width && c.y < mapConfig.height;
	}

	public IEnumerator AttemptMove(Coord dest, IEnumerator success, IEnumerator cancel) {
//		Coord dest = player.pos + new Coord(dx, dy);
		if (GetTile(dest).interaction != TileTerrain.Interaction.NONE) {
			yield return Interact(dest, success, cancel);
			yield return TakeAllMonstersTurn();
		} else if (GetTile(dest).passable && GetEntityAt(dest) == null) {
			yield return SlowMove(player.gameObject, dest, GameManager.StandardDelay);
			player.SetCoords(dest);
			yield return success;
		} else {
			yield return cancel;
		}
	}

	public IEnumerator SelectItemToReplace(System.Action<int> callback, IEnumerator success, IEnumerator back) {
		
		while (true){
			yield return null;
			if (Input.GetKeyDown(KeyCode.Escape)) {
				SetInstructions("");
				yield return back;
				yield break;
			} else if (Input.GetKeyDown(KeyCode.I)) {
				callback(0);
				yield return success;
				yield break;
			} else if (Input.GetKeyDown(KeyCode.J)) {
				callback(1);
				yield return success;
				yield break;
			} else if (Input.GetKeyDown(KeyCode.K)) {
				callback(2);
				yield return success;
				yield break;
			} else if (Input.GetKeyDown(KeyCode.L)) {
				callback(3);

				yield return success;
				yield break;
			}
		}
	}

	public IEnumerator Interact(Coord dest, IEnumerator success, IEnumerator back) {
		
		TileTerrain.Interaction type = GetTile(dest).interaction;
		switch(type) {
		case TileTerrain.Interaction.NEXT_LEVEL:
			yield return MoveToNextLevel();
			yield return success;
			yield break;
		case TileTerrain.Interaction.GIVE_ITEM:
			SetInstructions("Select item to replace with " + GetTile(dest).item.displayName);
			yield return SelectItemToReplace(
					(i)=>{
						SetInstructions("");
						player.SetItem(i, GetTile(dest).item);
						GetTile(dest).SetTerrain(mapConfig.open);
					},
				success, back);
			yield break;
		case TileTerrain.Interaction.NONE:
		default:
			throw new UnityException("Bad interaction." + type);
		}
	}
	public IEnumerator MoveToNextLevel() {
		this.currentLevelIndex += 1;

		if (this.currentLevelIndex == mapConfig.totalNumberOfLevels) {
			manager.Win();
		} else {
			yield return SetTerrain();
			player.SetCoords(CurrentLevel.startPos);
			PopulateMonsters(prefabConfig);
			yield return PreTurn();
		}
	}
	/* ------------------------------- *
	 * Monster related stuff goes here *
 	 * ------------------------------- */
	public Entity GetEntityAt(Coord a) {
		if (a.Equals(player.pos)) {
			return player;
		}
		foreach(MonsterComponent m in monsters) {
			if (m.pos.Equals(a)) {
				return m;
			}
		}
		return null;
	}

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
				if(!closedList.ContainsKey(c) && GetTile(c).passable) {
					List<Coord> newPath = new List<Coord>(currentPath);
					newPath.Add(c);
					closedList[c] = newPath;
					openList.Add(c);
				}
			}
		}

		return null;
	}

	public void ApplyLabels(DjikstraMap map) {
		for ( int x = 0; x < mapConfig.width; x+=1) {
			for (int y = 0; y < mapConfig.height; y +=1) {
				tiles[x][y].name = map.Value(x,y).ToString();
			}
		}
	}

	private IEnumerator TakeMonsterTurn(MonsterComponent m) {
		return m.ExecuteStrategy(this);
	}

//	public bool PassableAndNoEntity(Coord c) {
//		return CurrentLevel.GetAt(c).passable && GetEntityAt(c) == null;
//	}
	public bool Pathable(Coord c, Entity e = null) {
		return GetTile(c).passable && !GetTile(c).ShouldAvoid(e);
	}
	public DjikstraMap BuildPlayerMap(Entity forWhom) {
		return BuildTargettedMap(forWhom, player.pos);
	}
		
	public DjikstraMap BuildTargettedMap(Entity forWhom, params Coord[] targets) {
		DjikstraMap map = new DjikstraMap(mapConfig.width, mapConfig.height);
		foreach (Coord c in targets) {
			map.SetGoal(c);
		}
		map.Calculate(Pathable, forWhom);
		ApplyLabels(map);
		return map;
	}

	public List<MonsterComponent> GetMonsters() {
		return new List<MonsterComponent>(monsters);
	}
	public void PruneDeadMonsters() {
		List<MonsterComponent> deadMonsters = monsters.FindAll( m => m.IsDead() );
		foreach( MonsterComponent m in deadMonsters) {
			monsters.Remove(m);
			Destroy(m.gameObject);
		}
	}
	public IEnumerator TakeAllMonstersTurn() {
		PruneDeadMonsters();
		foreach( MonsterComponent m in monsters) {
			yield return TakeMonsterTurn(m);
		}
		yield return TakePassivesTurn();
	}

	private IEnumerator TakePassivesTurn() {
		List<Coroutine> cos = new List<Coroutine>();
		for ( int x = 0; x < mapConfig.width; x+=1) {
			for (int y = 0; y < mapConfig.height; y +=1) {
				MapTileComponent t = this.GetTile(new Coord(x,y));
				if (t.HasPassive()) {
					// This is probably super hacky, but start separate coroutines for all of the tiles.
					cos.Add(StartCoroutine(t.Passive(this)));
				}
			}
		}
		foreach(Coroutine co in cos) {
			yield return co;
		}
		PruneDeadMonsters();
		yield return PreTurn();
	}

	private IEnumerator PreTurn() {
//		Debug.Log("Preturn!");
		// Save the game here
		// Unfortunately, saving doesn't work, and is unlikely to in this week.
		// manager.SaveGameState();
		if (player.IsDead()) {
			Debug.Log("Dead!!!");
			// You died!
			yield return GameOver();
		} else {
			player.RestItems();
			yield return ListenForPlayerInput();
		}
	}

	private IEnumerator GameOver() {
		yield return null;
		manager.SwapToMainMenu();
	}

	public IEnumerator SlowMove(GameObject go, Coord target, float time) {
		float dt = 0;
		Vector3 startPosition = go.transform.position;
		Vector3 endPosition = target.toVec();
		while (dt < time) {
			yield return null;
			dt += Time.deltaTime;
			go.transform.position = Vector3.Lerp(startPosition, endPosition, dt/time);
		}
		go.transform.position = endPosition;
	}

	public delegate IEnumerator TargettedAction(GameInstance instance, Coord c, IEnumerator success, IEnumerator cancel);
	public IEnumerator SelectTarget(int range, KeyCode selectKeyCode, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		SetInstructions("Select target w/ WASD");
		yield return null;
		targettingReticle.transform.position = player.transform.position;
		// TODO these should probably be provided.
		int x = (int)player.pos.x;
		int y = (int)player.pos.y;
		targettingReticle.SetActive(true);
		while (true) {
			if (Input.GetKeyDown(KeyCode.W) && player.pos.DistanceTo(new Coord(x, y+1)) <= range) {
				y += 1;
				yield return SlowMove(targettingReticle, new Coord(x, y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.A) && player.pos.DistanceTo(new Coord(x-1, y)) <= range) {
				x -= 1;
				yield return SlowMove(targettingReticle, new Coord(x, y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.S) && player.pos.DistanceTo(new Coord(x, y-1)) <= range) {
				y -= 1;
				yield return SlowMove(targettingReticle, new Coord(x, y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.D) && player.pos.DistanceTo(new Coord(x+1, y)) <= range	) {
				x += 1;
				yield return SlowMove(targettingReticle, new Coord(x, y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(selectKeyCode)) {
				break;
			} else if (Input.GetKeyDown(KeyCode.Escape)) {
				SetInstructions("");
				targettingReticle.SetActive(false);
				yield return cancel;
				yield break;
			}
			yield return null;
		}
		SetInstructions("");
		targettingReticle.SetActive(false);
		yield return callback(this, new Coord(x, y), success, cancel);
	}

	public IEnumerator SelectCardinalDirection(KeyCode selectKeyCode, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		TargettedAction a = (instance, c, suc, can) => callback(instance, player.pos + c, suc, can);
		return GetCardinalDirectionInput(selectKeyCode, a, success, cancel);
	}
	public bool EmptyAndPassable(Coord c) {
		return GetTile(c).passable && GetEntityAt(c) == null;
	}
	public IEnumerator CardinalDirectionOpen(KeyCode selectKeyCode, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		TargettedAction a = (instance, c, suc, can) => {
			Coord pos = player.pos;
			while (EmptyAndPassable(pos+c)) {
				pos += c;
			}
			return callback(instance, pos, suc, can);
		};
		return GetCardinalDirectionInput(selectKeyCode, a, success, cancel);
	}
	public IEnumerator CardinalDirectionFirstEntity(KeyCode selectKeyCode, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		TargettedAction a = (instance, c, suc, can) => {
			Coord pos = player.pos;
			while (GetTile(pos + c).passable && GetEntityAt(pos+c) == null) {
				pos += c;
			}
			if (GetEntityAt(pos+c) != null) {
				return callback(instance, pos+c, suc, can);
			} else {
				return cancel;
			}
		};
		return GetCardinalDirectionInput(selectKeyCode, a, success, cancel);
	}
	public IEnumerator GetCardinalDirectionInput(KeyCode selectKeyCode, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		yield return null;
		SetInstructions("Select a direction w/ WASD");

		// TODO these should probably be provided.
		int oX = (int)player.transform.position.x;
		int oY = (int)player.transform.position.y;
		int x = 0;
		int y = -1;

		targettingReticle.transform.position = player.transform.position  + new Coord(x, y).toVec();
		targettingReticle.SetActive(true);
		while (true) {
			if (Input.GetKeyDown(KeyCode.W)) {
				x = 0;
				y = 1;
				yield return SlowMove(targettingReticle, new Coord(oX+x, oY+y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.A)) {
				x = -1;
				y = 0;
				yield return SlowMove(targettingReticle, new Coord(oX+x, oY+y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.S)) {
				x = 0;
				y = -1;
				yield return SlowMove(targettingReticle, new Coord(oX+x, oY+y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(KeyCode.D)) {
				x = 1;
				y = 0;
				yield return SlowMove(targettingReticle, new Coord(oX+x, oY+y), GameManager.StandardDelay);
			} else if (Input.GetKeyDown(selectKeyCode)) {
				break;
			} else if (Input.GetKeyDown(KeyCode.Escape)) {
				SetInstructions("");
				targettingReticle.SetActive(false);
				yield return cancel;
				yield break;
			}
			yield return null;
		}
		SetInstructions("");
		targettingReticle.SetActive(false);
		yield return callback(this, new Coord(x, y), success, cancel);
	}

	public IEnumerator RandomSpace(Coord focus, int range, TargettedAction callback, IEnumerator success, IEnumerator cancel) {
		List<Coord> possibleDestinations = new List<Coord>();
		for (int i = -range; i <= range; i+= 1) {
			for (int j = -range; j <= range; j+= 1) {
				Coord c2 = focus + new Coord(i, j);
				if (Mathf.Abs(i+j) <= range && 
					InBounds(c2) && 
					GetEntityAt(c2) == null && 
					GetTile(c2).passable) {

					possibleDestinations.Add(c2);
				}
			}
		}

		if (possibleDestinations.Count == 0) {
			yield return cancel;
		} else {			
			Coord finalChoice = possibleDestinations[Random.Range(0, possibleDestinations.Count)];
			yield return callback(this, finalChoice, success, cancel);
		}
		yield break;
	}
}

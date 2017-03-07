using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffect {
	public TileEffectDefinition def;
	private MapTileComponent myTile;
	private int turnCount;
	public TileEffect(TileEffectDefinition def, MapTileComponent myTile) {
		this.def = def;
		this.myTile = myTile;
	}

	public bool HasPassive() {
		return def.effectType == TileEffectDefinition.Type.FUSE;
	}
	public bool Expired() {
		return turnCount > def.fuseTime;
	}
	public IEnumerator Passive(GameInstance instance) {
		if (HasPassive()) {
			turnCount += 1;
			// If we are now expired, that means the fuse ran out this turn.
			if (Expired()) {
				yield return Activate(instance);
			}
		}
	}

	public IEnumerator Activate(GameInstance instance) {
		Debug.Log("Triggering!");
		List<Coroutine> coroutines = new List<Coroutine>();
		for (int i = -1; i <=1; i+=1) {
			for (int j = -1; j <=1; j+=1) {
				Coord c = new Coord(myTile.x + i, myTile.y + j);
				if (instance.InBounds(c)) {
					coroutines.Add(instance.StartCoroutine(Effects.TargettedActivation(instance, c, null, null, def.onTrigger, def.power, def.range, null)));
				}
			}
		}
		foreach (Coroutine c in coroutines) {
			yield return c;
		}
	}
}

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

	public bool ShouldAvoid(Entity e) {
		if (!def.avoid) {
			return false;
		}
		if (!def.targetsFliers && (e != null) && e.HasQuality(Qualities.Quality.FLYING)) {
			
			return false;
		}
		return true;
	}
	public bool HasPassive() {
		return def.effectType != TileEffectDefinition.Type.INACTIVE;
	}
	public bool Expired() {
		return turnCount > def.fuseTime && def.effectType == TileEffectDefinition.Type.FUSE;
	}
	public string Explain() {
		return def.explanation;
	}
	private bool ShouldActivate() {
		return (turnCount > def.fuseTime && def.effectType == TileEffectDefinition.Type.FUSE) ||
			def.effectType == TileEffectDefinition.Type.EVERY_TURN;
	}
	public IEnumerator Passive(GameInstance instance) {
		if (HasPassive()) {
			turnCount += 1;
			// If we are now expired, that means the fuse ran out this turn.
			if (ShouldActivate()) {
				yield return Activate(instance);
			}
		}
	}

	public IEnumerator AddTargettedMessageToLog(GameInstance instance, Coord c) {
		if (def.targettedMessage != "") {
			Entity e = instance.GetEntityAt(c);
			instance.AddEvent(def.targettedMessage.Replace("<name>", e.DisplayName()));			
		}

		yield return null;
	}
	public IEnumerator Activate(GameInstance instance) {
		instance.AddEvent(def.untargettedMessage);
		List<Coroutine> coroutines = new List<Coroutine>();
		for (int i = -def.range; i <=def.range; i+=1) {
			for (int j = -def.range; j <=def.range; j+=1) {
				Coord c = new Coord(myTile.x + i, myTile.y + j);
				if (instance.InBounds(c)) {
					coroutines.Add(instance.StartCoroutine(Effects.TargettedActivation(instance, c, AddTargettedMessageToLog(instance, c), null, def.onTrigger, def.power, 0, null, "")));
				}
			}
		}
		foreach (Coroutine c in coroutines) {
			yield return c;
		}
	}
}

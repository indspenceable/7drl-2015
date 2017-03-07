using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects {
	public enum Effect {
		DAMAGE = 0,
		HEALING = 1,
		SCRY = 2,
		BACKSTAB = 3,
		KNOCKBACK = 4,
		ADD_TILE_EFFECT = 5,
		HOOK = 6,
		GOTO= 7,
		USE_TILE=8,
		SNARE=9,
	}


	// TODO this should be wired up in a more intuitive way. Oh well, 7 days.

	public static IEnumerator TargettedActivation(
		GameInstance instance, 
		Coord c, 
		IEnumerator success,
		IEnumerator cancel,
		Effect effect,
		int power,
		int range,
		TileEffectDefinition tileEffect
	){ 
		Entity target;
		switch(effect) {
		case Effects.Effect.DAMAGE:
			target = instance.GetEntityAt(c);
			if (target != null) {
				yield return target.TakeHit(power);
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.HEALING:
			target = instance.GetEntityAt(c);
			if (target != null) {
				yield return target.TakeHit(-power);
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.BACKSTAB:
			target = instance.GetEntityAt(c);
			Coord dest = c-instance.player.pos + c;
			MapTileComponent tile = instance.GetTile(dest);
			if (tile.passable && instance.GetEntityAt(dest) == null) {
				yield return target.TakeHit(-power);
				yield return instance.AttemptMove(dest, success, cancel);
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.GOTO:
			yield return instance.AttemptMove(c, success, cancel);
			yield break;
		case Effects.Effect.ADD_TILE_EFFECT:
			MapTileComponent t = instance.GetTile(c);
			t.AddTileEffect(tileEffect);
			yield return success;
			yield break;
		case Effects.Effect.HOOK:
			MonsterComponent monster = instance.GetEntityAt(c) as MonsterComponent;
			Coord pullDestination = instance.player.pos + (c - instance.player.pos).AsCardinalDirection();
			yield return instance.SlowMove(monster.gameObject, pullDestination, GameManager.StandardDelay);
			monster.pos = pullDestination;
			monster.stunned += 1;

			yield return success;
			yield break;
		case Effects.Effect.KNOCKBACK:
			monster = instance.GetEntityAt(c) as MonsterComponent;
			pullDestination = instance.player.pos + (c - instance.player.pos).AsCardinalDirection();
			yield return instance.SlowMove(monster.gameObject, pullDestination, GameManager.StandardDelay);
			monster.pos = pullDestination;
			monster.stunned += power;
			yield return success;
			yield break;
		case Effects.Effect.SCRY:
		case Effects.Effect.SNARE:
		case Effects.Effect.USE_TILE:
		default:
			Debug.LogError("Unexpected Effect: " + effect);
			yield break;
		}
	}
}

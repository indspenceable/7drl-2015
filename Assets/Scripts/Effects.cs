using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects {
	public enum Effect {
		DAMAGE = 0,
		GROUNDED_DAMAGE = 1,
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
		TileEffectDefinition tileEffect,
		string message
	){ 
		Entity target;
		switch(effect) {
		case Effects.Effect.DAMAGE:
			target = instance.GetEntityAt(c);
			if (target != null) {
				instance.AddEvent(message.Replace("<name>", target.DisplayName()));
				yield return target.TakeHit(power);
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.GROUNDED_DAMAGE:
			target = instance.GetEntityAt(c);
			if (target != null && !target.HasQuality(Qualities.Quality.FLYING)) {
				instance.AddEvent(message.Replace("<name>", target.DisplayName()))	;
				yield return target.TakeHit(power);
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.BACKSTAB:
			target = instance.GetEntityAt(c);
			Coord dest = c-instance.player.pos + c;
			MapTileComponent tile = instance.GetTile(dest);
			if (tile.passable && instance.GetEntityAt(dest) == null && target != null) {
				instance.AddEvent(message.Replace("<name>", target.DisplayName()));
				yield return target.TakeHit(power);
				yield return instance.AttemptMove(dest, success, cancel);
			} else {
				yield return cancel;
			}
			yield break;
		case Effects.Effect.GOTO:
			instance.AddEvent(message);
			yield return instance.AttemptMove(c, success, cancel);
			yield break;
		case Effects.Effect.ADD_TILE_EFFECT:
			instance.AddEvent(message);
			MapTileComponent t = instance.GetTile(c);
			t.AddTileEffect(tileEffect);
			yield return success;
			yield break;
		case Effects.Effect.HOOK:
			if (instance.GetEntityAt(c) == null) {
				yield return cancel;
				yield break;
			} else {
				MonsterComponent monster = instance.GetEntityAt(c) as MonsterComponent;
				Coord pullDestination = instance.player.pos + (c - instance.player.pos).AsCardinalDirection();
				instance.AddEvent(message.Replace("<name>", monster.DisplayName()));

				yield return instance.SlowMove(monster.gameObject, pullDestination, GameManager.StandardDelay);
				monster.pos = pullDestination;
				monster.stunned += 1;

				yield return success;
				yield break;
			}
		case Effects.Effect.KNOCKBACK:
			if (instance.GetEntityAt(c) == null) {
				yield return cancel;
				yield break;
			} else {
				MonsterComponent monster = instance.GetEntityAt(c) as MonsterComponent;
				Coord pushDirection =  (c - instance.player.pos).AsCardinalDirection();
				int tilesPushed = 0;
				instance.AddEvent(message.Replace("<name>", monster.DisplayName()));

				while (instance.EmptyAndPassable(monster.pos + pushDirection) && tilesPushed<range) {
					yield return instance.SlowMove(monster.gameObject, monster.pos + pushDirection, GameManager.StandardDelay);
					monster.pos = monster.pos + pushDirection;
					tilesPushed += 1;
				}
				if(tilesPushed < range) {
					instance.AddEvent("<name> is stunned!".Replace("<name>", monster.DisplayName()));
					monster.stunned += power;
				}
				yield return success;
				yield break;
			}
		case Effects.Effect.SNARE:
			if (instance.GetEntityAt(c) == null) {
				yield return cancel;
				yield break;
			} else {
				MonsterComponent monster = instance.GetEntityAt(c) as MonsterComponent;
				instance.AddEvent(message.Replace("<name>", monster.DisplayName()));

				monster.stunned += power;
				yield return success;
				yield break;
			}
		case Effects.Effect.USE_TILE:
			if (instance.GetTile(c).interaction != TileTerrain.Interaction.NONE) {
				yield return instance.Interact(c, success, cancel);
			} else {
				yield return cancel;
			}
			yield break;
		default:
			Debug.LogError("Unexpected Effect: " + effect);
			yield break;
		}
	}
}

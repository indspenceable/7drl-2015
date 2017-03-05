using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterComponent : MonoBehaviour {
	private Monster mt;
	public Coord pos;

	public void Setup(Monster mt, Coord pos) {
		this.mt = mt;
		GetComponent<SpriteRenderer>().sprite = mt.sprite;
		this.pos = pos;
		transform.position = this.pos.toVec();
	}

	public IEnumerator ExecuteStrategy(GameInstance instance) {
		switch(mt.strategy) {
		case Monster.Strategy.BlindAttack:
			return BlindAttack(instance);
		default:
			throw new UnityException("Unhandled AI Strategy.");
		}
	}

	public IEnumerator DisplayAndExecuteAttack() {
		Debug.Log("We did the attack!");
		yield return null;
	}

	public IEnumerator BlindAttack(GameInstance instance) {
		List<Coord> path = instance.AStarMonsterToPlayer(this);
		if (path == null || path.Count == 0) {
			// do nothing - no way to approach player
			// Or we're on top of the player..... shouldn't happen, so ignore.
		} else if (path.Count == 1) {
			// We're adjacent to the player. Do the attack
			yield return DisplayAndExecuteAttack();
		} else {
			yield return instance.SlowMove(this.gameObject, path[0], 0.1f);
			this.pos = path[0];
		}
	}
}

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
		case Monster.Strategy.StandardAttack:
			return StandardAttack(instance);
		default:
			throw new UnityException("Unhandled AI Strategy.");
		}
	}

	public IEnumerator DisplayAndExecuteAttack() {
		Debug.Log("We did the attack!");
		yield return null;
	}

	public IEnumerator StandardAttack(GameInstance instance) {
		DjikstraMap map = instance.BuildPlayerMap();
		if (map.Value(pos.x, pos.y) < mt.minRange) {
			map.Scale(-1.2f);
			map.Calculate(instance.Passable);
			Coord c = map.FindBestNeighbor(pos);
			yield return instance.SlowMove(gameObject, c, GameManager.StandardDelay);
			pos = c;
		} else if (map.Value(pos.x, pos.y) > mt.maxRange) {
			Coord c = map.FindBestNeighbor(pos);
			yield return instance.SlowMove(gameObject, c, GameManager.StandardDelay);
			pos = c;
		} else {
			yield return DisplayAndExecuteAttack();
		}
	}
}

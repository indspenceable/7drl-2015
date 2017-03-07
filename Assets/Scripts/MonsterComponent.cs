using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	public class MonsterComponent : MonoBehaviour, Entity {
	private Monster mt;
	public Coord pos;
	public int hits;
	public int stunned;

	public void Setup(Monster mt, Coord pos) {
		this.mt = mt;
		GetComponent<SpriteRenderer>().sprite = mt.sprite;
		this.pos = pos;
		this.hits = 0;
		this.stunned = 0;
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

	// For the inteface.
	public GameObject GameObject() {
		return gameObject;
	}

	public IEnumerator TakeHit(int power) {
		Debug.Log("Monster Got hit for " + power + " power.");
		hits += power;
		yield return null;
	}

	public IEnumerator DisplayAndExecuteAttack() {
		Debug.Log("We did the attack!");
		yield return null;
	}

	public bool IsDead() {
		return hits > mt.hp;
	}

	public IEnumerator StandardAttack(GameInstance instance) {
		if (stunned > 0) {
			stunned -= 1;
			yield break;
		}

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

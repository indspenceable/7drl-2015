using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	public class MonsterComponent : MonoBehaviour, Entity {
	private Monster mt;
	public Coord pos;
	public int hits;
	public int stunned;

	public bool hover;

	public void OnMouseEnter() {
		hover = true;
	}
	public void OnMouseExit() {
		hover = false;
	}


	public void Setup(Monster mt, Coord pos) {
		this.mt = mt;
		GetComponent<SpriteRenderer>().sprite = mt.sprite;
		this.pos = pos;
		this.hits = 0;
		this.stunned = 0;
		transform.position = this.pos.toVec();
	}

	public string DisplayName() {
		return mt.displayName;
	}
	public string Desc() {
		return mt.desc;
	}


	public IEnumerator ExecuteStrategy(GameInstance instance) {
		if (stunned > 0) {
			stunned -= 1;
			return null;
		}

		switch(mt.strategy) {
		case Monster.Strategy.STANDARD_ATTACK:
			return StandardAttack(instance);
		case Monster.Strategy.STATIC:
			return Static(instance);
		case Monster.Strategy.HEAL:
			return Healer(instance);
		default:
			throw new UnityException("Unhandled AI Strategy.");
		}
	}

	public bool HasQuality(Qualities.Quality q) {
		if (q == Qualities.Quality.FLYING) {
			return mt.flies;
		}
		return false;
	}

	// For the inteface.
	public GameObject GameObject() {
		return gameObject;
	}

	public IEnumerator TakeHit(int power) {
		hits += power;
		yield return null;
	}

	public IEnumerator DisplayAndExecuteAttack(GameInstance instance) {
		instance.AddEvent("The " + DisplayName() + " hits " + instance.player.DisplayName());
		switch(mt.anim) {
		case Monster.AttackAnimation.PUNCH:
			yield return instance.DisplayPunch(instance.player.pos);
			break;
		case Monster.AttackAnimation.ARROW:
			yield return instance.DisplayArrow(pos, instance.player.pos);
			break;
		case Monster.AttackAnimation.SHOCK:
			yield return instance.DisplayShock(pos, instance.player.pos);
			break;
		default:
			// No animation
//			throw new UnityException("Unexpected attack animation!");
			break;
		}
		yield return instance.player.TakeHit(mt.damage);
	}

	public IEnumerator Heal(MonsterComponent target) {
		yield return target.TakeHit(-mt.damage);
	}

	public bool IsDead() {
		return hits >= mt.hp;
	}


	// HEALING doesn't repect min range. Deal with it.
	public IEnumerator Healer(GameInstance instance) {
		List<MonsterComponent> others = instance.GetMonsters();
		others.Remove(this);
		DjikstraMap map = instance.BuildPlayerMap(this);
		map.Scale(-2.4f);
		List<DjikstraMap> maps = new List<DjikstraMap>();
		foreach (MonsterComponent mc in others) {
			DjikstraMap currentMap = instance.BuildTargettedMap(this, mc.pos);
			if (map.Value(pos.x, pos.y) < mt.maxRange && mc.hits > 0) {
				yield return Heal(mc);
				yield break;
			}
			maps.Add(currentMap);
		}
		//If we get here, we aren't close to a target that needs healing. So, find one!
		foreach (DjikstraMap m in maps) {
			map.CombineWith(m);
		}
		Coord c = map.FindBestNeighbor(pos, (co) => instance.Pathable(co) && instance.GetEntityAt(co) == null);
		yield return instance.SlowMove(gameObject, c, GameManager.StandardDelay);
		pos = c;
	}
	public IEnumerator Static(GameInstance instance) {
		int dist = pos.DistanceTo(instance.player.pos);
		if (dist >= mt.minRange && dist <= mt.maxRange) {
			yield return DisplayAndExecuteAttack(instance);
		}
	}
	public IEnumerator StandardAttack(GameInstance instance) {
		DjikstraMap map = instance.BuildPlayerMap(this);
		if (map.Value(pos.x, pos.y) < mt.minRange) {
			map.Scale(-1.2f);
			map.Calculate(instance.Pathable, this);
			Coord c = map.FindBestNeighbor(pos, (co) => instance.Pathable(co, this) && instance.GetEntityAt(co) == null );
			yield return instance.SlowMove(gameObject, c, GameManager.StandardDelay);
			pos = c;
		} else if (map.Value(pos.x, pos.y) > mt.maxRange) {
			Coord c = map.FindBestNeighbor(pos, (co) => instance.Pathable(co, this) && instance.GetEntityAt(co) == null);
			yield return instance.SlowMove(gameObject, c, GameManager.StandardDelay);
			pos = c;
		} else {
			yield return DisplayAndExecuteAttack(instance);
		}
	}
}

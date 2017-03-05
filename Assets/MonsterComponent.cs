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
}

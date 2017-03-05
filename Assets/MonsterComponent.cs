using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterComponent : MonoBehaviour {
	private Monster mt;
	public Coord pos;

	public void Setup(Monster mt, int x, int y) {
		this.mt = mt;
		GetComponent<SpriteRenderer>().sprite = mt.sprite;
		pos = new Coord(x, y);
		transform.position = pos.toVec();
	}
}

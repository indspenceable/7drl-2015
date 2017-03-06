using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[System.Serializable]
public class MapTileComponent : MonoBehaviour {
	private TileTerrain terrain;
	public int x {get; private set;}
	public int y {get; private set;}

	public bool passable {
		get {
			return terrain.passable;
		}
	}

	public bool interactable = false;

	public void SetTerrain(TileTerrain t) {
		this.terrain = t;
		this.GetComponent<SpriteRenderer>().sprite = terrain.sprite;
	}
	public void SetCoords(int x, int y) {
		this.x = x;
		this.y = y;
		transform.position = new Vector3(x,y);
	}
}

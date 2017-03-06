using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, Entity {
	public Coord pos;
	private Item[] gear;
	[SerializeField]
	private ItemDefinition[] startingGear;

	// Use this for initialization
	void Start () {
		gear = new Item[4];
		for (int i = 0; i < startingGear.Length; i += 1) {
			gear[i] = new Item(startingGear[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator TakeHit(int power) {
		Debug.Log("Player Got hit for " + power + " power.");
		yield return null;
	}


	public void SetCoords(Coord c) {
		this.pos = c;
		transform.position = pos.toVec();
	}
	public Item GetItem(int slot) {
		return gear[slot];
	}
}

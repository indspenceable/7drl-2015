using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, Entity {
	public Coord pos;
	[SerializeField]
	private List<Item> gear;
	[SerializeField]
	private ItemDefinition[] startingGear;
	public int hp =5;

	// Use this for initialization
	void Start () {
		gear = new List<Item>(4);
		for (int i = 0; i < startingGear.Length; i += 1) {
			gear.Add(new Item());
			SetItem(i, startingGear[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool HasQuality(Qualities.Quality quality) {
		return gear.Exists(g => g.GivesQuality(quality));
	}

	// For the interface
	public GameObject GameObject() {
		return gameObject;
	}

	public IEnumerator TakeHit(int power) {
		hp -= power;
		yield return null;
	}

	public bool IsDead() {
		return hp <= 0;
	}


	public void SetCoords(Coord c) {
		this.pos = c;
		transform.position = pos.toVec();
	}
	public Item GetItem(int slot) {
		return gear[slot];
	}
	public void  RestItems() {
		foreach (Item i in gear) {
			i.Rest();
		}
	}
	public void LevelRestItems() {
		foreach (Item i in gear) {
			i.LevelRest();
		}
	}
	public void SetItem(int i, ItemDefinition item) {
		gear[i] = new Item();
		gear[i].SetType(item);
	}
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public Coord pos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetCoords(int x, int y) {
		this.pos = new Coord(x, y);
		transform.position = pos.toVec();
	}
}
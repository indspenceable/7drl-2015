﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain", menuName = "TileTerrain", order = 1)]
public class TileTerrain : ScriptableObject {
	// display
	public Sprite sprite;
	// Can you move over this tile
	public bool passable;
	// can you fire over this tile
	public bool blocked;
}
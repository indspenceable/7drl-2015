using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vault {
	public Coord size;
	public string map;
	public bool rotated;
	public char At(int x, int y) {
		if (rotated) {
			return map[x * size.y + y];
		} else {
			return map[y * size.x + x];
		}
	}
	public void Rotate() {
		rotated = true;
		size = new Coord(size.y, size.x);
	}
}
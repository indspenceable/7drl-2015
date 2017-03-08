using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain", menuName = "TileTerrain", order = 1)]
public class TileTerrain : ScriptableObject {
	public enum Interaction {
		NONE,
		NEXT_LEVEL,
		GIVE_ITEM,
	}

	// display
	public Sprite sprite;
	// Can you move over this tile
	public bool passable;
	// can you fire over this tile
	public bool losBlocked;
//	// Will monsters ever path over this?
//	public bool avoidThis;
	public Interaction interaction;
	public TileEffectDefinition tileEffect;
}

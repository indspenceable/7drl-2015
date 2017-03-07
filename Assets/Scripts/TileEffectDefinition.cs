using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Effect", menuName = "Tile Effect", order = 1)]
public class TileEffectDefinition: ScriptableObject {
	public enum Type {
		STATIC = 0,
		FUSE = 1,
	}
	public int fuseTime;
	public Type effectType;
	public bool providesVision;
	public Sprite sprite;
	public Effects.Effect onTrigger;
	public int power;
	public int range;
}

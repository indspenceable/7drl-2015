using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Effect", menuName = "Tile Effect", order = 1)]
public class TileEffectDefinition: ScriptableObject {
	public enum Type {
		INACTIVE = 0,
		FUSE = 1,
		EVERY_TURN = 2,
	}
	public int fuseTime;
	public Type effectType;
	public bool providesVision;
	public Sprite sprite;
	public Effects.Effect onTrigger;
	public int power;
	public int range;

	public bool targetsFliers;
	public bool avoid;

	public string explanation;

	public string targettedMessage;
	public string untargettedMessage;
}

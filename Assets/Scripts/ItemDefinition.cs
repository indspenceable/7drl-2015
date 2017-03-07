using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item", order = 1)]
public class ItemDefinition : ScriptableObject {
	public enum TargettingMethod {
		UNUSABLE = 0,
		ME = 1,
		WITHIN_RANGE = 2,
		CARDINAL=3,
		WITHIN_RANGE_RANDOM=4,
		CARDINAL_WHILE_OPEN=5,
		CARDINAL_FIRST_ENTITY=6,
	}
	public string displayName;
	public Sprite icon;
	public TargettingMethod targettingMethod;
	public Effects.Effect effect;
	public TileEffectDefinition tileEffect;
	public int cooldown = -1;
	public int levelLimit = -1;
	public int totalCharges = -1;
	public int targettingRange = 0;
	public int power = 1;
	public int LOSModifier = 0;
	public int thornsDamage = 0;
}

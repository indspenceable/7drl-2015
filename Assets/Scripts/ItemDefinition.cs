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
	}
	public enum Effect {
		DAMAGE = 0,
		HEALING = 1,
	}
	public TargettingMethod targettingMethod;
	public Effect effect;
	public int cooldown = -1;
	public int levelLimit = -1;
	public int totalCharges = -1;
	public int targettingRange = 0;
	public int power = 1;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item  {
	ItemDefinition itemType;
	int cooldown;
	int usesThisLevel;
	int chargesUsed;

	public string DisplayName {
		get {
			return itemType.displayName;
		}
	}

	public void SetType(ItemDefinition itemType) {
		this.itemType = itemType;
		this.cooldown = 0;
		this.usesThisLevel = 0;
		this.chargesUsed = 0;
	}

	public bool Usable() {
		return CheckCooldown() && CheckLevelLimit() && CheckCharges() && 
			itemType.targettingMethod != ItemDefinition.TargettingMethod.UNUSABLE;
//		return true;
	}

	public IEnumerator TargettedItemActivation(GameInstance instance, Coord c, IEnumerator success, IEnumerator cancel) {
		yield return Effects.TargettedActivation(instance, c, success, cancel, itemType.effect, itemType.power, itemType.targettingRange, itemType.tileEffect);
	}

	public IEnumerator BeginActivation(GameInstance instance, IEnumerator success, IEnumerator back) {
		if (Usable()) {
			switch(itemType.targettingMethod) {
			case ItemDefinition.TargettingMethod.CARDINAL:
				yield return instance.SelectCardinalDirection(KeyCode.Space, TargettedItemActivation, Use(success), back);
				yield break;
			case ItemDefinition.TargettingMethod.WITHIN_RANGE:
				yield return instance.SelectTarget(KeyCode.Space, TargettedItemActivation, Use(success), back);
				yield break;
			case ItemDefinition.TargettingMethod.WITHIN_RANGE_RANDOM:
				// Probably shouldn't use the player directly here...
				yield return instance.RandomSpace(instance.player.pos, itemType.targettingRange, TargettedItemActivation, Use(success), back);
				yield break;
			case ItemDefinition.TargettingMethod.ME:
				// Or here.
				yield return TargettedItemActivation(instance, instance.player.pos, Use(success), back);
				yield break;
			default:
				Debug.LogError("Unexpected targetting method."  + itemType.targettingMethod);
				yield return back;
				yield break;
			}
		} else {
			yield return back;
		}
	}

	private IEnumerator Use(IEnumerator callback) {
		if (itemType.cooldown != -1) {
			cooldown = itemType.cooldown;
		}
		if (itemType.levelLimit != -1) {
			usesThisLevel += 1;
		}
		if (itemType.totalCharges != -1) {
			chargesUsed += 1;
		}
		yield return callback;
	}

	private bool CheckCooldown() {
		return (itemType.cooldown == -1 || cooldown == 0);
	}
	private bool CheckLevelLimit() {
		return (itemType.levelLimit == -1 || usesThisLevel < itemType.levelLimit);
	}
	private bool CheckCharges() {
		return (itemType.totalCharges == -1 || chargesUsed < itemType.totalCharges);
	}
}

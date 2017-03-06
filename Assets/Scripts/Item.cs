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
	}


	public IEnumerator BeginActivation(GameInstance instance, IEnumerator success, IEnumerator back) {
		// Check if we're able to activate at this point
		IEnumerator cancel = BeginActivation(instance, success, back);

		if (Usable()) {
			switch(itemType.targettingMethod) {
			case ItemDefinition.TargettingMethod.CARDINAL:
				yield return instance.SelectCardinalDirection(KeyCode.Space, TargettedActivation, success, back);
				yield break;
			case ItemDefinition.TargettingMethod.WITHIN_RANGE:
				yield return instance.SelectTarget(KeyCode.Space, TargettedActivation, success, back);
				yield break;
			case ItemDefinition.TargettingMethod.ME:
				yield return TargettedActivation(instance, instance.player.pos, success, cancel);
				yield break;
			default:
				throw new UnityException("Unexpected TargettingMethod: " + itemType.targettingMethod);
			}
		} else {
			yield return back;
		}
	}


	private IEnumerator TargettedActivation(GameInstance instance, Coord c, IEnumerator success, IEnumerator cancel){ 
		Entity e;
		switch(itemType.effect) {
		case ItemDefinition.Effect.DAMAGE:
			e = instance.GetEntityAt(c);
			if (e != null) {
				yield return e.TakeHit(itemType.power);
				Use();
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case ItemDefinition.Effect.HEALING:
			e = instance.GetEntityAt(c);
			if (e != null) {
				yield return e.TakeHit(-itemType.power);
				Use();
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		default:
			throw new UnityException("Unexpected Effect: " + itemType.effect);
		}
	}

	private void Use() {
		if (itemType.cooldown != -1) {
			cooldown = itemType.cooldown;
		}
		if (itemType.levelLimit != -1) {
			usesThisLevel += 1;
		}
		if (itemType.totalCharges != -1) {
			chargesUsed += 1;
		}
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

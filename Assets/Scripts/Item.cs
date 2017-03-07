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
		Entity target;
		switch(itemType.effect) {
		case ItemDefinition.Effect.DAMAGE:
			target = instance.GetEntityAt(c);
			if (target != null) {
				yield return target.TakeHit(itemType.power);
				Use();
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case ItemDefinition.Effect.HEALING:
			target = instance.GetEntityAt(c);
			if (target != null) {
				yield return target.TakeHit(-itemType.power);
				Use();
				yield return success;
			} else {
				yield return cancel;
			}
			yield break;
		case ItemDefinition.Effect.BACKSTAB:
			target = instance.GetEntityAt(c);
			Coord dest = c-instance.player.pos + c;
			MapTileComponent tile = instance.GetTile(dest);
			if (tile.passable && instance.GetEntityAt(dest) == null) {
				yield return target.TakeHit(-itemType.power);
				yield return instance.AttemptMove(dest);
			} else {
				yield return cancel;
			}
			yield break;
		case ItemDefinition.Effect.BLINK:
			List<Coord> possibleDestinations = new List<Coord>();
			for (int i = -itemType.targettingRange; i <= itemType.targettingRange; i+= 1) {
				for (int j = -itemType.targettingRange; j <= itemType.targettingRange; j+= 1) {
					Coord c2 = c + new Coord(i,j);
					if (instance.InBounds(c2) && instance.GetEntityAt(c2) == null) {
						possibleDestinations.Add(c2);
					}
				}
			}
			if (possibleDestinations.Count == 0) {
				yield return cancel;
			} else {			
				Coord teleDest = possibleDestinations[Random.Range(0, possibleDestinations.Count)];
				yield return instance.AttemptMove(teleDest);
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

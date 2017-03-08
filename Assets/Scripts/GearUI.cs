﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearUI : MonoBehaviour {
	[SerializeField]
	int slot;
	[SerializeField]
	UnityEngine.UI.Text itemName;
	[SerializeField]
	UnityEngine.UI.Text cooldown;
	[SerializeField]
	UnityEngine.UI.Text chargesTotal;
	[SerializeField]
	UnityEngine.UI.Text chargesThisLevel;


	private Player player;

	public void InstallPlayer(Player p) {
		player = p;
	}

	// Update is called once per frame
	void Update () {
		// Actually draw the UI!
		if (player == null) return;
		Item item = player.GetItem(slot);
		Debug.Log(item);
		Debug.Log(slot);
		Debug.Log(itemName);
		itemName.text = item.DisplayName;
		if (item.itemType.cooldown != -1) {
			cooldown.text = "Cooldown: " + item.cooldown + " / " + item.itemType.cooldown;
		}
		if (item.itemType.totalCharges != -1) {
			chargesTotal.text = "" + item.chargesUsed + " / " + item.itemType.totalCharges + " used.";
		}
		if (item.itemType.levelLimit != -1 ) {
			chargesThisLevel.text = "" + item.usesThisLevel + " / " + item.itemType.levelLimit + " this level";
		}
	}
}

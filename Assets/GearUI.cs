using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearUI : MonoBehaviour {
	[SerializeField]
	int slot;
	[SerializeField]
	UnityEngine.UI.Text name;

	private Player player;

	public void InstallPlayer(Player p) {
		player = p;
	}

	// Update is called once per frame
	void Update () {
		// Actually draw the UI!
		if (player == null) return;
		Debug.Log("Setting name!");
		name.text = player.GetItem(slot).DisplayName;
	}
}

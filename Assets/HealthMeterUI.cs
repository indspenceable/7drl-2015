using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMeterUI : MonoBehaviour {
	private Player p;
	int lastKnownHealth;
	[SerializeField]
	private GameObject[] hearts;

	public void InstallPlayer(Player p) {
	}
	
	// Update is called once per frame
	void Update () {
		// Here's were we update the UI!
	}
}

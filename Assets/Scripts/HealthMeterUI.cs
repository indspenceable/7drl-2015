using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMeterUI : MonoBehaviour {
	private Player p;
	int lastKnownHealth;
	[SerializeField]
	private GameObject[] hearts;

	public void InstallPlayer(Player p) {
		this.p = p;
	}
	
	// Update is called once per frame
	void Update () {
		// Here's were we update the UI!
		for (int i = 0; i < hearts.Length; i+=1) {
			hearts[i].SetActive(p.hp>i);
		}
	}
}

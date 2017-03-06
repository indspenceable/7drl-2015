using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOption : MonoBehaviour {
	[SerializeField]
	GameObject pointer;
	public MainMenu.Action action;
	public void SetSelected(bool tf) {
		pointer.SetActive(tf);
	}
}

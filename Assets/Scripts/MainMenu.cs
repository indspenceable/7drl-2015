using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
	[SerializeField]
	private MenuOption[] options;
	int selectedIndex = 0;
	[SerializeField]
	GameManager manager;

	public enum Action {
		NEW_GAME,
		LOAD_GAME,
		EXIT
	}

	public void Startup() {
		RefreshSelected();
	}
	public void Update() {
		if (Input.GetKeyDown(KeyCode.W)) {
			selectedIndex -= 1;
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			selectedIndex += 1;
		}
		selectedIndex = (selectedIndex + options.Length) % options.Length;
		RefreshSelected();

		if (Input.GetKeyDown(KeyCode.Space)) {
			switch(options[selectedIndex].action) {
			case Action.NEW_GAME:
				manager.StartNewGame();
				break;
			case Action.LOAD_GAME:
				throw new UnityException("Load game is not yet implemented.");
			case Action.EXIT:
				Application.Quit();
				break;
			default:
				throw new UnityException("Unexpected main menu action " + options[selectedIndex].action);
			}
		}
	}

	private void RefreshSelected() {
		for (int i = 0; i < options.Length; i+=1) {
			options[i].SetSelected(i == selectedIndex);
		}
	}
}

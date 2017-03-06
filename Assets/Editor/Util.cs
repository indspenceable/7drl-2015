using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Util {
	[MenuItem("Edit/Reset Playerprefs")] 
	public static void DeletePlayerPrefs() { PlayerPrefs.DeleteAll(); }
}

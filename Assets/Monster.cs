using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster", order = 1)]
public class Monster : ScriptableObject {
	public Sprite sprite;
	public enum Strategy {
		BlindAttack = 0,
	}
	public Strategy strategy;
}

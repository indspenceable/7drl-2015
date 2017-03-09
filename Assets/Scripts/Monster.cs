using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster", order = 1)]
public class Monster : ScriptableObject {
	public Sprite sprite;
	public enum Strategy {
		STANDARD_ATTACK = 0,
		STATIC = 1,
		HEAL = 2,
	}
	public Strategy strategy;

	public int hp;
	public int armor;
	public int minRange;
	public int maxRange;
	public int damage;
	public bool flies;
	public int minimumFloor;

	public string displayName;
	[TextArea]
	public string desc;
}

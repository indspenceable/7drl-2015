using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster", order = 1)]
public class Monster : ScriptableObject {
	public Sprite sprite;
	public enum Strategy {
		StandardAttack = 0,
	}
	public Strategy strategy;

	public int hp;
	public int armor;
	public int minRange;
	public int maxRange;
	public int damage;
	public bool flies;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Entity {
	string DisplayName();
	IEnumerator TakeHit(int power);
	bool IsDead();
	GameObject GameObject();
	bool HasQuality(Qualities.Quality q);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Entity {
	IEnumerator TakeHit(int power);
}

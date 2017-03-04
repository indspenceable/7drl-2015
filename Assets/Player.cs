using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	int x; int y;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetCoords(int x, int y) {
		this.x = x;
		this.y = y;
		transform.position = new Vector3(x,y);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenParticlesFinished : MonoBehaviour {
	ParticleSystem mySystem;
	// Use this for initialization
	void Start () {
		mySystem = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!mySystem.IsAlive()) {
			Destroy(gameObject);
		}
	}
}

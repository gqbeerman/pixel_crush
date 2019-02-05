using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour {
    public ParticleSystem[] allParticles;
    public float lifetime = 1f;

    // Start is called before the first frame update
    void Start() {
        allParticles = GetComponentsInChildren<ParticleSystem>();

        Destroy(gameObject, lifetime);
    }

    public void Play() {
        foreach(ParticleSystem ps in allParticles) {
            ps.Stop();
            ps.Play();
        }
    }
}

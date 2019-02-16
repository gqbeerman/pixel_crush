using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour {
    public Image star;
    public ParticlePlayer starFX;
    public AudioClip starSound;

    public float delay = 0.5f;
    public bool activated = false;
    // Start is called before the first frame update
    void Start() {
        SetActive(false);
        //StartCoroutine(TestRoutine());
    }

    void SetActive(bool state) {
        if(star != null) {
            star.gameObject.SetActive(state);
        }
    }

    public void Activate() {
        if (activated) {
            return;
        }
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine() {
        activated = true;

        if(starFX != null) {
            starFX.Play();
        }

        if(SoundManager.Instance != null && starSound != null) {
            SoundManager.Instance.PlayClipAtPoint(starSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
        yield return new WaitForSeconds(delay);

        SetActive(true);
    }
    /*
    IEnumerator TestRoutine() {
        yield return new WaitForSeconds(3.0f);
        Activate();
    }
    */
}

using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
public class AudioShot : MonoBehaviour {

	// Use this for initialization
    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
        audio.Play(44100);
    }
}

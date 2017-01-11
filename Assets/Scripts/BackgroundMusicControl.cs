﻿using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BackgroundMusicControl : MonoBehaviour {

    public GameObject backgroundMusic;
    public AudioMixerSnapshot defaultSnap;
    public AudioMixerSnapshot beginGame;
    public AudioMixerSnapshot beginGame2;
    private bool started = false;


    // Use this for initialization
    void Start () {
	
	}


    void OnTriggerEnter2D(Collider2D other)
    {
        print("ENTERED SOUNDTRACK TRIGGER");
        if (!started && other.CompareTag("LocalPlayerTag"))
        {   
            float a = Random.Range(0.0f, 1.0f);
            if (a < 0.5f)
            {
                backgroundMusic.GetComponents<AudioSource>()[0].Play();
                beginGame.TransitionTo(0.01f);
            }
            else {
                backgroundMusic.GetComponents<AudioSource>()[1].Play();
                beginGame2.TransitionTo(0.01f);
            }
            started = true;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}

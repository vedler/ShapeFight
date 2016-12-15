using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BackgroundMusicControl : MonoBehaviour {


    public AudioMixerSnapshot defaultSnap;
    public AudioMixerSnapshot beginGame;
    private bool started = false;


	// Use this for initialization
	void Start () {
	
	}


    void OnTriggerEnter2D(Collider2D other)
    {
        print("ENTERED SOUNDTRACK TRIGGER");
        if (!started && other.CompareTag("LocalPlayerTag"))
        {
            beginGame.TransitionTo(0.1f);
            started = true;
        }
    }
}

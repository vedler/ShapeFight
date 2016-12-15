using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BackgroundMusicControl : MonoBehaviour {


    public AudioMixerSnapshot defaultSnap;
    public AudioMixerSnapshot beginGame;


	// Use this for initialization
	void Start () {
	
	}


    void OnTriggerEnter(Collider other)
    {
        print("ENTERED SOUNDTRACK TRIGGER");
        if (other.CompareTag("LocalPlayerTag"))
        {
            beginGame.TransitionTo(0.1f);
        }
    }
}

using UnityEngine;
using System.Collections;

public class ParticleSpawner : MonoBehaviour {

    public GameObject particle;

    private ParticlePool pool;

    // Use this for initialization
    void Start () {
        pool = GameManager.getInstance().getParticlePool();
        pool.CreatePool(particle, 100);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Spawn(Vector3 location)
    {
        for (int i = 0; i < 10; i++)
        {
            pool.ReuseObject(particle, location);
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {
    
    //tulistamise jaoks
    public GameObject shot;
    public Transform shotSpawn;
    public float fireDelta = 0.5F;
    private GameObject newShot;
    private float nextFire = 0.5F;
    private float myTime = 0.0F;
    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
        {
            return;
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        //copypaste unity dokumentatsioonist pmst
        myTime = myTime + Time.deltaTime;

        if (Input.GetButton("Fire1") && myTime > nextFire)
        {
            nextFire = myTime + fireDelta;
            newShot = Instantiate (shot, shotSpawn.position, shotSpawn.rotation) as GameObject;   

            nextFire = nextFire - myTime;
            myTime = 0.0F;
        }
        //https://unity3d.com/learn/tutorials/topics/multiplayer-networking/testing-multiplayer-movement?playlist=29690
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}

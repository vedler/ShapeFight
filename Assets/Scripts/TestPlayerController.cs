using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestPlayerController : NetworkBehaviour {
    


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

        //https://unity3d.com/learn/tutorials/topics/multiplayer-networking/testing-multiplayer-movement?playlist=29690
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}

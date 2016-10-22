using UnityEngine;
using System.Collections;

public class RocketMover : MonoBehaviour {
    public float speed;
    private bool shoot = false;
    private bool hasBeenShot;

	void Start () {
        hasBeenShot = false;
        //GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void startMoving ()
    {
        // = transform.forward * speed;
        shoot = true;
    }

    void FixedUpdate ()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.forward * speed, ForceMode2D.Impulse);
        if (shoot && !hasBeenShot)
        {
            hasBeenShot = true;
            //GetComponent<Rigidbody2D>().isKinematic = false;
        }
        //GetComponent<Rigidbody2D>().velocity = transform.forward * speed;
    }
}

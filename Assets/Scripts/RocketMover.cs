using UnityEngine;
using System.Collections;

public class RocketMover : PoolObject{
    public float speed;
    private bool shoot = false;
    private bool hasBeenShot;

    //[SerializeField]
    //public Vector2 direction;

	void Start () {
        hasBeenShot = false;
        //GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void startMoving ()
    {
        shoot = true;
        //GetComponent<Rigidbody2D>().AddForce(direction*100.0f, ForceMode2D.Impulse);
    }

    void FixedUpdate ()
    {
        //GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        //GetComponent<Rigidbody2D>().AddForce(new Vector3(100.0f, 0, 0));
        /* GetComponent<Rigidbody2D>().AddForce(direction * speed);
         if (shoot && !hasBeenShot)
         {
             hasBeenShot = true;
             //GetComponent<Rigidbody2D>().isKinematic = false;
         }
         //GetComponent<Rigidbody2D>().velocity = transform.forward * speed;
         */
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        this.gameObject.SetActive(false);
    }

    public override void FireMe(Vector2 direction)
    {
        GetComponent<Rigidbody2D>().AddForce(direction*speed, ForceMode2D.Impulse);
    }

}

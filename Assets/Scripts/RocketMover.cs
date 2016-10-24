using UnityEngine;
using System.Collections;

public class RocketMover : PoolObject{
    [SerializeField]
    public float speed;

    [SerializeField]
    public float maxSpeed;

    private bool isFired;
    private Vector2 direction;

    /*public override void Reset()
    {
        isFired = false;
        direction = Vector2.zero;
    }
    */

    //Neid võib hiljem vaja minna
    //private bool shoot = false; 
    //private bool hasBeenShot;

	void Start () {
        //hasBeenShot = false;
        //GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void startMoving ()
    {
        //shoot = true;
        //GetComponent<Rigidbody2D>().AddForce(direction*100.0f, ForceMode2D.Impulse);
    }

    void FixedUpdate ()
    {
        if (isFired)
        {
            direction.Normalize();
            GetComponent<Rigidbody2D>().AddForce(direction * maxSpeed, ForceMode2D.Impulse);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        this.gameObject.SetActive(false);
    }

    public override void FireMe(Vector2 direction)
    {
        GetComponent<Rigidbody2D>().AddForce(direction*speed, ForceMode2D.Impulse);
        this.direction = direction;
        isFired = true;
    }

}

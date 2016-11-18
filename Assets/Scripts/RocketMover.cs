using UnityEngine;
using System.Collections;

public class RocketMover : PoolObject{

    private GameManager gm;

    [SerializeField] 
    public float speed;

    [SerializeField]
    public float maxSpeed;

    private bool isFired;
    private Vector2 direction;
    private Vector2 startPosition;

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
        gm = GameManager.getInstance();
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
        if (collider.gameObject.tag == "LocalPlayerTag" || collider.gameObject.tag == "LocalProjectileTag")
        {
            return;
        }
        gm.onProjectileCollision(GetComponent<Rigidbody2D>().transform.position);
        this.gameObject.SetActive(false);
    }

    public override void FireMe(Vector2 direction)
    {
        GetComponent<Rigidbody2D>().AddForce(direction*speed, ForceMode2D.Impulse);
        this.direction = direction;
        isFired = true;
    }

    public void SetVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
        GetComponent<Rigidbody2D>().AddForce(velocity, ForceMode2D.Impulse);
    }

    public void SetStartPosition(Vector2 sp)
    {
        startPosition = sp;
    }

}

using UnityEngine;
using System.Collections;
using System;

public abstract class AbsWeaponMover : MonoBehaviour, PoolObject, IWeaponMover {

    public float maxSpeed;
    public float cooldownPeriod;
    public GameObject particleSysPrefab;

    protected bool isFired;
    protected bool hasEnded;
    protected Vector2 direction;
    protected Vector2 startPosition;

    public abstract void FireMe(Vector2 direction);

    public abstract void move();

    public abstract void OnObjectReuse();

    protected Rigidbody2D rigidBody;

    public virtual void Reset()
    {
        throw new NotImplementedException();
    }

    // Use this for initialization
    void Start () {
        this.rigidBody = GetComponent<Rigidbody2D>();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (isFired)
        {
            move();

            if (rigidBody.velocity.magnitude > maxSpeed)
            {
                Vector2 vel = GetComponent<Rigidbody2D>().velocity;
                vel.Normalize();
                GetComponent<Rigidbody2D>().velocity = vel * maxSpeed; 
            }
        }

        if (hasEnded)
        {
            this.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "LocalPlayerTag" || collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
        {
            return;
        }
        GameObject explosion = (GameObject)Instantiate(particleSysPrefab, transform.position, particleSysPrefab.transform.rotation);
        Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime * 2);

        this.gameObject.SetActive(false);
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

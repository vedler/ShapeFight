using UnityEngine;
using System.Collections;
using System;

public abstract class AbsWeaponMover : Photon.MonoBehaviour, PoolObject, IWeaponMover {

    public float speed;
    public float maxSpeed;

    protected bool isFired;
    protected Vector2 direction;
    protected Vector2 startPosition;

    protected Rigidbody2D rigidBody;

    public ProjectileSynchronizer sync;

    public abstract void FireMe(Vector2 direction);

    [PunRPC]
    public abstract void TriggerFireMe(Vector2 direction, PhotonMessageInfo info);

    public abstract void move();
    
    [PunRPC]
    public abstract void TriggerMove(PhotonMessageInfo info);

    public abstract void OnObjectReuse();

    public virtual void Reset()
    {
        throw new NotImplementedException();
    }

    // Use this for initialization
    void Awake () {
        sync = GetComponent<ProjectileSynchronizer>();
        rigidBody = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (isFired)
        {
            move();

            if (GetComponent<Rigidbody2D>().velocity.magnitude > maxSpeed)
            {
                Vector2 vel = GetComponent<Rigidbody2D>().velocity;
                vel.Normalize();
                GetComponent<Rigidbody2D>().velocity = vel * maxSpeed; 
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "LocalPlayerTag" || collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
        {
            return;
        }

        this.gameObject.SetActive(false);
        sync.TriggerExploded(rigidBody.position);
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

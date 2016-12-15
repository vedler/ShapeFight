using UnityEngine;
using System.Collections;
using System;

public abstract class AbsWeaponMover : MonoBehaviour, PoolObject, IWeaponMover {

    public float maxSpeed;
    public float cooldownPeriod;
    public GameObject particleSysPrefab;
    private AudioSource[] sounds;


    protected bool isFired;
    protected bool hasEnded;
    protected Vector2 direction;
    protected Vector2 startPosition;
    protected float damage;
    protected float radius;

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
        sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
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
        if (!CompareTag("SoundTriggerTag"))
        {
            if (collider.GetComponent<GrenadeMover>() != null)
                sounds[2].Play();
            else if (collider.GetComponent<RocketMover>() != null)
                sounds[1].Play();
            else if (collider.GetComponent<PelletMover>() != null)
                sounds[0].Play();

            if (collider.gameObject.tag == "LocalPlayerTag" || collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
            {
                return;
            }

            GameObject explosion = (GameObject)Instantiate(particleSysPrefab, transform.position, particleSysPrefab.transform.rotation);
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime * 2);
            PlayerCharacter[] pcs = FindObjectsOfType<PlayerCharacter>();
            foreach (PlayerCharacter pc in pcs)
            {
                if ((pc.transform.position - transform.position).magnitude <= radius)
                    pc.getHit((1 - (pc.transform.position - transform.position).magnitude / 10) * damage);
            }
            this.gameObject.SetActive(false);
        }
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

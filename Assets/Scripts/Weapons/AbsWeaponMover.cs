using UnityEngine;
using System.Collections;
using System;

public abstract class AbsWeaponMover : Photon.MonoBehaviour, PoolObject, IWeaponMover
{
    public float maxSpeed;
    
    protected bool isFired;
    protected bool hasEnded;
    protected Vector2 direction;
    protected Vector2 startPosition;

    [SerializeField]
    public WeaponConfig activeConfig;

    protected Collider2D myCollider;

    protected Rigidbody2D rigidBody;

    [HideInInspector]
    public ProjectileSynchronizer sync;

    public abstract void init();

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
        //sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
        myCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

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
        if (!CompareTag("SoundTriggerTag") && isFired)
        {
            if (collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
            {
                return;
            }

            if (activeConfig.sounds.Length > 0)
            {
                activeConfig.sounds[0].Play();
            }

            if (collider.name.Contains("Pellet"))
            {
                return;
            }

            GameObject explosion = (GameObject)Instantiate(activeConfig.particleSysPrefab, 
                transform.position, 
                activeConfig.particleSysPrefab.transform.rotation);
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime * 2);
            /*PlayerCharacter[] pcs = FindObjectsOfType<PlayerCharacter>();
            foreach (PlayerCharacter pc in pcs)
            {
                if ((pc.transform.position - transform.position).magnitude <= activeConfig.radius)
                {
                    pc.getHit(Mathf.Abs(1 - (pc.transform.position - transform.position).magnitude / activeConfig.radius) * activeConfig.damage);
                    print("1´- magnitude/radius = "+(1 - (pc.transform.position - transform.position).magnitude / 10));
                    print("total damage out of " + activeConfig.damage + ": " + Mathf.Abs(1 - (pc.transform.position - transform.position).magnitude / activeConfig.radius) * activeConfig.damage);
                }
            }*/
            this.gameObject.SetActive(false);
        }
        
        if (collider.gameObject.tag == "LocalPlayerTag")
        {
            PhotonView otherView = collider.gameObject.GetComponent<PhotonView>();

            if (otherView != null)
            {
                sync.TriggerProjectileHit(rigidBody.position, otherView.ownerId);
            }
            else
            {
                sync.TriggerProjectileHit(rigidBody.position, -1);
            }

            return;
        }

        if (collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
        {
            return;
        }

        sync.TriggerProjectileHit(rigidBody.position, -1);
    }
    

    public void SetVelocity(Vector2 velocity)
    {
        rigidBody.velocity = velocity;
        rigidBody.AddForce(velocity, ForceMode2D.Impulse);
    }

    public void SetStartPosition(Vector2 sp)
    {
        startPosition = sp;
    }
}

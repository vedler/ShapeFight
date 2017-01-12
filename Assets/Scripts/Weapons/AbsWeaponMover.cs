using UnityEngine;
using System.Collections;
using System;

public abstract class AbsWeaponMover : Photon.MonoBehaviour, PoolObject, IWeaponMover
{
    public float maxSpeed;
    public WeaponSpawner spawner;
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
        if (photonView.isMine)
        {
            if (hasEnded)
            {
                isFired = false;
                Remove();
            }

            if (isFired)
            {
                move();

                if (rigidBody.velocity.magnitude > maxSpeed)
                {
                    Vector2 vel = rigidBody.velocity;
                    vel.Normalize();
                    rigidBody.velocity = vel * maxSpeed;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!isFired || !sync.active || !photonView.isMine)
        {
            return;
        }

        if (!CompareTag("SoundTriggerTag"))
        {
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
            }
            else if (collider.gameObject.tag == "LocalProjectileTag" || collider.gameObject.tag == "")
            {
                return;
            }

            spawner.playTune(name);
            sync.TriggerProjectileHit(rigidBody.position, -1);

            if (activeConfig.sounds.Length > 0)
            {
                activeConfig.sounds[0].Play();
            }

            Remove();
        }
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

    public void Remove()
    {
        // Move it so it doesnt reappear in the old place
        rigidBody.transform.position = new Vector2(1000, 1000);
        rigidBody.velocity = Vector2.zero;

        sync.active = false;
        gameObject.SetActive(false);
        photonView.RPC("RemoteTriggerRemove", PhotonTargets.All);
    }

    [PunRPC]
    public void RemoteTriggerRemove()
    {
        // Move it so it doesnt reappear in the old place
        rigidBody.transform.position = new Vector2(1000, 1000);
        rigidBody.velocity = Vector2.zero;

        sync.active = false;
        gameObject.SetActive(false);
    }
}

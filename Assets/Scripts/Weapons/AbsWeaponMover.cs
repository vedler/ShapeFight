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
    
    private SpriteRenderer[] renderers;

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

        SpriteRenderer[] _renderers = GetComponentsInChildren<SpriteRenderer>();
        renderers = new SpriteRenderer[_renderers.Length + 1];
        renderers[0] = GetComponent<SpriteRenderer>();
        _renderers.CopyTo(renderers, 1);

        // Request from owner
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
        //gameObject.SetActive(false);
        Disable();
        photonView.RPC("RemoteTriggerRemove", PhotonTargets.All);
    }

    [PunRPC]
    public void RemoteTriggerRemove()
    {
        // Move it so it doesnt reappear in the old place
        rigidBody.transform.position = new Vector2(1000, 1000);
        rigidBody.velocity = Vector2.zero;

        sync.active = false;
        //gameObject.SetActive(false);

        Disable();
    }

    public void Disable()
    {
        if (!myCollider.enabled)
        {
            // We are already disabled
            return;
        }

        rigidBody.transform.position = new Vector2(1000, 1000);
        rigidBody.velocity = Vector2.zero;

        rigidBody.isKinematic = true;

        myCollider.enabled = false;


        foreach (var r in renderers)
        {
            r.enabled = false;
        }
        
        photonView.RPC("RemoteTriggerProjDisable", PhotonTargets.All);
    }

    public void Enable()
    {
        if (myCollider.enabled)
        {
            // We are already enabled
            return;
        }

        rigidBody.isKinematic = false;

        myCollider.enabled = true;

        foreach (var r in renderers)
        {
            r.enabled = true;
        }
        
        photonView.RPC("RemoteTriggerProjEnable", PhotonTargets.All);
    }

    public bool isEnabled()
    {
        return myCollider.enabled;
    }

    [PunRPC]
    public void RemoteTriggerProjDisable(PhotonMessageInfo info)
    {
        rigidBody.transform.position = new Vector2(1000, 1000);
        rigidBody.velocity = Vector2.zero;

        rigidBody.isKinematic = true;

        myCollider.enabled = false;

        foreach (var r in renderers)
        {
            r.enabled = false;
        }
    }

    [PunRPC]
    public void RemoteTriggerProjEnable(PhotonMessageInfo info)
    {
        rigidBody.isKinematic = false;

        myCollider.enabled = true;

        foreach (var r in renderers)
        {
            r.enabled = true;
        }
    }
}

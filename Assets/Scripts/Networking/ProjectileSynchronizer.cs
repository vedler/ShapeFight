using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ProjectileSynchronizer : Photon.MonoBehaviour
{
    [HideInInspector]
    public bool active;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector2 syncStartPosition = Vector2.zero;
    private Vector2 syncEndPosition = Vector2.zero;
    private Vector2 syncStartVelocity = Vector2.zero;
    private Vector2 syncEndVelocity = Vector2.zero;

    private Rigidbody2D rigidBody;
    private Collider2D collider2d;

    private AbsWeaponMover mover;

    void Awake()
    {
        active = false;

        rigidBody = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();
        mover = GetComponent<AbsWeaponMover>();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // If the object doesnt have the correct components, set them up
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (collider2d == null)
        {
            collider2d = GetComponent<Collider2D>();
        }

        if (!active)
        {
            return;
        }

        // It's our view
        if (photonView.isMine)
        {
            if (stream.isWriting)
            {
                stream.SendNext(rigidBody.position);
                stream.SendNext(rigidBody.velocity);
            }
        }
        else
        {
            Vector2 syncPosition = (Vector2)stream.ReceiveNext();
            Vector2 syncVelocity = (Vector2)stream.ReceiveNext();

            syncTime = 0f;
            syncDelay = Time.realtimeSinceStartup - lastSynchronizationTime;
            lastSynchronizationTime = Time.realtimeSinceStartup;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncEndVelocity = syncVelocity;
            syncStartPosition = rigidBody.position;
            syncStartVelocity = rigidBody.velocity;
        }
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (collider2d == null)
        {
            collider2d = GetComponent<Collider2D>();
        }

        if (!photonView.isMine)
        {
            SyncedExternalMovement();
        }
    }

    private void SyncedExternalMovement()
    {
        syncTime += Time.fixedDeltaTime;
        
        rigidBody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        
        //rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, Mathf.Pow(syncTime / syncDelay, 0.5f));
        rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, syncTime / syncDelay);
        //rigidBody.velocity = syncEndVelocity;
    }

    // Reset to position and make visible
    // Exploded
    // Owner migration?

    public void TriggerResetToPosition(Vector2 position, Vector2 velocity, Quaternion rotation)
    {
        active = true;
        gameObject.SetActive(true);

        rigidBody.position = position;
        // Mitigate some activation issues
        rigidBody.transform.position = position;
        rigidBody.velocity = velocity;
        rigidBody.transform.rotation = rotation;
        rigidBody.angularVelocity = 0.0f;

        syncTime = 0f;
        syncDelay = Time.realtimeSinceStartup - lastSynchronizationTime;
        lastSynchronizationTime = Time.realtimeSinceStartup;

        syncEndPosition = position;
        syncEndVelocity = velocity;

        photonView.RPC("ResetToPosition", PhotonTargets.All, position, velocity, rotation);
    }

    [PunRPC]
    public void ResetToPosition(Vector2 position, Vector2 velocity, Quaternion rotation, PhotonMessageInfo info)
    {
        active = true;
        gameObject.SetActive(true);

        rigidBody.position = position;
        // Mitigate some activation issues
        rigidBody.transform.position = position;
        rigidBody.velocity = velocity;
        rigidBody.transform.rotation = rotation;
        rigidBody.angularVelocity = 0.0f;
        syncEndPosition = position;
        syncEndVelocity = velocity;

        syncTime = 0f;
        syncDelay = Time.realtimeSinceStartup - lastSynchronizationTime;
        lastSynchronizationTime = Time.realtimeSinceStartup;

        //syncEndPosition = position + velocity * syncDelay;
        //syncEndVelocity = velocity;
        syncStartPosition = position;
        syncStartVelocity = velocity;
    }

    public void TriggerProjectileHit(Vector2 position, int otherId)
    {

        GameObject explosion = (GameObject)Instantiate(mover.activeConfig.particleSysPrefab,
                transform.position,
                mover.activeConfig.particleSysPrefab.transform.rotation);

        Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime * 2);

        active = false;
        mover.Remove();

        photonView.RPC("RemoteProjectileHit", PhotonTargets.All, position, otherId);
    }

    [PunRPC]
    public void RemoteProjectileHit(Vector2 position, int otherId, PhotonMessageInfo info)
    {
        if (!gameObject.activeSelf)
        {
            print("ayy we inactive");
        }
        GameObject explosion = (GameObject)Instantiate(mover.activeConfig.particleSysPrefab,
                position,
                mover.activeConfig.particleSysPrefab.transform.rotation);

        Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime * 2);

        active = false;
        GameManager.getInstance().getNetworkManager().localPlayerCharacter.handleRemoteHit(position, mover.activeConfig, otherId);
    }

}

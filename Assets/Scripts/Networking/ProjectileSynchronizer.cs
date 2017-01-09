using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ProjectileSynchronizer : Photon.MonoBehaviour
{
    private bool active = false;

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

        RaycastHit hit;

        // Get movement direction vector
        Vector2 rayDir = rigidBody.velocity;
        rayDir.Normalize();

        // Get diagonal size of the square
        Vector2 diagVec = collider2d.bounds.size;

        // Layer mask to only detect hits with the map layer
        // TODO: Player layer
        var layerMask = (1 << LayerMask.NameToLayer("MapLayer"));

        bool collidingSoon = false;
        Vector2 collisionTransform = Vector2.zero;

        if (Physics.Raycast(transform.position, rayDir, out hit, diagVec.magnitude * 3, layerMask))
        {
            Vector3 colliderEdgePoint = collider2d.bounds.ClosestPoint(hit.point);
            collidingSoon = Vector2.Distance(colliderEdgePoint, transform.position) * 2 >= Vector2.Distance(hit.point, transform.position);

            // Calculate the center of the transform
            if (collidingSoon)
            {
                collisionTransform = hit.point - colliderEdgePoint + transform.position;
            }
        }

        if (collidingSoon)
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, collisionTransform, Mathf.Pow(syncTime / syncDelay, 2));
        }
        else
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, Mathf.Pow(syncTime / syncDelay, 2));
        }
        
        rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, Mathf.Pow(syncTime / syncDelay, 2));
    }

    // Reset to position and make visible
    // Exploded
    // Owner migration?

    public void TriggerResetToPosition(Vector2 position, Vector2 velocity, Quaternion rotation)
    {
        active = true;
        gameObject.SetActive(true);
        rigidBody.position = position;
        rigidBody.velocity = velocity;
        rigidBody.transform.rotation = rotation;
        rigidBody.angularVelocity = 0.0f;
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
        rigidBody.velocity = velocity;
        rigidBody.transform.rotation = rotation;
        rigidBody.angularVelocity = 0.0f;
        //syncEndPosition = position;
        //syncEndVelocity = velocity;

        syncTime = 0f;
        syncDelay = Time.realtimeSinceStartup - lastSynchronizationTime;
        lastSynchronizationTime = Time.realtimeSinceStartup;

        syncEndPosition = position + velocity * syncDelay;
        syncEndVelocity = velocity;
        syncStartPosition = position;
        syncStartVelocity = velocity;
    }

    public void TriggerExploded(Vector2 position)
    {
        active = false;
        gameObject.SetActive(false);
        photonView.RPC("Exploded", PhotonTargets.All, position);
    }

    [PunRPC]
    public void Exploded(Vector2 position, PhotonMessageInfo info)
    {
        active = false;
        gameObject.SetActive(false);
    }

}

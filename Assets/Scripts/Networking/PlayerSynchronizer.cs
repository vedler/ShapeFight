using UnityEngine;
using System.Collections;

public class PlayerSynchronizer : Photon.MonoBehaviour {

    // GetComponent<Rigidbody2D>()

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector2 syncStartPosition = Vector2.zero;
    private Vector2 syncEndPosition = Vector2.zero;
    private Vector2 syncStartVelocity = Vector2.zero;
    private Vector2 syncEndVelocity = Vector2.zero;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        if (stream.isWriting)
        {
            stream.SendNext(rigidBody.position);
            stream.SendNext(rigidBody.velocity);
        }
        else
        {
            Vector2 syncPosition = (Vector2)stream.ReceiveNext();
            Vector2 syncVelocity = (Vector2)stream.ReceiveNext();

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

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

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        if (!photonView.isMine)
        {
            SyncedMovement();
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.fixedDeltaTime;
        //GetComponent<Rigidbody2D>().position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        //GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, syncTime / syncDelay);

        RaycastHit hit;

        // Get movement direction vector
        Vector2 rayDir = rigidBody.velocity;
        rayDir.Normalize();

        // Get diagonal size of the square
        Vector2 diagVec = boxCollider.size;

        // Layer mask to only detect hits with the map layer
        // TODO: Player layer
        var layerMask = (1 << LayerMask.NameToLayer("MapLayer"));

        bool collidingSoon = false;
        Vector2 collisionTransform = Vector2.zero;

        if (Physics.Raycast(transform.position, rayDir, out hit, diagVec.magnitude * 3, layerMask))
        {
            Vector3 colliderEdgePoint = boxCollider.bounds.ClosestPoint(hit.point);
            print("closest to coll: " + colliderEdgePoint);
            collidingSoon = Vector2.Distance(colliderEdgePoint, transform.position) * 2 >= Vector2.Distance(hit.point, transform.position);

            // Calculate the center of the transform
            if (collidingSoon)
            {
                print("Colliding soon");
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

        /*if (!collidingSoon)
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, Mathf.Pow(syncTime / syncDelay, 2));
            rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, Mathf.Pow(syncTime / syncDelay, 2));
        }
        else
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, Mathf.Pow(syncTime / syncDelay, 2) / 3);
            rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, Mathf.Pow(syncTime / syncDelay, 2) / 3);
        }
        */
    }
}

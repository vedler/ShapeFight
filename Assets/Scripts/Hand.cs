using UnityEngine;
using System.Collections;
using System;

public class Hand : MonoBehaviour {
    
    private Transform playerTransform;
    public int ticks;
    private PlayerSynchronizer parentSync;

    void Start()
    {

    }

	// Use this for initialization
	void Awake () {
        ticks = 0;
        playerTransform = gameObject.GetComponentInParent<Transform>();
        
    }

   // Update is called once per frame
   void Update () {
        if (parentSync == null)
        {
            parentSync = gameObject.GetComponentInParent<PlayerSynchronizer>();
        }

        if (!parentSync.photonView.isMine)
        {
            InterpRotation();
            return;
        }

        if (playerTransform == null)
        {
            playerTransform = gameObject.GetComponentInParent<Transform>();
        }

        ticks++;
        /* Uncomment if we decide to make the Hand a stand-alone object
         * 
         * transform.position = playerTransform.position 
         */

        // Get target rotation point from mouse, pivot point from self
        Vector2 targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition), 
                myPivot = new Vector3(transform.position.x, transform.position.y);

        // Calculate the right direction
        Vector2 direction = targetPoint - myPivot;
        direction.Normalize();

        // QUATERNION-IFY IT!
        Quaternion rotateTo = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 180);

        // Smoothly rotate towards the target point.
        Quaternion rot = Quaternion.Slerp(transform.rotation, rotateTo, 5f * Time.deltaTime);
        rotate(rot);

        // Throttle the network sendrate a bit
        if (ticks >= 2)
        {
            parentSync.photonView.RPC("SendHandRotation", PhotonTargets.All, rot);
            ticks = 0;
        }
    }

    Quaternion targetRot;

    private void InterpRotation()
    {
        /*if (targetRot == null)
        {
            return;
        }*/

        // Smoothly rotate towards the target point.
        Quaternion rot = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        rotate(rot);
    }

    public void rotate(Quaternion rot)
    {
        transform.rotation = rot;
    }

    public void setTargetRot(Quaternion rot)
    {
        targetRot = rot;
    }
}

using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour {


    private Transform playerTransform;

	// Use this for initialization
	void Start () {
        playerTransform = gameObject.GetComponentInParent<Transform>();
    }

   // Update is called once per frame
   void Update () {

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
        transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, 5f * Time.deltaTime);
             
    }
}

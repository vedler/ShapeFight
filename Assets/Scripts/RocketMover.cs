using UnityEngine;
using System.Collections;

public class RocketMover : MonoBehaviour {
    public float speed;

	void Start () {
        GetComponent<Rigidbody2D>().velocity = transform.forward * speed;
	}
}

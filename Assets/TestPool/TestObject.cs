using UnityEngine;
using System.Collections;

public class TestObject : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.right * Time.deltaTime * 25);
	}
}

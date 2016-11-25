using UnityEngine;
using System.Collections;

public class CamMovement : MonoBehaviour {
    
    private GameObject target;
    
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
        }
    }

    public void setTarget(GameObject target)
    {
        this.target = target;
    }
}

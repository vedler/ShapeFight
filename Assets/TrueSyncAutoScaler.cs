using UnityEngine;
using System.Collections;
using TrueSync;

public class TrueSyncAutoScaler : MonoBehaviour {
    bool done = false;

    // Use this for initialization
    void Start()
    {
        //this.transform.childCount;
        //this.transform.localScale *= 100;
        this.GetComponent<TSTransform2D>().scale *= 100;
        this.GetComponent<TSTransform2D>().position *= 100;
        TSTransform2D[] tsTransforms = this.GetComponentsInChildren<TSTransform2D>();

        foreach (var trans in tsTransforms)
        {
            trans.gameObject.transform.position *= 100;
        }

        /*for (int i = 0; i < transform.childCount; ++i)
        {
            TSTransform2D trans = transform.GetChild(i).gameObject.GetComponent<TSTransform2D>();
            if (trans != null) trans.position *= 100;
        }*/
        print(this.transform.localScale);
    }

    // Use this for initialization
    void Update()
    {
        //this.transform.position *= 10000;
        //print(this.transform.localScale);
        /*if (!done)
        {
            done = true;
            this.transform.localScale *= 100;
        }*/
    }
}

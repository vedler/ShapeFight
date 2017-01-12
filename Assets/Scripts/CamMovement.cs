using UnityEngine;
using System.Collections;

public class CamMovement : MonoBehaviour {

    //[SerializeField]
    private GameObject target;

    private float defaultZoom;
    private Vector3 defaultPos;
    [HideInInspector]
    public bool deathCam;
    private bool isZoomReset;

    [SerializeField]
    private float deathcamZoomPerSecond;

    private Camera mainCamera;

    // Use this for initialization
    void Start()
    {
        isZoomReset = true;
        defaultPos = transform.position;
        mainCamera = gameObject.GetComponent<Camera>();
        defaultZoom = mainCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (!deathCam)
        {
            if (!isZoomReset)
            {
                mainCamera.orthographicSize = defaultZoom;
                isZoomReset = true;
            }

            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Zoom out
        if (deathCam)
        {
            transform.position = defaultPos;
            isZoomReset = false;
            mainCamera.orthographicSize += deathcamZoomPerSecond * Time.fixedDeltaTime;
        }
    }
    
    public void setTarget(GameObject target)
    {
        this.target = target;
    }
}

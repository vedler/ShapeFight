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

    private readonly float MAX_SHAKE_DISTANCE = 20f;

    [SerializeField]
    private float deathcamZoomPerSecond;

    private Camera mainCamera;

    // How long the object should shake for.
    [HideInInspector]
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;
    
    [HideInInspector]
    public float shakeMod = 1f;

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

        
    }

    void FixedUpdate()
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

        // Zoom out
        else
        {
            transform.position = defaultPos;
            isZoomReset = false;
            mainCamera.orthographicSize += deathcamZoomPerSecond * Time.fixedDeltaTime;
        }

        // Cam shake
        if (shakeDuration > 0)
        {
            transform.position = transform.position + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
        }
    }
    
    public void setTarget(GameObject target)
    {
        this.target = target;
    }
    
    public void SetShakeFor(float duration, Vector2 explosionPos)
    {
        shakeDuration = duration;

        float dist = (transform.position - new Vector3(explosionPos.x, explosionPos.y)).magnitude;

        if (dist <= MAX_SHAKE_DISTANCE)
        {
            shakeMod = 1.0f - (dist / MAX_SHAKE_DISTANCE);
        }
    }
    
}

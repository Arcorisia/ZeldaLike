using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class AutoCameraZoom : MonoBehaviour
{
    [Header("Camera")]
    public CinemachineCamera cinemachineCamera;

    [Header("Orthographic Size")]
    public float zoomInSize = 1f;
    public float zoomOutSize = 4.3f;

    [Header("Timers")]
    public float idleTime = 6f;
    public float moveDelay = 1f;

    [Header("Zoom Speed")]
    public float zoomSpeed = 2f;

    [Header("Movement Detection")]
    public float movementThreshold = 0.01f;

    private Vector3 lastPosition;
    private float idleTimer = 0f;
    private bool isZoomedIn = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();

        lastPosition = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        // Detecta movimiento
        if (distance > movementThreshold)
        {
            idleTimer = 0f;
            lastPosition = transform.position;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(ZoomOutAfterDelay());
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTime && !isZoomedIn)
            {
                isZoomedIn = true;
            }
        }

        // Zoom suave
        if (cinemachineCamera.Lens.Orthographic)
        {
            float targetSize = isZoomedIn ? zoomInSize : zoomOutSize;

            LensSettings lens = cinemachineCamera.Lens;
            lens.OrthographicSize = Mathf.Lerp(
                lens.OrthographicSize,
                targetSize,
                Time.deltaTime * zoomSpeed);

            cinemachineCamera.Lens = lens;
        }
    }

    IEnumerator ZoomOutAfterDelay()
    {
        yield return new WaitForSeconds(moveDelay);

        isZoomedIn = false;
    }
}
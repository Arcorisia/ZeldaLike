using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopDownCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("References")]
    [SerializeField] private Camera isoCamera;
    [Tooltip("Assign a child transform that contains the visible mesh if the model pivot is offset from the collider.")]
    [SerializeField] private Transform visualRoot;
    [Tooltip("Rotation offset used when the model forward axis is not aligned with the local Z axis.")]
    [SerializeField] private Vector3 meshRotationOffset = Vector3.zero;

    private Rigidbody _rb;
    private Vector3 _moveDirection = Vector3.zero;
    private Quaternion _visualRotationOffset = Quaternion.identity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = visualRoot != null
            ? RigidbodyConstraints.FreezeRotation
            : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (isoCamera == null)
            isoCamera = Camera.main;

        if (visualRoot != null)
        {
            _visualRotationOffset = Quaternion.Euler(meshRotationOffset);
            visualRoot.rotation = transform.rotation * _visualRotationOffset;
        }
    }

    public void Move(Vector2 input)
    {
        if (isoCamera == null)
        {
            isoCamera = Camera.main;
            if (isoCamera == null)
                return;
        }

        Vector3 forward = isoCamera.transform.forward;
        Vector3 right = isoCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = right * input.x + forward * input.y;
        _moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.zero;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 velocity = _moveDirection * moveSpeed;
        velocity.y = _rb.linearVelocity.y + Physics.gravity.y * Time.fixedDeltaTime;
        _rb.linearVelocity = velocity;
    }

    private void HandleRotation()
    {
        if (_moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
        Quaternion nextRotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(nextRotation);
    }
}

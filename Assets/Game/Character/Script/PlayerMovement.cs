using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Config field (modify on your own)
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private float _sprintSpeed;
    [SerializeField]
    private float _walkSprintTransition;
    private float _speed;
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private InputManager _input;
    [SerializeField]
    private Transform _groundDetector;
    [SerializeField]
    private float _detectorRadius;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private Vector3 _upperStepOffset;
    [SerializeField]
    private float _stepCheckerDistance;
    [SerializeField]
    private float _stepForce;
    [SerializeField]
    private Transform _climbDetector;
    [SerializeField]
    private float _climbCheckDistance;
    [SerializeField]
    private LayerMask _climbableLayer;
    [SerializeField]
    private Vector3 _climbOffset;
    [SerializeField]
    private float _climbSpeed;

    private Rigidbody _rigidbody;
    private bool _isGrounded;
    private PlayerStance _playerStance;

    // Function active when start
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;
    }

    // Start function
    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
    }

    // Stop function, when object destroy
    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
    }

    // check method everytime
    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
    }

    // Method & Function
    private void Move(Vector2 axisDirection) // Character movement axis
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;

        if (isPlayerStanding)
        {
            if (axisDirection.magnitude >= 0.1)
            {
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
            }
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection = horizontal + vertical;
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
        }
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (_speed < _sprintSpeed)
            {
                _speed = _speed + _walkSprintTransition * Time.deltaTime;
            }
        }
        else
        {
            if (_speed > _walkSpeed)
            {
                _speed = _speed - _walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce, ForceMode.Impulse); // Use ForceMode for continuous jumps; no need to multiply by deltaTime. minimize value of jumpForce 
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
    }

    // Step stairs and ramp
    private void CheckStep() 
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);

        if (isHitLowerStep && !isHitUpperStep)
        {
            _rigidbody.AddForce(0, _stepForce * Time.deltaTime, 0);
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position,
                                                        transform.forward,
                                                        out RaycastHit hit,
                                                        _climbCheckDistance,
                                                        _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;

        if (isInFrontOfClimbingWall && _isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
        }
    }

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 1f;
        }
    }

    // Measuring climb detector
    private void OnDrawGizmosSelected()
    {
        if (_climbDetector == null) return;

        // Visual raycast: front, top, bottom, right, left
        DrawClimbRay(_climbDetector.position, _climbDetector.forward, _climbCheckDistance);
        DrawClimbRay(_climbDetector.position, transform.up, _climbCheckDistance);
        DrawClimbRay(_climbDetector.position, -transform.up, _climbCheckDistance);
        DrawClimbRay(_climbDetector.position, transform.right, _climbCheckDistance);
        DrawClimbRay(_climbDetector.position, -transform.right, _climbCheckDistance);
    }

    private void DrawClimbRay(Vector3 origin, Vector3 direction, float distance)
    {
        bool hitWall = Physics.Raycast(origin, direction, out RaycastHit hit, distance, _climbableLayer);
        Gizmos.color = hitWall ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + direction * distance);
        if (hitWall) Gizmos.DrawSphere(hit.point, 0.05f);
    }
}

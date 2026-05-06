/*
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private float _verticalVelocity;

    private void Awake() => _cc = GetComponent<CharacterController>();

    private void Update()
    {
        if (!IsOwner) return;

        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null && !playerNetwork.IsAlive.Value) return;


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v).normalized * _speed;

        _verticalVelocity += _gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);

        if (_cc.isGrounded) _verticalVelocity = 0f;
    }
}
*/



/*
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    private CharacterController _cc;
    private float _verticalVelocity;

    // Для CSP
    private MoveData _lastMoveData;

    private void Awake() => _cc = GetComponent<CharacterController>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Подписка на тики для CSP
        if (base.IsOwner)
        {
            base.TimeManager.OnTick += OnTick;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.IsOwner)
        {
            base.TimeManager.OnTick -= OnTick;
        }
    }

    private void OnTick()
    {
        if (!base.IsOwner) return;

        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null && !playerNetwork.IsAlive) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        MoveData md = new MoveData
        {
            Horizontal = h,
            Vertical = v,
            Tick = base.TimeManager.LocalTick
        };

        _lastMoveData = md;
        ReplicateMovement(md);
    }

    [Replicate]
    private void ReplicateMovement(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (base.IsOwner)
        {
            ApplyMovement(md.Horizontal, md.Vertical);
        }
    }

    [Reconcile]
    private void ReconcileMovement(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _verticalVelocity = rd.VerticalVelocity;
    }

    private void ApplyMovement(float h, float v)
    {
        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null && !playerNetwork.IsAlive) return;

        Vector3 move = new Vector3(h, 0f, v).normalized * _speed;

        _verticalVelocity += _gravity * (float)base.TimeManager.TickDelta;
        move.y = _verticalVelocity;

        _cc.Move(move * (float)base.TimeManager.TickDelta);

        if (_cc.isGrounded) _verticalVelocity = 0f;

        // Отправка данных на сервер для Reconcile
        if (base.IsServerInitialized)
        {
            ReconcileData rd = new ReconcileData
            {
                Position = transform.position,
                VerticalVelocity = _verticalVelocity,
                Tick = base.TimeManager.LocalTick
            };
            ReconcileMovement(rd);
        }
    }

    // Стандартное Update для не-CSP fallback
    private void Update()
    {
        if (!base.IsOwner) return;
        // CSP обрабатывается в OnTick
    }
}

// Структуры для CSP
public struct MoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;
    public uint Tick;

    public void Dispose() { }
    public uint GetTick() => Tick;
    public void SetTick(uint value) => Tick = value;
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public float VerticalVelocity;
    public uint Tick;

    public void Dispose() { }
    public uint GetTick() => Tick;
    public void SetTick(uint value) => Tick = value;
}
*/

using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Camera Follow Rotation")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private bool _smoothRotation = true;
    [SerializeField] private float _rotationThreshold = 5f;

    [Header("Camera Reference")]
    [SerializeField] private Camera _playerCamera;

    private CharacterController _cc;
    private float _verticalVelocity;
    private float _targetYRotation;

    private void Awake() => _cc = GetComponent<CharacterController>();

    private void Update()
    {
        if (!IsOwner) return;

        var playerNetwork = GetComponent<PlayerNetwork>();
        if (playerNetwork != null && !playerNetwork.IsAlive.Value) return;

        UpdateRotationFromCamera();
        HandleMovement();
    }

    private void UpdateRotationFromCamera()
    {
        Camera cam = GetActiveCamera();
        if (cam == null) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;

        float angleDiff = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);

        if (Mathf.Abs(angleDiff) < _rotationThreshold) return;

        if (_smoothRotation)
        {
            _targetYRotation = Mathf.MoveTowardsAngle(
                _targetYRotation,
                targetAngle,
                _rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            _targetYRotation = targetAngle;
        }

        transform.rotation = Quaternion.Euler(0f, _targetYRotation, 0f);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f)
        {
            ApplyGravityOnly();
            return;
        }

        Camera cam = GetActiveCamera();
        Vector3 camForward = cam != null ? cam.transform.forward : transform.forward;
        Vector3 camRight = cam != null ? cam.transform.right : transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 moveDirection = (camForward * v + camRight * h).normalized;

        _verticalVelocity += _gravity * Time.deltaTime;
        if (_cc.isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;

        Vector3 velocity = moveDirection * _speed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);
    }

    private void ApplyGravityOnly()
    {
        _verticalVelocity += _gravity * Time.deltaTime;
        if (_cc.isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;

        _cc.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
    }

    private Camera GetActiveCamera()
    {
        if (_playerCamera != null && _playerCamera.gameObject.activeInHierarchy)
            return _playerCamera;

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null && cam.gameObject.activeInHierarchy)
            return cam;

        return Camera.main;
    }

    public void SetRotationSpeed(float speed) => _rotationSpeed = speed;

    public void ResetVerticalVelocity() => _verticalVelocity = 0f;

    public void SnapRotationToCamera()
    {
        Camera cam = GetActiveCamera();
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _targetYRotation = angle;
        }
    }
}
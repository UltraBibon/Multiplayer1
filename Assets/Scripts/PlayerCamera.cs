
/*
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);
    [SerializeField] private Camera _cameraPrefab;

    private Camera _cam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        _cam = GetComponentInChildren<Camera>();
        if (_cam == null && _cameraPrefab != null)
        {
            var camInstance = Instantiate(_cameraPrefab, transform);
            camInstance.transform.localPosition = Vector3.zero;
            camInstance.transform.localRotation = Quaternion.identity;
            _cam = camInstance;
        }
        else if (_cam == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform, false);
            camObj.transform.localPosition = _offset;
            _cam = camObj.AddComponent<Camera>();
        }

        _cam.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (_cam == null || transform == null) return;
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position);
    }
}
*/


/*
using FishNet.Object;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);
    [SerializeField] private Camera _cameraPrefab;

    private Camera _cam;

    public override void OnStartNetwork()
    {
        if (!base.IsOwner)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        _cam = GetComponentInChildren<Camera>();
        if (_cam == null && _cameraPrefab != null)
        {
            var camInstance = Instantiate(_cameraPrefab, transform);
            camInstance.transform.localPosition = Vector3.zero;
            camInstance.transform.localRotation = Quaternion.identity;
            _cam = camInstance;
        }
        else if (_cam == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform, false);
            camObj.transform.localPosition = _offset;
            _cam = camObj.AddComponent<Camera>();
        }

        _cam.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (_cam == null || transform == null) return;
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position);
    }
}
*/

using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);
    [SerializeField] private Camera _cameraPrefab;

    [Header("Rotation Settings")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minVerticalAngle = -10f;
    [SerializeField] private float _maxVerticalAngle = 80f;
    [SerializeField] private bool _invertY = false;

    [Header("Smooth Follow")]
    [SerializeField] private bool _useSmoothFollow = true;
    [SerializeField] private float _smoothSpeed = 10f;

    private Camera _cam;
    private float _horizontalRotation = 0f;
    private float _verticalRotation = 0f;
    private Vector3 _currentVelocity;

    // В FishNet используем OnStartClient
    public override void OnStartClient()
    {
        if (!IsOwner)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        _cam = GetComponentInChildren<Camera>();
        if (_cam == null && _cameraPrefab != null)
        {
            var camInstance = Instantiate(_cameraPrefab, transform);
            camInstance.transform.localPosition = Vector3.zero;
            camInstance.transform.localRotation = Quaternion.identity;
            _cam = camInstance;
        }
        else if (_cam == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform, false);
            _cam = camObj.AddComponent<Camera>();
        }

        InitializeRotationFromOffset();

        _cam.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void InitializeRotationFromOffset()
    {
        Vector3 offset = _offset;
        float distance = offset.magnitude;

        _verticalRotation = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;
        _horizontalRotation = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
    }

    private void LateUpdate()
    {
        if (_cam == null || transform == null || !IsOwner) return;

        HandleMouseInput();
        UpdateCameraPosition();
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        if (_invertY) mouseY *= -1;

        _horizontalRotation += mouseX;
        _verticalRotation -= mouseY;

        _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        float radH = _horizontalRotation * Mathf.Deg2Rad;
        float radV = _verticalRotation * Mathf.Deg2Rad;

        float distance = _offset.magnitude;

        Vector3 targetOffset = new Vector3(
            distance * Mathf.Sin(radH) * Mathf.Cos(radV),
            distance * Mathf.Sin(radV),
            distance * Mathf.Cos(radH) * Mathf.Cos(radV)
        );

        Vector3 targetPosition = transform.position + targetOffset;

        if (_useSmoothFollow)
        {
            _cam.transform.position = Vector3.SmoothDamp(
                _cam.transform.position,
                targetPosition,
                ref _currentVelocity,
                1f / _smoothSpeed
            );
        }
        else
        {
            _cam.transform.position = targetPosition;
        }

        _cam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    public void RotateCamera(float horizontal, float vertical)
    {
        if (!IsOwner) return;

        _horizontalRotation += horizontal * _mouseSensitivity;
        _verticalRotation -= vertical * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);
    }

    public void ResetCameraRotation()
    {
        InitializeRotationFromOffset();
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDestroy()
    {
        if (IsOwner && Application.isPlaying)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
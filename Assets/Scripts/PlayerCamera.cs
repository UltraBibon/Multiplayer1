/*
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerNetwork))]
public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private float _lookSpeed = 2f;

    [Header("References")]
    [SerializeField] private Camera _playerCamera;

    private Vector3 _currentOffset;
    private float _rotationX = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        if (_playerCamera == null)
            _playerCamera = Camera.main;

        if (_playerCamera != null)
        {
            _currentOffset = _offset;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void LateUpdate()
    {
        if (_playerCamera == null) return;
        if (!IsOwner) return;

        Vector3 desiredPosition = transform.position + _currentOffset;
        Vector3 smoothedPosition = Vector3.Lerp(
            _playerCamera.transform.position,
            desiredPosition,
            _smoothSpeed
        );
        _playerCamera.transform.position = smoothedPosition;

        HandleMouseLook();

        _playerCamera.transform.LookAt(transform.position);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * _lookSpeed;

        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

        Quaternion verticalRotation = Quaternion.Euler(_rotationX, 0f, 0f);

        Quaternion horizontalRotation = Quaternion.Euler(0f, mouseX, 0f);
        transform.Rotate(Vector3.up, mouseX);
    }
}
*/


/*
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerNetwork))]
public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f);
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minVerticalAngle = -45f;
    [SerializeField] private float _maxVerticalAngle = 85f;

    [Header("References")]
    [SerializeField] private Transform _cameraTarget; // Пустой объект на уровне глаз игрока

    private Camera _cam;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Камера работает ТОЛЬКО у владельца
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Инициализация камеры
        if (_cameraTarget == null)
        {
            // Создаём целевую точку автоматически
            GameObject target = new GameObject("CameraTarget");
            target.transform.SetParent(transform, false);
            target.transform.localPosition = new Vector3(0f, 1.6f, 0f); // Уровень глаз
            _cameraTarget = target.transform;
        }

        _cam = GetComponentInChildren<Camera>();
        if (_cam == null)
        {
            Debug.LogError("[PlayerCamera] Camera not found in children!");
            enabled = false;
            return;
        }

        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Синхронизируем начальные углы
        _rotationY = transform.eulerAngles.y;

        Debug.Log("[PlayerCamera] Initialized for owner");
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner || _cam == null || _cameraTarget == null) return;

        HandleMouseLook();
        HandleCameraFollow();
    }

    private void HandleMouseLook()
    {
        // Горизонтальное вращение (вращаем игрока)
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        _rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, _rotationY, 0f);

        // Вертикальное вращение (вращаем только камеру)
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, _minVerticalAngle, _maxVerticalAngle);

        // Применяем вертикальное вращение к камере относительно цели
        Quaternion verticalRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        _cam.transform.localRotation = verticalRotation;
    }

    private void HandleCameraFollow()
    {
        // Позиция камеры относительно цели
        Vector3 desiredPosition = _cameraTarget.position + Quaternion.Euler(_rotationX, _rotationY, 0f) * _offset;

        // Плавное следование
        Vector3 smoothedPosition = Vector3.Lerp(
            _cam.transform.position,
            desiredPosition,
            _smoothSpeed
        );
        _cam.transform.position = smoothedPosition;

        // Камера смотрит на цель
        _cam.transform.LookAt(_cameraTarget);
    }
}*/



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
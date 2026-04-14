/*
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerNetwork))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpForce = 2f;

    [Header("References")]
    [SerializeField] private PlayerNetwork _playerNetwork;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_playerNetwork != null && !_playerNetwork.GetIsAlive())
            return;

        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = move.normalized * _moveSpeed;

        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
        }

        _characterController.Move(move * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; 
        }

        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
*/


//вторая версия

/*
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerNetwork))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpForce = 2f;

    private CharacterController _characterController;
    private PlayerNetwork _playerNetwork;
    private Vector3 _velocity;
    private bool _isGrounded;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();

        // Критически важно: проверяем компоненты
        if (_characterController == null)
        {
            Debug.LogError("[PlayerMovement] CharacterController not found!");
            enabled = false;
        }
        if (_playerNetwork == null)
        {
            Debug.LogError("[PlayerMovement] PlayerNetwork not found!");
            enabled = false;
        }
    }

    private void Start()
    {
        // Дополнительная проверка после сетевого спавна
        if (IsOwner)
        {
            Debug.Log($"[PlayerMovement] Owner initialized: ClientID={OwnerClientId}");
        }
        else
        {
            Debug.Log($"[PlayerMovement] Remote player: OwnerClientId={OwnerClientId}, MyClientId={NetworkManager.Singleton?.LocalClientId}");
        }
    }

    private void Update()
    {
        // Критическая проверка: обрабатываем ввод ТОЛЬКО если мы владелец
        if (!IsOwner)
        {
            // Для отладки: раскомментируйте чтобы видеть кто есть кто
            // Debug.Log($"[PlayerMovement] Skipping update: IsOwner={IsOwner}, NetworkObjectId={NetworkObjectId}");
            return;
        }

        // Мёртвый игрок не двигается
        if (_playerNetwork != null && !_playerNetwork.GetIsAlive())
            return;

        // Проверяем что контроллер активен
        if (!_characterController.enabled)
            return;

        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Создаём вектор движения в локальном пространстве игрока
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Нормализуем чтобы по диагонали не было быстрее
        if (move.magnitude > 1f)
            move.Normalize();

        move *= _moveSpeed;

        _isGrounded = _characterController.isGrounded;

        // Прыжок только на земле
        if (_isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravity);
        }

        // Применяем движение
        _characterController.Move(move * Time.deltaTime);
    }

    private void HandleGravity()
    {
        // Если на земле и падаем - прижимаем к земле
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // Применяем гравитацию
        _velocity.y += _gravity * Time.deltaTime;

        // Применяем вертикальную скорость
        _characterController.Move(_velocity * Time.deltaTime);
    }

    // === ОТЛАДОЧНЫЙ МЕТОД ===
    private void OnDrawGizmosSelected()
    {
        if (!IsOwner) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
*/

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

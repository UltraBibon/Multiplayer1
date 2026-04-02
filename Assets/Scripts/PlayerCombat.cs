/*
using Unity.Netcode;
using UnityEngine;
// Подключаем пространство имен нового инпута
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private Camera _mainCamera;

    [Header("Combat Settings")]
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private LayerMask _playerLayer;

    // Ссылка на сгенерированный класс действий
    private PlayerActions _inputActions;

    // Ссылка на конкретное действие атаки
    private InputAction _attackAction;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        // Инициализация системы ввода
        _inputActions = new PlayerActions();

        // Получаем действие "Attack" из карты "Gameplay"
        _attackAction = _inputActions.Gameplay.Attack;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Включаем действие только если этот объект принадлежит локальному игроку
        // Клиенты не должны обрабатывать ввод чужих персонажей
        if (IsOwner)
        {
            _attackAction.Enable();
            // Подписываемся на событие нажатия (срабатывает один раз при нажатии)
            _attackAction.performed += OnAttackPerformed;

            Debug.Log($"[Input] Ввод активирован для игрока {_playerNetwork.Nickname.Value}");
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner && _attackAction != null)
        {
            _attackAction.performed -= OnAttackPerformed;
            _attackAction.Disable();
        }
    }

    // Обработчик события нажатия кнопки атаки
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Дополнительная проверка на всякий случай
        if (!IsOwner) return;

        TryAttack();
    }

    private void TryAttack()
    {
        Debug.Log($"[Combat] Игрок {_playerNetwork.Nickname.Value} пытается атаковать (Input System)");

        // Raycast от центра экрана (для шутера от 3-го лица можно использовать камеру за спиной)
        // Если у вас вид сверху или изометрия, можно использовать ScreenPointToRay с позицией мыши
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, _attackRange, _playerLayer))
        {
            PlayerNetwork target = hit.collider.GetComponent<PlayerNetwork>();

            if (target != null)
            {
                // Отправляем запрос на сервер
                DealDamageServerRpc(target.NetworkObjectId, _damage);

                // Опционально: визуальный эффект выстрела/удара
                // Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            Debug.Log("[Combat] Промах или цель вне радиуса/слоя");
        }
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        // Логика сервера остается без изменений
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
        {
            Debug.LogWarning("[Server] Цель не найдена в SpawnedObjects");
            return;
        }

        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();

        if (targetPlayer == null)
        {
            Debug.LogWarning("[Server] У цели нет компонента PlayerNetwork");
            return;
        }

        if (targetPlayer == _playerNetwork)
        {
            Debug.LogWarning("[Server] Игрок попытался атаковать сам себя!");
            return;
        }

        targetPlayer.TakeDamage(damage);
    }
}*/


using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(_attackKey))
        {
            TryAttack();
        }
    }

    public void TryAttack()
    {
        if (!IsOwner) return;

        PlayerNetwork target = FindNearestTarget();

        if (target == null)
        {
            Debug.Log("[PlayerCombat] Цель не найдена");
            return;
        }

        if (target == _playerNetwork)
        {
            Debug.Log("[PlayerCombat] Нельзя атаковать себя!");
            return;
        }

        Debug.Log($"[PlayerCombat] Атака по: {target.Nickname.Value}");
        DealDamageServerRpc(target.NetworkObjectId, _damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
        {
            Debug.LogWarning("[ServerRpc] Цель не найдена");
            return;
        }

        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();

        if (targetPlayer == null || targetPlayer == _playerNetwork)
        {
            Debug.LogWarning("[ServerRpc] Некорректная цель");
            return;
        }

        targetPlayer.TakeDamage(damage);
    }

    private PlayerNetwork FindNearestTarget()
    {
        PlayerNetwork nearest = null;
        float nearestDistance = _attackRange;

        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (spawnedObject == NetworkObject) continue;

            PlayerNetwork otherPlayer = spawnedObject.GetComponent<PlayerNetwork>();
            if (otherPlayer == null) continue;

            float distance = Vector3.Distance(transform.position, spawnedObject.transform.position);

            if (distance <= nearestDistance)
            {
                nearest = otherPlayer;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
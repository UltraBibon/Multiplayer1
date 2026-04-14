/*
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Никнейм: читают все, пишет только сервер
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Здоровье: читают все, пишет только сервер
    public NetworkVariable<int> HP = new(
        100, // Стартовое значение
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[PlayerNetwork] Спавн игрока. Owner: {IsOwner}, ClientID: {OwnerClientId}");

        if (IsOwner)
        {
            // Только владелец отправляет свой ник на сервер
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает в NetworkVariable
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerClientId}"
            : nickname.Trim();

        Nickname.Value = safeValue;
        Debug.Log($"[Server] Установлен ник: {safeValue} для клиента {OwnerClientId}");
    }

    // Публичный метод для получения урона (вызывается только сервером)
    public void TakeDamage(int damage)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[PlayerNetwork] TakeDamage вызван не на сервере!");
            return;
        }

        int nextHp = Mathf.Max(0, HP.Value - damage);
        HP.Value = nextHp;
        Debug.Log($"[Server] Игрок {Nickname.Value} получил {damage} урона. HP: {HP.Value}");

        if (HP.Value <= 0)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log($"[Server] Игрок {Nickname.Value} погиб!");
        // Здесь можно добавить логику респавна или удаления
    }
}
*/


/*

using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        new FixedString32Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[PlayerNetwork] Спавн игрока. IsOwner: {IsOwner}, OwnerClientId: {OwnerClientId}");

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Debug.Log($"[PlayerNetwork] Деспаун игрока: {Nickname.Value}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerClientId}"
            : nickname.Trim();

        Nickname.Value = new FixedString32Bytes(safeValue);
        Debug.Log($"[ServerRpc] Ник установлен: {Nickname.Value}");
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[PlayerNetwork] TakeDamage вызван не на сервере!");
            return;
        }

        int nextHp = Mathf.Max(0, HP.Value - damage);
        HP.Value = nextHp;

        Debug.Log($"[PlayerNetwork] {Nickname.Value} получил урон. HP: {HP.Value}");
    }
}

*/




//вторая версия

/*
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Network Variables")]
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        new FixedString32Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server

    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Respawn Settings")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 3f;

    [Header("References")]
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private Canvas _deathUI;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }

        Debug.Log($"[PlayerNetwork] Spawned: {Nickname.Value}, Alive: {IsAlive.Value}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerClientId}"
            : nickname.Trim();

        Nickname.Value = new FixedString32Bytes(safeValue);
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[PlayerNetwork] TakeDamage called not on server!");
            return;
        }

        if (!IsAlive.Value) return; 

        int nextHp = Mathf.Max(0, HP.Value - damage);
        HP.Value = nextHp;

        Debug.Log($"[PlayerNetwork] {Nickname.Value} took {damage} dmg. HP: {HP.Value}");
    }

    public void Heal(int amount)
    {
        if (!IsServer) return;
        if (!IsAlive.Value) return;

        int nextHp = Mathf.Min(100, HP.Value + amount);
        HP.Value = nextHp;
    }


    /*
    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            Die();
        }
    }
    

    private void OnHpChanged(int prev, int next)
    {
        Debug.Log($"[PlayerNetwork] HP changed: {prev} → {next}, IsServer: {IsServer}, IsAlive: {IsAlive.Value}");

        // Только сервер управляет циклом смерти
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            Debug.Log($"[PlayerNetwork] Player {Nickname.Value} died! Starting death sequence.");
            Die();
        }
    }

    /*
    private void Die()
    {
        IsAlive.Value = false;
        Debug.Log($"[PlayerNetwork] {Nickname.Value} died!");
    }
    

    private void Die()
    {
        IsAlive.Value = false;

        // Останавливаем физику мёртвого игрока
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Debug.Log($"[PlayerNetwork] {Nickname.Value} is now dead. Starting respawn in {_respawnDelay}s");

        // Запускаем респавн ТОЛЬКО если ещё не запущен
        if (!IsInvoking(nameof(RespawnAfterDelay)))
        {
            Invoke(nameof(RespawnAfterDelay), _respawnDelay);
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        UpdateVisualState();

        if (IsOwner && _deathUI != null)
        {
            _deathUI.gameObject.SetActive(!next);
        }

        if (next && IsServer)
        {
            Respawn();
        }
    }

    
    private void UpdateVisualState()
    {
        if (_playerModel != null)
            _playerModel.SetActive(IsAlive.Value);
    }
    

    private void UpdateVisualState()
    {
        if (_playerModel != null)
            _playerModel.SetActive(IsAlive.Value);

        // Обновляем коллайдеры
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = IsAlive.Value;
    }

    
    private void Respawn()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            transform.position = Vector3.zero;
        }
        else
        {
            int idx = Random.Range(0, _spawnPoints.Length);
            transform.position = _spawnPoints[idx].position;
        }

        HP.Value = 100;
        Debug.Log($"[PlayerNetwork] {Nickname.Value} respawned!");
    }
    

    private void Respawn()
    {
        // Выбираем точку респавна
        Vector3 respawnPos = Vector3.zero;

        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, _spawnPoints.Length);
            respawnPos = _spawnPoints[idx].position;
            Debug.Log($"[PlayerNetwork] Respawn at point {idx}: {respawnPos}");
        }

        // Телепортируем игрока
        transform.position = respawnPos;

        // Сбрасываем скорость и гравитацию
        //_velocity = Vector3.zero;

        // Восстанавливаем контроллер
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        // Восстанавливаем здоровье
        HP.Value = 100;

        // Уведомляем визуальные компоненты
        UpdateVisualState();
    }

    private void RespawnAfterDelay()
    {
        // Двойная проверка что игрок ещё мёртв
        if (!IsAlive.Value)
        {
            Respawn();
            IsAlive.Value = true;
            Debug.Log($"[PlayerNetwork] {Nickname.Value} respawned!");
        }
    }





    [ServerRpc]
    public void RequestRespawnServerRpc()
    {
        if (!IsServer) return;
        if (IsAlive.Value) return; 

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(_respawnDelay);

        if (!IsAlive.Value) 
        {
            Respawn();
            IsAlive.Value = true;
        }
    }
    public bool GetIsAlive() => IsAlive.Value;
    public int GetHP() => HP.Value;
}

*/


//не работает респаун

/*
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _playerModel;

    public override void OnNetworkSpawn()
    {
        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }

    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;
        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        int idx = Random.Range(0, _spawnPoints.Length);
        transform.position = _spawnPoints[idx].position;

        HP.Value = 100;
        IsAlive.Value = true;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (_playerModel != null)
            _playerModel.SetActive(next);
    }
}
*/


/*
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private GameObject _playerModel;

    private Transform[] _spawnPoints;
    private Coroutine _respawnCoroutine;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerNetwork] OnNetworkSpawn - OwnerClientId: {OwnerClientId}");

        FindSpawnPoints();

        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        if (IsServer && _spawnPoints != null && _spawnPoints.Length > 0)
        {
            transform.position = _spawnPoints[0].position;
            Debug.Log($"[PlayerNetwork] Spawned at {_spawnPoints[0].position}");
        }

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        _spawnPoints = spawns.Select(go => go.transform).ToArray();

        if (_spawnPoints.Length == 0)
        {
            Debug.LogWarning("[PlayerNetwork] No spawn points found with tag 'SpawnPoint'!");
            _spawnPoints = new Transform[] { transform };
        }
        else
        {
            Debug.Log($"[PlayerNetwork] Found {_spawnPoints.Length} spawn points");
        }
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;

        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }

    private void OnHpChanged(int prev, int next)
    {
        Debug.Log($"[PlayerNetwork] HP changed: {prev} -> {next}, IsServer: {IsServer}, IsAlive: {IsAlive.Value}");

        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            Debug.Log("[PlayerNetwork] Player died!");
            Die();
        }
    }

    private void Die()
    {
        IsAlive.Value = false;

        if (_respawnCoroutine != null)
            StopCoroutine(_respawnCoroutine);

        _respawnCoroutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log("[PlayerNetwork] RespawnRoutine: Waiting 3 seconds...");
        yield return new WaitForSeconds(3f);
        Debug.Log("[PlayerNetwork] RespawnRoutine: Respawning now");

        Vector3 spawnPosition = Vector3.zero;

        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, _spawnPoints.Length);
            spawnPosition = _spawnPoints[idx].position;
            Debug.Log($"[PlayerNetwork] Using spawn point {idx} at {spawnPosition}");
        }

        transform.position = spawnPosition;
        HP.Value = 100;
        IsAlive.Value = true;

        Debug.Log("[PlayerNetwork] RESPAWN COMPLETE");
        _respawnCoroutine = null;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        Debug.Log($"[PlayerNetwork] IsAlive changed: {prev} -> {next}");

        if (_playerModel != null)
            _playerModel.SetActive(next);

        var cc = GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = next;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = next;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
            shooting.enabled = next;
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[PlayerNetwork] K pressed - killing player");
            if (IsServer)
            {
                HP.Value = 0;
            }
            else
            {
                TestKillServerRpc();
            }
        }
    }

    [ServerRpc]
    private void TestKillServerRpc()
    {
        HP.Value = 0;
    }
}*/



using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> HP = new(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsAlive = new(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private GameObject _playerModel;

    private Transform[] _spawnPoints;
    private Coroutine _respawnCoroutine;

    public override void OnNetworkSpawn()
    {
        DebugGUI.Log($"[Spawn] Owner: {OwnerClientId} | Server: {IsServer} | Client: {IsClient}");
        FindSpawnPoints();

        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        if (IsOwner)
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);

        // При Owner Authority сервер НЕ может телепортировать напрямую.
        // Отправляем RPC клиенту, чтобы он установил позицию сам.
        if (IsServer && _spawnPoints != null && _spawnPoints.Length > 0)
        {
            ApplyPositionClientRpc(_spawnPoints[0].position);
            DebugGUI.Log($"[Server] Initial pos RPC sent: {_spawnPoints[0].position}");
        }
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        _spawnPoints = spawns.Select(go => go.transform).ToArray();

        if (_spawnPoints.Length == 0)
        {
            DebugGUI.LogWarning("No 'SpawnPoint' tags found! Using fallback.");
            _spawnPoints = new Transform[] { transform };
        }
        else
        {
            DebugGUI.Log($"Found {_spawnPoints.Length} spawn points.");
        }
    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;

        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safe = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safe;
    }

    private void OnHpChanged(int prev, int next)
    {
        DebugGUI.Log($"[NetVar] HP: {prev} -> {next} | IsServer: {IsServer} | IsAlive: {IsAlive.Value}");

        // Убираем проверку IsServer - пусть ВСЕ получают уведомление
        // Но Die() вызываем только на сервере
        if (next <= 0 && IsAlive.Value)
        {
            if (IsServer)
            {
                DebugGUI.Log("[Server] Player died - starting respawn");
                Die();
            }
            else
            {
                DebugGUI.Log("[Client] Player died - waiting for server");
            }
        }
    }

    private void Die()
    {
        IsAlive.Value = false;
        if (_respawnCoroutine != null) StopCoroutine(_respawnCoroutine);
        _respawnCoroutine = StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        DebugGUI.Log("[Routine] Waiting 3s for respawn...");
        yield return new WaitForSeconds(3f);

        // НЕ вызываем ServerRpc из корутины на сервере!
        // Вместо этого напрямую делаем респавн
        DebugGUI.Log("[Routine] Time's up - respawning directly");

        Vector3 spawnPos = Vector3.zero;
        if (_spawnPoints != null && _spawnPoints.Length > 0)
            spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

        // Применяем позицию через ClientRpc
        ApplyPositionClientRpc(spawnPos);

        // Обновляем статы
        HP.Value = 100;
        IsAlive.Value = true;

        DebugGUI.Log($"[Server] Direct respawn at {spawnPos}");
        _respawnCoroutine = null;
    }

    /*
    [ServerRpc]
    private void RespawnPlayerServerRpc()
    {
        DebugGUI.Log("[Server] Respawn RPC triggered.");
        Vector3 spawnPos = Vector3.zero;

        if (_spawnPoints != null && _spawnPoints.Length > 0)
            spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

        // Сначала телепортируем, потом обновляем статы
        ApplyPositionClientRpc(spawnPos);

        HP.Value = 100;
        IsAlive.Value = true;
        DebugGUI.Log($"[Server] HP reset, IsAlive=true. Pos: {spawnPos}");
    }
    */

    [ClientRpc]
    private void ApplyPositionClientRpc(Vector3 pos)
    {
        if (!IsOwner) return;

        transform.position = pos;
        transform.rotation = Quaternion.identity;
        DebugGUI.Log($"[Client] APPLIED RESPAWN POSITION: {pos}");

        // Сбрасываем гравитацию, чтобы персонаж не падал сквозь пол
        var movement = GetComponent<PlayerMovement>();
        //if (movement != null) movement.ResetGravity();

        var shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.ResetAmmo();
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        DebugGUI.Log($"[Client] IsAlive: {prev} -> {next}");
        if (_playerModel) _playerModel.SetActive(next);

        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = next;

        var movement = GetComponent<PlayerMovement>();
        if (movement) movement.enabled = next;

        var shooting = GetComponent<PlayerShooting>();
        if (shooting) shooting.enabled = next;
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.K))
        {
            DebugGUI.Log("[Input] K pressed -> Kill");
            if (IsServer) HP.Value = 0;
            else TestKillServerRpc();
        }
    }

    [ServerRpc]
    private void TestKillServerRpc() => HP.Value = 0;
}
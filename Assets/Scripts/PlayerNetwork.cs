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
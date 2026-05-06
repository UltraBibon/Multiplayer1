/*
using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (_playerNetwork == null)
        {
            Debug.LogError("[PlayerView] PlayerNetwork не найден!");
            return;
        }

        // В FishNet хуки SyncVar вызываются автоматически, но можно обновить UI сразу
        UpdateUI(_playerNetwork.Nickname, _playerNetwork.HP);
    }

    private void UpdateUI(string nickname, int hp)
    {
        if (_nicknameText != null)
            _nicknameText.text = nickname;
        if (_hpText != null)
            _hpText.text = $"HP: {hp}";
    }

    // Хуки уже в PlayerNetwork, здесь просто читаем SyncVar
    private void LateUpdate()
    {
        if (_playerNetwork != null)
        {
            UpdateUI(_playerNetwork.Nickname, _playerNetwork.HP);
        }
    }
}
*/

using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (_playerNetwork == null)
        {
            Debug.LogError("[PlayerView] PlayerNetwork не найден!");
            return;
        }

        _playerNetwork.Nickname.OnChange += OnNicknameChanged;
        _playerNetwork.HP.OnChange += OnHpChanged;

        // Первоначальное отображение значений
        OnNicknameChanged(null, _playerNetwork.Nickname.Value, false);
        OnHpChanged(0, _playerNetwork.HP.Value, false);

        Debug.Log($"[PlayerView] Подписан на изменения: {_playerNetwork.Nickname.Value}");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnChange -= OnNicknameChanged;
            _playerNetwork.HP.OnChange -= OnHpChanged;
        }
    }

    // В FishNet сигнатура обратного вызова SyncVar другая: (prev, next, asServer)
    private void OnNicknameChanged(string oldValue, string newValue, bool asServer)
    {
        if (_nicknameText != null)
            _nicknameText.text = newValue;
    }

    private void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newValue}";
    }
}
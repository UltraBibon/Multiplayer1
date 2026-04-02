/*
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    [Header("UI Settings")]
    [SerializeField] private float _followOffset = 2f;
    [SerializeField] private Camera _mainCamera;

    private void Awake()
    {
        // Автоматический поиск компонентов, если не назначены
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();

        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[PlayerView] OnNetworkSpawn для {gameObject.name}");

        // Подписываемся на изменения сетевых переменных
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        // Сразу обновляем UI текущими значениями
        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        Debug.Log($"[PlayerView] OnNetworkDespawn для {gameObject.name}");

        // Обязательная отписка для предотвращения утечек памяти
        _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged -= OnHpChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (_nicknameText != null)
        {
            _nicknameText.text = newValue.ToString();
            Debug.Log($"[UI] Никнейм обновлён: {newValue}");
        }
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        if (_hpText != null)
        {
            _hpText.text = $"HP: {newValue}";
            Debug.Log($"[UI] HP обновлён: {oldValue} -> {newValue}");
        }
    }

    private void LateUpdate()
    {
        // UI следует за игроком (опционально)
        if (_mainCamera != null && transform.position.y > -100)
        {
            Vector3 targetPos = transform.position + Vector3.up * _followOffset;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(targetPos);

            if (screenPos.z > 0)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            }
        }
    }
}*/

using TMPro;
using Unity.Collections;
using Unity.Netcode;
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (_playerNetwork == null)
        {
            Debug.LogError("[PlayerView] PlayerNetwork не найден!");
            return;
        }

        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);

        Debug.Log($"[PlayerView] Подписан на изменения: {_playerNetwork.Nickname.Value}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
            _playerNetwork.HP.OnValueChanged -= OnHpChanged;
        }
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (_nicknameText != null)
            _nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newValue}";
    }
}
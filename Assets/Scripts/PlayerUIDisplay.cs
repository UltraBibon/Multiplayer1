/*
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerNetwork))]
public class PlayerUIDisplay : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private GameObject _crosshair;

    [Header("Settings")]
    [SerializeField] private bool _showOnlyForOwner = true;

    private PlayerNetwork _playerNetwork;
    private PlayerShooting _playerShooting;

    private void Awake()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        _playerShooting = GetComponent<PlayerShooting>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (_showOnlyForOwner && !IsOwner)
        {
            enabled = false;
            return;
        }

        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
            _playerNetwork.HP.OnValueChanged += OnHpChanged;

            OnNicknameChanged(default, _playerNetwork.Nickname.Value);
            OnHpChanged(0, _playerNetwork.HP.Value);
        }

        if (_playerShooting != null && IsOwner)
        {
            _playerShooting.OnAmmoChanged += OnAmmoChanged;
            OnAmmoChanged(_playerShooting.GetCurrentAmmo());
        }

        if (_crosshair != null)
            _crosshair.SetActive(IsOwner);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
            _playerNetwork.HP.OnValueChanged -= OnHpChanged;
        }

        if (_playerShooting != null)
        {
            _playerShooting.OnAmmoChanged -= OnAmmoChanged;
        }
    }

    private void OnNicknameChanged(FixedString32Bytes oldVal, FixedString32Bytes newVal)
    {
        if (_nicknameText != null)
            _nicknameText.text = newVal.ToString();
    }

    private void OnHpChanged(int oldVal, int newVal)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newVal}/100";

        if (_hpText != null && newVal <= 30)
            _hpText.color = Color.red;
        else if (_hpText != null)
            _hpText.color = Color.white;
    }

    private void OnAmmoChanged(int ammo)
    {
        if (_ammoText != null)
            _ammoText.text = $"Ammo: {ammo}";
    }
}*/
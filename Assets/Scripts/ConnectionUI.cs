/*
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;

    // Статическое поле для передачи ника сетевому объекту
    public static string PlayerNickname { get; private set; } = "Player";

    private void Awake()
    {
        // Сохраняем ссылку на меню для последующего скрытия
        _menuPanel = _menuPanel ?? transform.Find("Panel").gameObject;
    }

    public void StartAsHost()
    {
        SaveNickname();
        Debug.Log($"[Host] Запуск с ником: {PlayerNickname}");
        NetworkManager.Singleton.StartHost();
        HideMenu();
    }

    public void StartAsClient()
    {
        SaveNickname();
        Debug.Log($"[Client] Подключение с ником: {PlayerNickname}");
        NetworkManager.Singleton.StartClient();
        HideMenu();
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void HideMenu()
    {
        if (_menuPanel != null)
            _menuPanel.SetActive(false);
    }

    // Проверка подключения для скрытия меню
    private void OnNetworkReady()
    {
        if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost)
        {
            HideMenu();
        }
    }
}
*/



// FISHNET
/*
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;

    public static string PlayerNickname { get; private set; } = "Player";
    public static bool IsConnected { get; private set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден в сцене!");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;

        Debug.Log("[ConnectionUI] Инициализирован успешно");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }
    }

    public void StartAsHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден!");
            return;
        }

        SaveNickname();
        NetworkManager.Singleton.StartHost();
        Debug.Log($"[ConnectionUI] Host запущен. Ник: {PlayerNickname}");
    }

    public void StartAsClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден!");
            return;
        }

        SaveNickname();
        NetworkManager.Singleton.StartClient();
        Debug.Log($"[ConnectionUI] Client запущен. Ник: {PlayerNickname}");
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void OnClientConnected(ulong clientId)
    {
        IsConnected = true;
        Debug.Log($"[ConnectionUI] Подключён! ClientID: {clientId}");

        if (_menuPanel != null)
            _menuPanel.SetActive(false);
    }

    private void OnServerStopped(bool cleanly)
    {
        IsConnected = false;
        Debug.Log($"[ConnectionUI] Сервер остановлен");

        if (_menuPanel != null)
            _menuPanel.SetActive(true);
    }
}
*/



/*
using FishNet.Managing;
using FishNet.Connection;
using TMPro;
using UnityEngine;
using FishNet;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;

    private NetworkManager _manager;

    public static string PlayerNickname { get; private set; } = "Player";
    public static bool IsConnected { get; private set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _manager = InstanceFinder.NetworkManager;

        if (_manager == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден в сцене!");
            return;
        }

        // ПРАВИЛЬНАЯ подписка в FishNet 4.x - через SceneManager
        _manager.SceneManager.OnClientConnected += OnClientConnected;
        _manager.ServerManager.OnServerStarted += OnServerStarted;

        Debug.Log("[ConnectionUI] Инициализирован успешно");
    }

    private void OnDestroy()
    {
        if (_manager != null)
        {
            _manager.SceneManager.OnClientConnected -= OnClientConnected;
            _manager.ServerManager.OnServerStarted -= OnServerStarted;
        }
    }

    public void StartAsHost()
    {
        SaveNickname();
        if (_manager != null)
        {
            // ПРАВИЛЬНЫЙ запуск хоста в FishNet 4.x
            _manager.StartServer();
            _manager.StartClient();
            Debug.Log($"[ConnectionUI] Host запущен. Ник: {PlayerNickname}");
        }
    }

    public void StartAsClient()
    {
        SaveNickname();
        if (_manager != null)
        {
            // ПРАВИЛЬНЫЙ запуск клиента
            _manager.StartClient();
            Debug.Log($"[ConnectionUI] Client запущен. Ник: {PlayerNickname}");
        }
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void OnServerStarted()
    {
        Debug.Log("[ConnectionUI] Server started");
    }

    private void OnClientConnected(FishNet.Connection.NetworkConnection conn)
    {
        if (conn != _manager.LocalConnection) return;

        IsConnected = true;
        Debug.Log($"[ConnectionUI] Подключён! ClientId: {conn.ClientId}");
        if (_menuPanel != null) _menuPanel.SetActive(false);
    }
}
*/


using TMPro;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;

    public static string PlayerNickname { get; private set; } = "Player";
    public static bool IsConnected { get; private set; } = false;

    private void Start()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден в сцене!");
            return;
        }

        // Подписываемся на события клиента
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        // Серверные события, если нужно
        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;

        Debug.Log("[ConnectionUI] Инициализирован успешно");
    }

    private void OnDestroy()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager != null)
        {
            networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        }
    }

    //Колибек при изменении состояния сервера
    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            IsConnected = false;
            Debug.Log($"[ConnectionUI] Сервер остановлен");
            if (_menuPanel != null)
                _menuPanel.SetActive(true);
        }
    }

    //Колибек при изменении состояния клиента
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            IsConnected = true;
            Debug.Log($"[ConnectionUI] Подключён! ClientID: {InstanceFinder.NetworkManager.ClientManager.Connection.ClientId}");

            if (_menuPanel != null)
                _menuPanel.SetActive(false);
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            IsConnected = false;
            Debug.Log($"[ConnectionUI] Клиент остановлен");
            if (_menuPanel != null)
                _menuPanel.SetActive(true);
        }
    }

    public void StartAsHost()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден!");
            return;
        }

        SaveNickname();
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
        Debug.Log($"[ConnectionUI] Host запущен. Ник: {PlayerNickname}");
    }

    public void StartAsClient()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
        {
            Debug.LogError("[ConnectionUI] NetworkManager не найден!");
            return;
        }

        SaveNickname();
        networkManager.ClientManager.StartConnection();
        Debug.Log($"[ConnectionUI] Client запущен. Ник: {PlayerNickname}");
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }
}
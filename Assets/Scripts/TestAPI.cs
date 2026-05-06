using UnityEngine;
using FishNet.Managing;
using FishNet;

public class TestAPI : MonoBehaviour
{
    private void Start()
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null)
        {
            Debug.LogError("NetworkManager не найден!");
            return;
        }

        // Проверяем ВСЕ доступные методы
        var methods = nm.GetType().GetMethods();
        Debug.Log("=== ДОСТУПНЫЕ МЕТОДЫ NetworkManager ===");
        foreach (var m in methods)
        {
            if (m.Name.Contains("Start") || m.Name.Contains("Connection"))
            {
                Debug.Log(m.Name);
            }
        }

        // Проверяем ServerManager
        if (nm.ServerManager != null)
        {
            var sm = nm.ServerManager;
            var smEvents = sm.GetType().GetEvents();
            Debug.Log("=== СОБЫТИЯ ServerManager ===");
            foreach (var e in smEvents)
            {
                Debug.Log(e.Name);
            }
        }

        // Проверяем ClientManager  
        if (nm.ClientManager != null)
        {
            var cm = nm.ClientManager;
            var cmEvents = cm.GetType().GetEvents();
            Debug.Log("=== СОБЫТИЯ ClientManager ===");
            foreach (var e in cmEvents)
            {
                Debug.Log(e.Name);
            }
        }
    }
}
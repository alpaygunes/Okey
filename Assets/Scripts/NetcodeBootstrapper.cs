using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetcodeBootstrapper : MonoBehaviour
{
    public GameObject networkManagerPrefab;

    private static NetcodeBootstrapper _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (NetworkManager.Singleton == null)
        {
            Instantiate(networkManagerPrefab);
        }
    }

    public static void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            GameObject.Destroy(NetworkManager.Singleton.gameObject);
        }

        _instance = null;
    }
}
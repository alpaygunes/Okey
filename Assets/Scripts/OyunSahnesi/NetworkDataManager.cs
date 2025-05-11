using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkDataManager : NetworkBehaviour{
    public static NetworkDataManager Instance;
    public NetworkList<PlayerData> oyuncuListesi = new NetworkList<PlayerData>();
    public Coroutine skorListesiniYavasGuncelleCoroutine;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Yeni olanı yok et
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public override void OnNetworkSpawn(){
        if (IsServer){
            
            // Host olduğunda ilk client kendisi olur
            NetworkManager.Singleton.OnServerStarted += () => {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList){ 
                    AddOyuncu(client.ClientId);
                }
                NetworkManager.Singleton.OnClientConnectedCallback += AddOyuncu;
            };
             
            // Host kendi kaydını da gecikmeli eklesin
            StartCoroutine(GecikmeliOyuncuEkle(NetworkManager.Singleton.LocalClientId));
        }

        if (IsClient){
            oyuncuListesi.OnListChanged += OnOyuncuListesiGuncellendi;
        }
    } 

    private IEnumerator GecikmeliOyuncuEkle(ulong clientId){ 
        yield return new WaitUntil(() => IsSpawned && oyuncuListesi != null);
        AddOyuncu(clientId);
    }


 

    private void OnDisable(){
        if (IsServer && NetworkManager.Singleton != null){
            NetworkManager.Singleton.OnClientConnectedCallback -= AddOyuncu;
        }

        if (skorListesiniYavasGuncelleCoroutine != null){
            StopCoroutine(skorListesiniYavasGuncelleCoroutine);
        }

        oyuncuListesi.OnListChanged -= OnOyuncuListesiGuncellendi;
    }

    private void AddOyuncu(ulong clientId){
        try{
            if (!IsSpawned){
                //Debug.LogWarning("AddOyuncu() çağrıldı ama NetworkDataManager henüz spawn edilmemiş.");
                return;
            }

            if (oyuncuListesi == null){
                //Debug.LogError("oyuncuListesi null! OnNetworkSpawn henüz çalışmadı.");
                return;
            }

            string displayName = LobbyManager.Instance != null ? LobbyManager.Instance.myDisplayName : "Unknown";

            if (!OyuncuVarMi(clientId)){
                oyuncuListesi.Add(new PlayerData
                {
                    ClientId = clientId,
                    Skor = 0,
                    HamleSayisi = 0,
                    ClientName = displayName,
                });
            }
        }catch (Exception e){
            Debug.LogError($"AddOyuncu içinde hata {e.Message}");
        }
    }


    private bool OyuncuVarMi(ulong clientId){
        if (oyuncuListesi == null || !IsSpawned){
            Debug.LogWarning("Oyuncu listesi hazır değil veya nesne spawn edilmedi.");
            return false;
        }

        for (int i = 0; i < oyuncuListesi.Count; i++){
            if (oyuncuListesi[i].ClientId == clientId){
                return true;
            }
        }

        return false;
    }


    private void OnOyuncuListesiGuncellendi(NetworkListEvent<PlayerData> changeEvent){
        if (GameManager.Instance?.oyunDurumu == GameManager.OynanmaDurumu.bitti){
            skorListesiniYavasGuncelleCoroutine = StartCoroutine(SkorListesiniYavasGuncelle());
        }
    }


    public IEnumerator SkorListesiniYavasGuncelle(){
        yield return new WaitUntil(() => OyunSonu.Instance != null);
        OyunSonu.Instance.SonucListesiniGoster();
    }


    [ServerRpc(RequireOwnership = false)]
    public void SkorVeHamleGuncelleServerRpc(int skor, int hamleSayisi, FixedString64Bytes clientName,
        ServerRpcParams rpcParams = default){
        try{
            for (int i = 0; i < oyuncuListesi.Count; i++){
                if (oyuncuListesi[i].ClientId == rpcParams.Receive.SenderClientId){
                    oyuncuListesi[i] = new PlayerData
                    {
                        ClientId = rpcParams.Receive.SenderClientId,
                        Skor = skor,
                        HamleSayisi = hamleSayisi,
                        ClientName = clientName,
                    };
                    break;
                }
            }
        }
        catch (Exception e){
            Debug.Log($"SkorVeHamleGuncelleServerRpc HATA : {e.Message}");
        }
    }

    public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>{
        public ulong ClientId;
        public int Skor;
        public int HamleSayisi;
        public FixedString64Bytes ClientName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Skor);
            serializer.SerializeValue(ref HamleSayisi);
            serializer.SerializeValue(ref ClientName);
        }

        public bool Equals(PlayerData other){
            return ClientId == other.ClientId;
        }
    }

    [ServerRpc]
    public  void OyunuYenidenBaslatServerRpc(){
        YeniSeediLobbyeGonder();
        if (IsHost){
            NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
        }
    }

    private async void YeniSeediLobbyeGonder(){
        var gameSeed = LobbyManager.Instance.GetRandomSeed();
        try{
            await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                { 
                    { "GameSeed", new DataObject(DataObject.VisibilityOptions.Public, gameSeed) }, 
                }
            });
        }
        catch (Exception e){
           Debug.Log($"YeniSeediLobbyeGonder içinde HATA {e.Message}");
        }
    } 

}
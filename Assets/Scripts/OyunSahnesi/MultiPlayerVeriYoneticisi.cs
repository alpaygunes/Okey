using System;
using System.Collections;
using System.Collections.Generic; 
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiPlayerVeriYoneticisi : NetworkBehaviour{
    public static MultiPlayerVeriYoneticisi Instance;
    public NetworkList<PlayerData> OyuncuListesi = new NetworkList<PlayerData>();
    public Coroutine skorListesiniYavasGuncelleCoroutine;
    
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>{
        public ulong ClientId; 
        public int Skor;
        public int BonusMeyveSayisi;
        public int HamleSayisi; 
        public int Zaman;
        public int CanSayisi;
        public int YokEdilenTasSayisi;
        public int GorevSayisi;
        public int AltinSayisi;
        public int ElmasSayisi;
        public int ToplamTasSayisi; 
        public FixedString64Bytes ClientName;
        public FixedString64Bytes AvadarID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref BonusMeyveSayisi);
            serializer.SerializeValue(ref HamleSayisi);
            serializer.SerializeValue(ref Zaman);
            serializer.SerializeValue(ref CanSayisi);
            serializer.SerializeValue(ref YokEdilenTasSayisi);
            serializer.SerializeValue(ref GorevSayisi);
            serializer.SerializeValue(ref AltinSayisi);
            serializer.SerializeValue(ref ElmasSayisi);
            serializer.SerializeValue(ref ToplamTasSayisi); 
            serializer.SerializeValue(ref AvadarID); 
        }

        public bool Equals(PlayerData other){
            return ClientId == other.ClientId;
        }
    }

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
            OyuncuListesi.OnListChanged += OnOyuncuListesiGuncellendi;
        }
    } 

    private IEnumerator GecikmeliOyuncuEkle(ulong clientId){ 
        yield return new WaitUntil(() => IsSpawned && OyuncuListesi != null);
        AddOyuncu(clientId);
    }

    private void OnDisable(){
        if (IsServer && NetworkManager.Singleton != null){
            NetworkManager.Singleton.OnClientConnectedCallback -= AddOyuncu;
        }

        if (skorListesiniYavasGuncelleCoroutine != null){
            StopCoroutine(skorListesiniYavasGuncelleCoroutine);
        } 
        OyuncuListesi.OnListChanged -= OnOyuncuListesiGuncellendi;
    }

    private void AddOyuncu(ulong clientId){ 
        try{
            if (!IsSpawned){
                return;
            }

            if (OyuncuListesi == null){
                return;
            }

            string displayName = LobbyManager.Instance ? LobbyManager.Instance.myDisplayName : "Unknown";
            string AvadarID = LobbyManager.Instance.AvadarID;

            if (!OyuncuVarMi(clientId)){
                OyuncuListesi.Add(new PlayerData
                {
                    ClientId = clientId,
                    Skor  = 0, 
                    BonusMeyveSayisi = 0,
                    HamleSayisi = 0,
                    Zaman = 0,
                    CanSayisi=0,
                    YokEdilenTasSayisi=0,
                    GorevSayisi=0,
                    AltinSayisi=0,
                    ElmasSayisi=0,
                    ToplamTasSayisi=0,  
                    ClientName = displayName,
                    AvadarID = AvadarID,
                });
            }
        }catch (Exception e){
            Debug.LogError($"AddOyuncu içinde hata {e.Message}");
        }
    }
    
    private bool OyuncuVarMi(ulong clientId){
        if (OyuncuListesi == null || !IsSpawned){ 
            return false;
        }

        for (int i = 0; i < OyuncuListesi.Count; i++){
            if (OyuncuListesi[i].ClientId == clientId){
                return true;
            }
        }

        return false;
    }
    
    private void OnOyuncuListesiGuncellendi(NetworkListEvent<PlayerData> changeEvent){
        if (OyunSahnesiUI.Instance != null){
            OyunSahnesiUI.Instance.AvatarSirasiniGuncelle();
        }
        
        if (GameManager.Instance?.oyunDurumu != GameManager.OyunDurumlari.LimitDoldu){
            skorListesiniYavasGuncelleCoroutine = StartCoroutine(SkorListesiniYavasGuncelle());
        }
    }
    
    public IEnumerator SkorListesiniYavasGuncelle(){
        yield return new WaitUntil(() => OyunSonu.Instance != null);
        OyunSonu.Instance.SonucListesiniGoster();
    }

    public void OyuncuVerileriniGuncelle(){
        Dictionary<string, string> playerDatas = new Dictionary<string, string>();  
        playerDatas.Add("BonusMeyveSayisi",PuanlamaIStatistikleri.BonusMeyveSayisi.ToString());
        playerDatas.Add("HamleSayisi",PuanlamaIStatistikleri.HamleSayisi.ToString());
        playerDatas.Add("Zaman",PuanlamaIStatistikleri.Zaman.ToString());
        playerDatas.Add("CanSayisi",PuanlamaIStatistikleri.CanSayisi.ToString());
        playerDatas.Add("YokEdilenTasSayisi",PuanlamaIStatistikleri.YokEdilenTasSayisi.ToString());
        playerDatas.Add("GorevSayisi",PuanlamaIStatistikleri.GorevSayisi.ToString());
        playerDatas.Add("AltinSayisi",PuanlamaIStatistikleri.AltinSayisi.ToString());
        playerDatas.Add("ElmasSayisi",PuanlamaIStatistikleri.ElmasSayisi.ToString());
        playerDatas.Add("ToplamTasSayisi",PuanlamaIStatistikleri.ToplamTasSayisi.ToString());
        playerDatas.Add("myDisplayName",LobbyManager.Instance.myDisplayName);
        playerDatas.Add("AvadarID",LobbyManager.Instance.AvadarID);

        // Örneğin JSON
        string json = JsonUtility.ToJson(new SerializationHelper(playerDatas));

        FixedString512Bytes fs = json;
        SkorVeHamleGuncelleServerRpc(fs); 
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SkorVeHamleGuncelleServerRpc( FixedString512Bytes playerDatas, ServerRpcParams rpcParams = default){
        // FixedString -> JSON string
        string jsonString = playerDatas.ToString(); 
        // JSON string -> Dictionary<string, string>
        Dictionary<string, string> deserializedDatas = JsonUtility.FromJson<SerializationHelper>(jsonString).ToDictionary();
        FixedString64Bytes clientName = LobbyManager.Instance.myDisplayName;
        try{
            for (int i = 0; i < OyuncuListesi.Count; i++){ 
                if (OyuncuListesi[i].ClientId == rpcParams.Receive.SenderClientId){
                    OyuncuListesi[i] = new PlayerData{
                        ClientId = rpcParams.Receive.SenderClientId,
                        Skor = 0, 
                        BonusMeyveSayisi = Convert.ToInt16(deserializedDatas["BonusMeyveSayisi"]),
                        HamleSayisi = Convert.ToInt16(deserializedDatas["BonusMeyveSayisi"]),
                        Zaman = Convert.ToInt16(deserializedDatas["Zaman"]),
                        CanSayisi=Convert.ToInt16(deserializedDatas["CanSayisi"]),
                        YokEdilenTasSayisi=Convert.ToInt16(deserializedDatas["YokEdilenTasSayisi"]),
                        GorevSayisi=Convert.ToInt16(deserializedDatas["GorevSayisi"]),
                        AltinSayisi=Convert.ToInt16(deserializedDatas["AltinSayisi"]),
                        ElmasSayisi=Convert.ToInt16(deserializedDatas["ElmasSayisi"]),
                        ToplamTasSayisi=Convert.ToInt16(deserializedDatas["ToplamTasSayisi"]),  
                        ClientName = deserializedDatas["myDisplayName"],
                        AvadarID = deserializedDatas["AvadarID"],
                    };
                    break;
                }
            }
        }
        catch (Exception e){
            Debug.Log($"SkorVeHamleGuncelleServerRpc HATA : {e.Message}");
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
        var gameSeed = MainMenu.GetRandomSeed();
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
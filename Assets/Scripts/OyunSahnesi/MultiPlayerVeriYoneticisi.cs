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

            string displayName = LobbyManager.Instance != null ? LobbyManager.Instance.myDisplayName : "Unknown";

            if (!OyuncuVarMi(clientId)){
                OyuncuListesi.Add(new PlayerData
                {
                    ClientId = clientId,
                    Skor  = 0,
                    // PuanlamaIStatistikleri alanları
                    BonusMeyveSayisi = PuanlamaIStatistikleri.BonusMeyveSayisi,
                    HamleSayisi = PuanlamaIStatistikleri.HamleSayisi,
                    Zaman = PuanlamaIStatistikleri.Zaman,
                    CanSayisi=PuanlamaIStatistikleri.CanSayisi,
                    YokEdilenTasSayisi=PuanlamaIStatistikleri.YokEdilenTasSayisi,
                    GorevSayisi=PuanlamaIStatistikleri.GorevSayisi,
                    AltinSayisi=PuanlamaIStatistikleri.AltinSayisi,
                    ElmasSayisi=PuanlamaIStatistikleri.ElmasSayisi,
                    ToplamTasSayisi=PuanlamaIStatistikleri.ToplamTasSayisi, 
                    //------------
                    ClientName = displayName,
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
        /*burada kaldın. puanlar her güncellediniğinde burası çağrılıyor
            puanlarına bakıp avatar sırasını değiştirmek mümün.
            avatarlara animasyon efekt eklene bilir.*/
        if (GameManager.Instance?.OyunDurumu == GameManager.OynanmaDurumu.LimitDoldu){
            skorListesiniYavasGuncelleCoroutine = StartCoroutine(SkorListesiniYavasGuncelle());
        }
    }
    
    public IEnumerator SkorListesiniYavasGuncelle(){
        yield return new WaitUntil(() => OyunSonu.Instance != null);
        OyunSonu.Instance.SonucListesiniGoster();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SkorVeHamleGuncelleServerRpc(ServerRpcParams rpcParams = default){
        FixedString64Bytes clientName = LobbyManager.Instance.myDisplayName;
        try{
            for (int i = 0; i < OyuncuListesi.Count; i++){
                if (OyuncuListesi[i].ClientId == rpcParams.Receive.SenderClientId){
                    OyuncuListesi[i] = new PlayerData{
                        ClientId = rpcParams.Receive.SenderClientId,
                        Skor = 0,
                        // PuanlamaIStatistikleri alanları
                        BonusMeyveSayisi = PuanlamaIStatistikleri.BonusMeyveSayisi,
                        HamleSayisi = PuanlamaIStatistikleri.BonusMeyveSayisi,
                        Zaman = PuanlamaIStatistikleri.Zaman,
                        CanSayisi=PuanlamaIStatistikleri.CanSayisi,
                        YokEdilenTasSayisi=PuanlamaIStatistikleri.YokEdilenTasSayisi,
                        GorevSayisi=PuanlamaIStatistikleri.GorevSayisi,
                        AltinSayisi=PuanlamaIStatistikleri.AltinSayisi,
                        ElmasSayisi=PuanlamaIStatistikleri.ElmasSayisi,
                        ToplamTasSayisi=PuanlamaIStatistikleri.ToplamTasSayisi, 
                        //------------
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
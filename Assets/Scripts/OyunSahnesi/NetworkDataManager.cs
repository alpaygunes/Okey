using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkDataManager : NetworkBehaviour{
    public static NetworkDataManager Instance;

    // ✅ Doğrudan burada başlat
    private NetworkList<PlayerData> OyuncuListesi = new NetworkList<PlayerData>();

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Bu nesneyi sahne değişimlerinde yok olmaktan koru
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
        }

        if (IsClient){
            OyuncuListesi.OnListChanged += OnOyuncuListesiGuncellendi; 
        }
    }

    private void OnDisable(){
        OyuncuListesi.OnListChanged -= OnOyuncuListesiGuncellendi; 
    }

    private void AddOyuncu(ulong clientId){  
        if (!OyuncuVarMi(clientId)){
            OyuncuListesi.Add(new PlayerData
            {
                ClientId = clientId,
                Skor = 0,
                HamleSayisi = 0,
                ClientName = LobbyManager.Instance.myDisplayName,
            }); 
        }
    }

    private bool OyuncuVarMi(ulong clientId){
        for (int i = 0; i < OyuncuListesi.Count; i++){
            if (OyuncuListesi[i].ClientId == clientId){
                return true;
            }
        }
        return false;
    }


    private void OnOyuncuListesiGuncellendi(NetworkListEvent<PlayerData> changeEvent){    
        if (GameManager.Instance?.OyunDurumu == GameManager.OynanmaDurumu.bitti){
            StartCoroutine(SkorListesiniYavasGuncelle());
        }
    }
    
    
    private IEnumerator SkorListesiniYavasGuncelle(){ 
        yield return new WaitUntil(() => OyunSonu.Instance != null);
        OyunSonu.Instance.SonucListesiniGoster(OyuncuListesi); 
    }
     
    
    [ServerRpc(RequireOwnership = false)]
    public void SkorVeHamleGuncelleServerRpc(  int skor, int hamleSayisi,FixedString64Bytes clientName, ServerRpcParams rpcParams = default){  
 
        for (int i = 0; i < OyuncuListesi.Count; i++){
            if (OyuncuListesi[i].ClientId == rpcParams.Receive.SenderClientId ){ 
                OyuncuListesi[i] = new PlayerData
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
}
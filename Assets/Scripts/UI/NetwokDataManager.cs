using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetwokDataManager : NetworkBehaviour{
    public static NetwokDataManager Instance;

    // ✅ Doğrudan burada başlat
    public NetworkList<PlayerData> oyuncuListesi = new NetworkList<PlayerData>();

    private void Awake(){
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
            oyuncuListesi.OnListChanged += OnOyuncuListesiGuncellendi;
        }
    }

    private void AddOyuncu(ulong clientId){
        if (!OyuncuVarMi(clientId)){
            oyuncuListesi.Add(new PlayerData
            {
                ClientId = clientId,
                Skor = 0,
                HamleSayisi = 0
            });
        }
    }

    private bool OyuncuVarMi(ulong clientId){
        for (int i = 0; i < oyuncuListesi.Count; i++){
            if (oyuncuListesi[i].ClientId == clientId){
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
        OyunSonu.Instance.SonucListesiniGoster(oyuncuListesi); 
    }
    
    
    
    
    
    

    [ServerRpc(RequireOwnership = false)]
    public void SkorVeHamleGuncelleServerRpc(int skor, int hamleSayisi, ServerRpcParams rpcParams = default){
        for (int i = 0; i < oyuncuListesi.Count; i++){
            if (oyuncuListesi[i].ClientId == rpcParams.Receive.SenderClientId){
                oyuncuListesi[i] = new PlayerData
                {
                    ClientId = rpcParams.Receive.SenderClientId,
                    Skor = skor,
                    HamleSayisi = hamleSayisi
                };
                break;
            }
        }
    }

    public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>{
        public ulong ClientId;
        public int Skor;
        public int HamleSayisi;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Skor);
            serializer.SerializeValue(ref HamleSayisi);
        }

        public bool Equals(PlayerData other){
            return ClientId == other.ClientId;
        }
    }
}
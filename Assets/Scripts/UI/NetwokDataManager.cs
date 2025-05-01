using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetwokDataManager : NetworkBehaviour{
    public static NetwokDataManager Instance;

    private void Awake(){
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public NetworkVariable<PlayerData> playerData = new NetworkVariable<PlayerData>(
        new PlayerData
        {
            score = 0,
            health = 100,
            playerName = "Unknown"
        },
        NetworkVariableReadPermission.Everyone, // Herkes okuyabilir
        NetworkVariableWritePermission.Owner // Sadece owner yazabilir
    );



    [System.Serializable]
    public struct PlayerData : INetworkSerializable{
        public int score;
        public int health;
        public FixedString64Bytes playerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
            serializer.SerializeValue(ref score);
            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref playerName);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestHamleSayisiGuncelleServerRpc(int hamleSayisi){
        var data = playerData.Value;
        data.score = hamleSayisi;
        playerData.Value = data;
        //Debug.Log($"HAMLE SAYISI güncellendi: {data.score}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestToplamPuanGuncelleServerRpc(int netwokrDataToplamPuan){
        var data = playerData.Value;
        data.score = netwokrDataToplamPuan;
        playerData.Value = data;
        //Debug.Log($"TOPLAM PUAN güncellendi: {data.score}");
    }


    private void Start(){
        if (IsOwner){
            var data = new PlayerData
            {
                score = 0,
                health = 100,
                playerName = "Player_" + OwnerClientId
            };
            playerData.Value = data;
        }
        playerData.OnValueChanged += OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged(PlayerData previous, PlayerData current){
        //Debug.Log($"Data Updated! Name: {current.playerName}, Score: {current.score}, Health: {current.health}");
    }

    public void HamleLimitiDoldu(){
        HamleLimitiDolduServerRpc(Puanlama.Instance.ToplamPuan, GameManager.OynanmaDurumu.bitti);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HamleLimitiDolduServerRpc(int clientToplamPuan, GameManager.OynanmaDurumu OyunDurumu,
        ServerRpcParams rpcParams = default){
        ulong clientId = rpcParams.Receive.SenderClientId;
        // Client'tan gelen puanı kaydet
        OyunKurallari.Instance._hamleLimitiDolanClientIDlerinDic[clientId] = clientToplamPuan;
        Debug.Log($"CLIENT ID: {clientId}  PUAN: {clientToplamPuan}");
        ulong[] clientIds = OyunKurallari.Instance._hamleLimitiDolanClientIDlerinDic.Keys.ToArray();
        int[] puanlar = OyunKurallari.Instance._hamleLimitiDolanClientIDlerinDic.Values.ToArray();
        SkorListesiniGuncelleClientRpc(clientIds, LobbyManager.Instance.myDisplayName, puanlar, OyunDurumu);
    }

    [ClientRpc]
    private void SkorListesiniGuncelleClientRpc(ulong[] clientIds, string myDisplayName, int[] puanlar,
        GameManager.OynanmaDurumu OyunDurumu){
        StartCoroutine(SkorListesiniYavasGuncelle(clientIds, myDisplayName, puanlar, OyunDurumu));
        skorListesiOlustu = false;// hostta ikdefa çalışmaması için
    }

    private bool skorListesiOlustu;
    private IEnumerator SkorListesiniYavasGuncelle(ulong[] clientIds, string myDisplayName, int[] puanlar,
        GameManager.OynanmaDurumu OyunDurumu){
        yield return new WaitUntil(() => OyunSonu.Instance != null);
        if (IsHost){
            if (skorListesiOlustu) yield break;
            skorListesiOlustu = true; 
        }
        if (OyunDurumu == GameManager.OynanmaDurumu.bitti){
            var yeniDic = new Dictionary<ulong, Dictionary<string, string>>();
            for (int i = 0; i < clientIds.Length; i++){
                var bilgiler = new Dictionary<string, string>();
                bilgiler["myDisplayName"] = myDisplayName;
                bilgiler["puan"] = puanlar[i].ToString();
                yeniDic.Add(clientIds[i], bilgiler);
            }

            OyunSonu.Instance.SkorListesiDic = yeniDic;
        }
    }
}
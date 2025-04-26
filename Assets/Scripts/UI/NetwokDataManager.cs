using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetwokDataManager : NetworkBehaviour{
    public static NetwokDataManager Instance;

    private void Awake(){
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
    public void RequestHamleSayisiGuncelleServerRpc(int hamleSayisi)
    {
        var data = playerData.Value;
        data.score = hamleSayisi;
        playerData.Value = data;

        Debug.Log($"HAMLE SAYISI güncellendi: {data.score}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestToplamPuanGuncelleServerRpc(int toplamPuan)
    {
        var data = playerData.Value;
        data.score = toplamPuan;
        playerData.Value = data;

        Debug.Log($"TOPLAM PUAN güncellendi: {data.score}");
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
        Debug.Log($"Data Updated! Name: {current.playerName}, Score: {current.score}, Health: {current.health}");
    }
}
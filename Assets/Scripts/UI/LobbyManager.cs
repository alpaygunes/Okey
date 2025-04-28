using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour{
    public static LobbyManager Instance;

    //public string RelayID;
    public string RelayIDForJoin;

    private LobbyEventCallbacks _callbacks;
    private ILobbyEvents _lobbyEventsSubscription;
    public Lobby CurrentLobby;
    private bool _isRelayActive = false;
    private string _playerId = null;
    private int _maxPlayers = 4;
    private string _lobbyName = "okey";
    private Coroutine _heartbeatCoroutine; 
    private HashSet<ulong> _connectedClients = new HashSet<ulong>();

    async void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }

        try{
            await UnityServices.InitializeAsync();
            await AnonimGiris();
        }
        catch (Exception e){ 
            Debug.Log(e.Message);
        }
    }

    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerId = AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex){
            Debug.Log(ex.Message);
        }
        catch (RequestFailedException ex){
            Debug.Log(ex.Message);
        }
    }

    public async Task CreateLobi(){
        if (_playerId == null){
            await AnonimGiris();
        }

        try{
            if (CurrentLobby != null){
                await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                StopHeartbeat();
            }

            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "HOST") }
            };

            var player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: playerData
            );

            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = player, 
                Data = new Dictionary<string, DataObject>
                {
                    //{ "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, "BOSKOD") },
                    { "oyunTipi", new DataObject(DataObject.VisibilityOptions.Public, OyunKurallari.Instance.GuncelOyunTipi.ToString()) }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, _maxPlayers, options);
            StartHeartbeat();
            LobbyListUI.Instance.CreatedLobiCodeTxt.text = CurrentLobby.LobbyCode;
            await SubscribeToLobbyEvents();

            // await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            // {
            //     Data = new Dictionary<string, DataObject>
            //     {
            //         { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, "BOSKOD") },
            //         { "oyunTipi", new DataObject(DataObject.VisibilityOptions.Public, OyunKurallari.Instance.GuncelOyunTipi.ToString()) }
            //     }
            // });
        }
        catch (Exception e){
            Debug.Log(e.Message);
        }
    }

    public async Task<QueryResponse> GetLobbyList(){
        try{
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    )
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return response;
        }
        catch (LobbyServiceException e){
            Debug.Log(e.Message);
            return null; // Veya hata fırlatmak istersen throw;
        }
    }

    public async Task<bool> JoinLobbyByID(string lobbyID){
        if (string.IsNullOrEmpty(lobbyID)) return false;

        if (!AuthenticationService.Instance.IsSignedIn){
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try{
            var options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "ALPAY") }
                    }
                }
            };


            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, options);
            CurrentLobby = joinedLobby;

            // eğer lobyde data değişmi olursa tetiklenen Listener
            _callbacks = new LobbyEventCallbacks();
            _callbacks.DataChanged += (data) => {
                if (data.TryGetValue("RelayCode", out var newRelayData)){
                    string newRelayCode = newRelayData.Value.Value;
                    if (RelayIDForJoin != newRelayCode){
                        RelayIDForJoin = newRelayCode;
                        if (newRelayCode != "BOSKOD" && !_isRelayActive){
                            _ = StartClientWithRelay(newRelayCode, "dtls");
                        }
                    }
                } 
            };

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, _callbacks);

            // ilk girişte lobideki datayı al
            if (joinedLobby.Data.TryGetValue("RelayCode", out var relayData) &&
                !string.IsNullOrEmpty(relayData.Value)){
                string relayCode = relayData.Value;
                RelayIDForJoin = relayCode;
                if (relayCode != "BOSKOD" && !_isRelayActive){
                    await StartClientWithRelay(relayCode, "dtls");
                }
            }  
        }
        catch (LobbyServiceException e){  
            Debug.Log(e.Message);
            return false;
        }
        return true;
    }

    public async Task SubscribeToLobbyEvents(){
        var callbacks = new LobbyEventCallbacks();
        callbacks.PlayerJoined += async players =>
        {
            // SUNUCUDAN GÜNCEL LOBBY'Yİ AL
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            LobbyManager.Instance.CurrentLobby = updatedLobby;
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        
        callbacks.PlayerLeft += async players =>
        {
            // SUNUCUDAN GÜNCEL LOBBY'Yİ AL
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            LobbyManager.Instance.CurrentLobby = updatedLobby;
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
    }

    public void StartHeartbeat(){
        if (_heartbeatCoroutine == null){
            _heartbeatCoroutine = StartCoroutine(SendHeartbeatRoutine());
        }
    }

    public void StopHeartbeat(){
        if (_heartbeatCoroutine != null){
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }
    }

    private IEnumerator SendHeartbeatRoutine(){
        while (true){
            if (CurrentLobby != null){
                LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }

            yield return new WaitForSeconds(15f); // her 15 saniyede bir ping
        }
    }


    
    
    
    
    
    public async Task StartHostWithRelay(int maxConnections){
        if (CurrentLobby.HostId != AuthenticationService.Instance.PlayerId){
            return; // Host başlatılamazsa null döndür
        }

        foreach (var player in CurrentLobby.Players){
        }

        // Mevcut relay varsa kapat
        if (_isRelayActive || NetworkManager.Singleton.IsListening){
            NetworkManager.Singleton.Shutdown();
            _isRelayActive = false;
            await Task.Delay(300); // Kapatma işleminin tamamlanması için kısa bir bekleme
        }

        try{
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); 

            // Lobby'nin relay koduyla güncellenmesi
            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                }
            });

            // Güncellemeyi doğrula
            // var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            // if (updatedLobby.Data.TryGetValue("RelayCode", out var updatedRelayData)){
            // }

            _isRelayActive = true;
            NetworkManager.Singleton.StartHost();

            if (NetworkManager.Singleton == null){
                return;
            }

            if (!IsOwner || !IsServer){
                return;
            }
 
        }
        catch (Exception ex){
            Debug.Log(ex.Message);
        }
    }

    public async Task StartClientWithRelay(string joinCode, string connectionType){
        try{
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client Relaya Bağlandı");
        }
        catch (Exception ex){
            Debug.Log(ex.Message);
        }
    }

    public override void OnNetworkSpawn(){
        if (!IsOwner) return;
        HostaClientinBaglandiginiBildirServerRpc();
    }
    
    [ServerRpc]
    private void HostaClientinBaglandiginiBildirServerRpc(ServerRpcParams rpcParams = default){
        HerkesteOyunBaslasinServerRpc(rpcParams.Receive.SenderClientId);
    }
    
     
    
    [ServerRpc]
    void HerkesteOyunBaslasinServerRpc(ulong clientId){
        _connectedClients.Add(clientId);
    
        if (_connectedClients.Count > 0){
            StartGameClientRpc();
        }
    }
    
    [ClientRpc]
    void StartGameClientRpc(){
        var status = NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
    }
}
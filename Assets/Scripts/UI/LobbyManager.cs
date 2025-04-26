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
    public Lobby currentLobby;
    private bool _isRelayActive = false;
    private string playerId = null;
    private int maxPlayers = 4;
    private string lobbyName = "okey";
    private Coroutine heartbeatCoroutine;
    private int BaglananClientSayisi = 0;

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
        }
    }

    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex){
        }
        catch (RequestFailedException ex){
        }
    }

    public async Task CreateLobi(){
        if (playerId == null){
            await AnonimGiris();
        }

        try{
            if (currentLobby != null){
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
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
                Player = player
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            StartHeartbeat();
            LobbyListUI.Instance.CreatedLobiCodeTxt.text = currentLobby.LobbyCode;
            await SubscribeToLobbyEvents();

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, "BOSKOD") }
                }
            });
        }
        catch (Exception e){
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
            return null; // Veya hata fırlatmak istersen throw;
        }
    }

    public async Task JoinLobbyByID(string lobbyID){
        if (string.IsNullOrEmpty(lobbyID)) return;

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
            currentLobby = joinedLobby;


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
        }
    }

    public async Task SubscribeToLobbyEvents(){
        var callbacks = new LobbyEventCallbacks();
        callbacks.PlayerJoined += async players =>
        {
            // SUNUCUDAN GÜNCEL LOBBY'Yİ AL
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            LobbyManager.Instance.currentLobby = updatedLobby;
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        
        callbacks.PlayerLeft += async players =>
        {
            // SUNUCUDAN GÜNCEL LOBBY'Yİ AL
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            LobbyManager.Instance.currentLobby = updatedLobby;
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(currentLobby.Id, callbacks);
    }

    public void StartHeartbeat(){
        if (heartbeatCoroutine == null){
            heartbeatCoroutine = StartCoroutine(SendHeartbeatRoutine());
        }
    }

    public void StopHeartbeat(){
        if (heartbeatCoroutine != null){
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private IEnumerator SendHeartbeatRoutine(){
        while (true){
            if (currentLobby != null){
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }

            yield return new WaitForSeconds(15f); // her 15 saniyede bir ping
        }
    }


    
    
    
    
    
    public async Task StartHostWithRelay(int maxConnections){
        if (currentLobby.HostId != AuthenticationService.Instance.PlayerId){
            return; // Host başlatılamazsa null döndür
        }

        foreach (var player in currentLobby.Players){
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
            //RelayID = relayCode;

            // Lobby'nin relay koduyla güncellenmesi
            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                }
            });

            // Güncellemeyi doğrula
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            if (updatedLobby.Data.TryGetValue("RelayCode", out var updatedRelayData)){
            }

            _isRelayActive = true;
            NetworkManager.Singleton.StartHost();

            if (NetworkManager.Singleton == null){
                return;
            }

            if (!IsOwner || !IsServer){
                return;
            }


            // Relay kodunu başarıyla döndür
            //return relayCode;
        }
        catch (Exception ex){
            //return null; // Hata durumunda null döndür
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
        }
    }

    public override void OnNetworkSpawn(){
        //Debug.Log("OnNetworkSpawn Client bağlandı, host'a haber veriliyor...");
        if (!IsOwner) return;
        HostaClientinBaglandiginiBildirServerRpc();
    }
    
    [ServerRpc]
    private void HostaClientinBaglandiginiBildirServerRpc(ServerRpcParams rpcParams = default){
        HerkesteOyunBaslasinServerRpc(rpcParams.Receive.SenderClientId);
    }
    
    
    private HashSet<ulong> connectedClients = new HashSet<ulong>();
    
    [ServerRpc]
    void HerkesteOyunBaslasinServerRpc(ulong clientId){
        connectedClients.Add(clientId);
        //Debug.Log($"Client {clientId} bağlandı. Toplam: {connectedClients.Count}");
    
        if (connectedClients.Count > 0){
            //Debug.Log("Tüm client'lar bağlandı, oyunu başlatıyoruz!");
            StartGameClientRpc();
        }
    }
    
    [ClientRpc]
    void StartGameClientRpc(){
        var status = NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
        //Debug.Log($"Sahne yükleme durumu: {status}");
    }
}
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

public class LobbyManager : MonoBehaviour{
    public static LobbyManager Instance;

    public string RelayID;
    public string RelayIDForJoin;
    public string LobiIDforJoin;

    private LobbyEventCallbacks _callbacks;
    private ILobbyEvents _lobbyEventsSubscription;
    public Lobby currentLobby;
    private bool _isRelayActive = false;
    private string playerId = null;
    private int maxPlayers = 4;
    private string lobbyName = "okey"; 
    private Coroutine heartbeatCoroutine;

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
            Debug.LogException(e);
        }
    }

    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            playerId = AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex){
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex){
            Debug.LogException(ex);
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
                { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "HostIsmi") }
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
            Debug.Log("CreateLobi Hata : " + e.Message);
        }
    }

    public async Task<QueryResponse> GetLobbyList()
    {
        try
        {
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
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobi listesi alınırken hata oluştu: {e.Message}");
            return null; // Veya hata fırlatmak istersen throw;
        }
    }

    public async Task JoinLobbyByID(string lobbyID){ 
        if (string.IsNullOrEmpty(lobbyID)) return;
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
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
            
            
            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID,options);
            currentLobby = joinedLobby;
            Debug.Log("Lobby'ye katıldı: " + joinedLobby.LobbyCode);
            
  
            Debug.Log(" Abonelik başlatılıyor 0");
            _callbacks = new LobbyEventCallbacks();
            _callbacks.DataChanged += (data) => {
                Debug.Log("Lobby veri değişikliği algılandı!");
                if (data.TryGetValue("RelayCode", out var newRelayData)){
                    string newRelayCode = newRelayData.Value.Value;
                    Debug.Log($"Alınan relay kodu: {newRelayCode}");
                    if (RelayIDForJoin != newRelayCode){
                        RelayIDForJoin = newRelayCode;
                        Debug.Log("Yeni relay kodu alındı (event): " + newRelayCode);
                        if (newRelayCode != "BOSKOD" && !_isRelayActive){
                            _ = StartClientWithRelay(newRelayCode, "dtls");
                        }
                    }
                }
                else{
                    Debug.LogWarning("Lobby verisinde RelayCode bulunamadı!");
                }
            };

            Debug.Log(" Abonelik başlatılıyor...");
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, _callbacks);
            Debug.Log(" Event aboneliği tamamlandı.");
            
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
            Debug.LogError("Lobby join hatası: " + e.Message);
        }
    }
    
    public async Task SubscribeToLobbyEvents()
    { 
        var callbacks = new LobbyEventCallbacks();
        callbacks.PlayerJoined += players =>
        {
            foreach (var p in players)
            {
                var id = p.Player.Id;
                Debug.Log($"Katılan Oyuncu ID: {id}");
            }
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        
        callbacks.PlayerLeft += players =>
        {
            foreach (var p in players)
            { 
                Debug.Log($"Katılan Oyuncu ID: {p}");
            }
            LobbyListUI.Instance.RefreshPlayerList();  
        };
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(currentLobby.Id, callbacks);
    }
    
    

    public void StartHeartbeat()
    {
        if (heartbeatCoroutine == null)
        {
            heartbeatCoroutine = StartCoroutine(SendHeartbeatRoutine());
        }
    }

    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private IEnumerator SendHeartbeatRoutine()
    {
        while (true)
        {
            if (currentLobby != null)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                Debug.Log("💓 Heartbeat gönderildi");
            }
            yield return new WaitForSeconds(15f); // her 15 saniyede bir ping
        }
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    public void StartHostWithRelayForButton(){
        _ = StartHostWithRelay(2, "dtls");
    }

    public async Task StartHostWithRelay(int maxConnections, string connectionType){
        if (currentLobby.HostId != AuthenticationService.Instance.PlayerId){
            Debug.LogWarning("Sadece lobby sahibi host başlatabilir.");
            return;
        }

        foreach (var player in currentLobby.Players){
            Debug.Log("Oyuncu: " + player.Id);
        }

        // Mevcut relay varsa kapat
        if (_isRelayActive || NetworkManager.Singleton.IsListening){
            Debug.Log("Mevcut relay kapatılıyor...");
            NetworkManager.Singleton.Shutdown();
            _isRelayActive = false;
            await Task.Delay(300); // Kapatma işleminin tamamlanması için kısa bir bekleme
        }


        try{
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayID = relayCode;

            // 1. UpdateLobbyAsync işleminin başarılı olduğunu doğrulayın
            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                }
            });
            Debug.Log($"Lobby güncellendi, Relay kodu: {relayCode}"); // Başarılı olduğunu doğrulayın

            // 2. Lobby'nin güncel halini alarak relay kodunun gerçekten güncellenip güncellenmediğini kontrol edin
            var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            if (updatedLobby.Data.TryGetValue("RelayCode", out var updatedRelayData)){
                Debug.Log($"Güncel lobby'deki relay kodu: {updatedRelayData.Value}");
            }

            _isRelayActive = true;
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception ex){
            Debug.LogError("Host başlatma hatası: " + ex.Message);
        }
    }

    public void StartClientWithRelayForButton(){
        _ = StartClientWithRelay(RelayIDForJoin, "dtls");
    }

    public async Task StartClientWithRelay(string joinCode, string connectionType){
        try{
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

            NetworkManager.Singleton.StartClient();
            Debug.Log("Client başlatıldı.");
        }
        catch (Exception ex){
            Debug.LogError("StartClientWithRelay hatası: " + ex.Message);
        }
    }
}
using System;
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
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour{
    
    public static LobbyManager Instance;

    public string LobiID;
    public string RelayID;
    public string RelayIDForJoin;
    public string LobiIDforJoin;
    
    private LobbyEventCallbacks _callbacks;
    private ILobbyEvents _lobbyEventsSubscription;
    private Lobby currentLobby;
    private bool _isRelayActive =false;

    async void Awake(){
        if (Instance == null) {
            Instance = this;
        }
        else {
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

    public void LobiOlusturForButton(){
        _ =  CreateLobi();
    }

    public void JoinLobiForButton(){
        _ = JoinLobbyByCode();
    }

    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex){
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex){
            Debug.LogException(ex);
        }
    }

    public async Task CreateLobi(){ 
        string lobbyName = "LobiAdi";
        int maxPlayers = 4;
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = false;

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        Debug.Log("Created lobby: " + currentLobby.LobbyCode);
        LobiID = currentLobby.LobbyCode;
        
        // boş relay kodu . tetikleme için
        // 1. UpdateLobbyAsync işleminin başarılı olduğunu doğrulayın
        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, "BOSKOD") }
            }
        });
        
    }

    public async Task JoinLobbyByCode(){
        string lobbyCode = LobiIDforJoin.Trim();
        if (string.IsNullOrEmpty(lobbyCode)) return;

        try{
            var joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            currentLobby = joinedLobby; 
            Debug.Log("Lobby'ye katıldı: " + joinedLobby.LobbyCode);



            Debug.Log(" Abonelik başlatılıyor 0");
            _callbacks = new LobbyEventCallbacks();
            _callbacks.DataChanged += (data) => {
                Debug.Log("Lobby veri değişikliği algılandı!");
                if (data.TryGetValue("RelayCode", out var newRelayData)) {
                    string newRelayCode = newRelayData.Value.Value;
                    Debug.Log($"Alınan relay kodu: {newRelayCode}");
                    if (RelayIDForJoin != newRelayCode) {
                        RelayIDForJoin = newRelayCode;
                        Debug.Log("Yeni relay kodu alındı (event): " + newRelayCode);
                        if (newRelayCode != "BOSKOD" && !_isRelayActive){
                            _ = StartClientWithRelay(newRelayCode, "dtls");
                        } 
                    }
                } else {
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
            if (updatedLobby.Data.TryGetValue("RelayCode", out var updatedRelayData)) {
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
        _= StartClientWithRelay(RelayIDForJoin, "dtls");
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
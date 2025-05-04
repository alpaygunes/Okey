using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class LobbyManager : NetworkBehaviour{
    public static LobbyManager Instance; 
    public string relayIDForJoin;
    private LobbyEventCallbacks clientCallbacks;
    public Lobby CurrentLobby;
    private bool isRelayActive = false;
    private string playerId = null;
    private const int MaxPlayers = 4;
    private const string LobbyName = "okey";
    private Coroutine heartbeatCoroutine;
    private readonly HashSet<ulong> connectedClients = new HashSet<ulong>();
    public string myDisplayName;
    private LobbyEventCallbacks hostCallBacks;
    private Coroutine lobbyUpdateCoroutine;
    public string gameSeed;  

    private async void Awake(){
        myDisplayName = "Player_" + UnityEngine.Random.Range(1, 50);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Bu nesneyi sahne değişimlerinde yok olmaktan koru


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
            playerId = AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex){
            Debug.Log(ex.Message);
        }
        catch (RequestFailedException ex){
            Debug.Log(ex.Message);
        }
    }

    public async Task LobbyCreate(){
        if (playerId == null){
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
                    {
                        "oyunTipi",
                        new DataObject(DataObject.VisibilityOptions.Public,
                            OyunKurallari.Instance.GuncelOyunTipi.ToString())
                    }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, options);
            StartHeartbeat();
            LobbyListUI.Instance.CreatedLobiCodeTxt.text = CurrentLobby.LobbyCode;
            LobbyListUI.Instance.CloseLobbyBtn.style.display = DisplayStyle.Flex;
            LobbyListUI.Instance.CreateLobbyBtn.style.display = DisplayStyle.None;
            LobbyListUI.Instance.StartRelay.style.display = DisplayStyle.Flex;

            hostCallBacks = new LobbyEventCallbacks();
            hostCallBacks.PlayerJoined += OnPlayerJoined;
            hostCallBacks.PlayerLeft += OnPlayerLeft;
            hostCallBacks.DataChanged += LobiVerisiDegisti;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, hostCallBacks);
            // lobiden düşen olabilir. listeyi güncelleme için belli aralıklarda çalışan coruotin
            if (lobbyUpdateCoroutine == null){
                lobbyUpdateCoroutine = StartCoroutine(UpdateLobbyLoop());
            }  else{
                StopCoroutine(lobbyUpdateCoroutine);
                lobbyUpdateCoroutine = null;
            }
        }
        catch (Exception e){
            Debug.Log(e.Message);
        }
    }

    private IEnumerator UpdateLobbyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // her 10 saniyede bir bekle 
            _ = UpdateLobbyAsync();
        }
    }
    
    private async Task UpdateLobbyAsync()
    {
        try
        {
            CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            LobbyListUI.Instance.RefreshPlayerList();
            Debug.Log("Lobby güncellendi.");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning($"Lobby güncelleme hatası: {ex.Message}");
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
        try{
            // 1. Lobi bilgilerini al (henüz katılmadan)
            var lobbyInfo = await LobbyService.Instance.GetLobbyAsync(lobbyID);
        
            // 2. Mevcut oyuncu sayısı ile maxPlayers’ı karşılaştır
            if (lobbyInfo.Players.Count >= lobbyInfo.MaxPlayers) {
                Debug.Log("Lobi dolu, katılamazsınız.");
                return false;
            }
            
            var options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "DisplayName",
                            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, myDisplayName)
                        }
                    }
                }
            }; 

            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, options);
            CurrentLobby = joinedLobby;

            // eğer lobyde data değişimi olursa tetiklenen Listener
            clientCallbacks = new LobbyEventCallbacks();
            clientCallbacks.DataChanged += LobiVerisiDegisti;
            clientCallbacks.PlayerLeft += OnClientPlayerLeft;
            clientCallbacks.LobbyChanged += OnLobbyChangedForBtn;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, clientCallbacks);

            // ilk girişte lobideki datayı al
            if (joinedLobby.Data.TryGetValue("oyunTipi", out var relayData) &&
                !string.IsNullOrEmpty(relayData.Value)){
                if (Enum.TryParse<OyunKurallari.OyunTipleri>(relayData.Value, out var oyunTipi)){
                    OyunKurallari.Instance.GuncelOyunTipi = oyunTipi;
                }
                else{
                    Debug.LogWarning("Geçersiz oyun tipi: " + relayData.Value);
                }
            }
        }
        catch (LobbyServiceException e){
            Debug.Log(e.Message);
            return false;
        }

        return true;
    }

    private void OnLobbyChangedForBtn(ILobbyChanges obj){
        _ = OnLobbyChanged(obj);
    }
    
    private async Task OnLobbyChanged(ILobbyChanges changes){
        // Data değişikliklerini kontrol et
        if (changes != null && changes.Data.Value != null){
            var lobbyData = changes.Data.Value;
            if (lobbyData != null && lobbyData.ContainsKey("lobby_message")){
                string messageValue = lobbyData["lobby_message"].Value.Value;
                if (messageValue == "lobby_kapanacak"){
                    Debug.Log("1 Lobi kapanacak mesajı alındı, çıkış yapılıyor...");
                    LobbyListUI.Instance.LobiList.Clear();
                    Debug.Log("PublicList.Clear()");
                    await LobbyListUI.Instance.LobidenAyril();
                    LobbyListUI.Instance.OnLobbyListButtonClicked();
                    CurrentLobby = null;
                    Debug.Log(" CurrentLobby " + CurrentLobby);
                }
            }
        }
    }

    private void LobiVerisiDegisti(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> data){
        // GAmeSeedini al
        if (data.TryGetValue("GameSeed", out var GameSeedData)){
            string gameSeedData = GameSeedData.Value.Value;
            if (!isRelayActive){
                gameSeed = gameSeedData;
                Debug.Log("CLIENT RANDOM SEED ALINDI: " + gameSeed + "");
            }
        }
        
        // Relay kodunu al ve relayı başlat
        if (data.TryGetValue("RelayCode", out var relayCodeData)){
            string newRelayCode = relayCodeData.Value.Value;
            if (relayIDForJoin != newRelayCode){
                relayIDForJoin = newRelayCode;  
                if (!isRelayActive){
                    Debug.Log("Relay kodu alındı");
                    _ = StartClientWithRelay(newRelayCode, "dtls");
                }
            }
        } 
    }
    
 

    private void OnClientPlayerLeft(List<int> playerIds){
        Debug.Log("Ben lobiden atıldım mı kontrol ediliyor...");
        if (CurrentLobby.Players.Any(p => p.Id == AuthenticationService.Instance.PlayerId)){
            Debug.Log("Bu client lobiden atıldı, lobby listesi yenileniyor.");
            LobbyListUI.Instance.OnLobbyListButtonClicked();
        }
    }
    
    private void OnPlayerJoined(List<LobbyPlayerJoined> players){
        _ = HandlePlayerJoinedAsync(players);
    }

    private void OnPlayerLeft(List<int> playerIds){
        _ = HandlePlayerLeftAsync(playerIds);
    }

    private async Task HandlePlayerJoinedAsync(List<LobbyPlayerJoined> players){
        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
        CurrentLobby = updatedLobby;
        LobbyListUI.Instance.RefreshPlayerList();
    }

    private async Task HandlePlayerLeftAsync(List<int> playerIds){
        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
        CurrentLobby = updatedLobby;
        LobbyListUI.Instance.RefreshPlayerList();
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
            if (CurrentLobby != null){
                LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }

            yield return new WaitForSeconds(15f); // her 15 saniyede bir ping
        }
    }


    public async Task StartHostWithRelay()
    {
        if (CurrentLobby.HostId != AuthenticationService.Instance.PlayerId)
        {
            return;
        }

        if (isRelayActive || NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            isRelayActive = false;
            await Task.Delay(300);
        }

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
            var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Lobby'nin relay koduyla güncellenmesi
            gameSeed = GetRandomSeed();
            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) },
                    { "GameSeed" , new DataObject(DataObject.VisibilityOptions.Public , gameSeed)}
                }
            });

            isRelayActive = true;
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host Relay Başlatıldı");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private string GetRandomSeed(){ 
        int length = 4;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        System.Random random = new System.Random();
        var seed = new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray()); 
        Debug.Log("RANDOM SEED OLUŞTURULDU: " + seed + "");
        return  seed;
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
        connectedClients.Add(clientId);
        StartGameClientRpc();
    }

    [ClientRpc]
    void StartGameClientRpc(){ 
        Debug.Log("StartGameClientRpc çağrıldı");

        if (IsHost)
        {
            // Sadece host bu kontrolü yapacak
            StartCoroutine(HostBeklesinVeOyunBaslasin());
        }
        else
        {
            // Diğer client'lar direkt sahne yükleyebilir (istersen senkronizasyon eklersin)
            NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
            Debug.Log("Client sahne bekliyor (veya yükleniyor)");
        }
    } 
    
    private IEnumerator HostBeklesinVeOyunBaslasin()
    {
        Debug.Log("Host oyuncu sayısını bekliyor...");

        // 2 oyuncuya (host + 1 client) ulaşılana kadar bekle
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count >= 2);

        Debug.Log("Oyuncu sayısı 2 oldu. Sahne yükleniyor...");
        NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
    }


    //  /////////////////////////////////////////  LOBBY SİLME İŞLEMLERİ ////////////////////////////////////

    public async void OyunculariCikartVeLobiyiSil(string mevcutLobiId){
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(mevcutLobiId);
        if (lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId) // Host kontrolü ekledik
        {
            var updateData = new Dictionary<string, DataObject>
            {
                { "lobby_message", new DataObject(DataObject.VisibilityOptions.Public, "lobby_kapanacak") }
            };
            await LobbyService.Instance.UpdateLobbyAsync(mevcutLobiId, new UpdateLobbyOptions { Data = updateData });

            try{
                await LobbyService.Instance.DeleteLobbyAsync(mevcutLobiId);
                Debug.Log($"Lobi başarıyla silindi: {mevcutLobiId}");
                LobbyListUI.Instance.CloseLobbyBtn.style.display = DisplayStyle.None;
                LobbyListUI.Instance.CreateLobbyBtn.style.display = DisplayStyle.Flex;
                LobbyListUI.Instance.StartRelay.style.display = DisplayStyle.None;
                LobbyListUI.Instance.CreatedLobiCodeTxt.text = null;
                LobbyListUI.Instance.PlayerList.Clear();

                if (hostCallBacks != null){
                    hostCallBacks.PlayerJoined -= OnPlayerJoined;
                    hostCallBacks.PlayerLeft -= OnPlayerLeft; 
                    hostCallBacks.DataChanged -= LobiVerisiDegisti;
                } 
                CurrentLobby = null;
                StopHeartbeat();
                StopCoroutine(lobbyUpdateCoroutine);
                lobbyUpdateCoroutine = null;
            }
            catch (LobbyServiceException e){
                Debug.LogError($" Lobi silinirken bir hata oluştu: {e.Message}");
            }
        }
    }

    //  ---------------------------     LOBBY SİLME İŞLEMLERİ  SON ////////////////////////////////////
}
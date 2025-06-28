using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyManager : NetworkBehaviour{
    
    public static LobbyManager Instance;
    public string relayIDForJoin;
    private LobbyEventCallbacks clientCallbacks;
    public Lobby CurrentLobby; 
    private const int MaxPlayers = 10;
    private const string LobbyName = "okey";
    private Coroutine heartbeatCoroutine;
    public string myDisplayName;
    private LobbyEventCallbacks hostCallBacks;
    public Coroutine lobbyUpdateCoroutine;
    public string gameSeed;
    private bool IsGameStarted = false;
    const float LOBBY_LISTESINI_GUNCELLEME_PERYODU = 15f;
    public GameObject networkPlayerPrefab;
    public string AvadarID;
    
    private void Awake(){
        myDisplayName = "Player_" + UnityEngine.Random.Range(1, 50);
        AvadarID = "avatar" + UnityEngine.Random.Range(0, 6);

        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Bu nesneyi sahne değişimlerinde yok olmaktan koru
    }

    private void OnDisable(){
        StopHeartbeat();
    }

    public async Task LobbyCreate(){
        try{
            if (CurrentLobby != null){
                if (IsHost){
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                    StopHeartbeat();
                }
                else{
                    await LobbyListUI.Instance.LobidenAyril();
                }
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
            LobbyListUI.Instance.CreatedLobiCodeTxt.text = $"{OyunKurallari.Instance.GuncelOyunTipi.ToString()} : {CurrentLobby.LobbyCode}";
            LobbyListUI.Instance.CloseLobbyBtn.style.display = DisplayStyle.Flex; 
            LobbyListUI.Instance.StartRelay.style.display = DisplayStyle.Flex; 
            LobbyListUI.Instance.CrtLobBtn.visible = false;
            LobbyListUI.Instance.benLobininSahibiyim = true;

            hostCallBacks = new LobbyEventCallbacks();
            hostCallBacks.PlayerJoined += OnPlayerJoined;
            hostCallBacks.PlayerLeft += OnPlayerLeft;
            hostCallBacks.DataChanged += LobiVerisiDegisti;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, hostCallBacks);
            // lobiden düşen olabilir. listeyi güncelleme için belli aralıklarda çalışan coruotin
            if (lobbyUpdateCoroutine == null){
                lobbyUpdateCoroutine = StartCoroutine(UpdateLobbyLoop());
            }
            else{
                StopCoroutine(lobbyUpdateCoroutine);
                lobbyUpdateCoroutine = null;
            }

            // host single player oynarsa diye başka bir esprisi yok
            OyunKurallari.Instance.InitializeSettings();
        }
        catch (Exception e){
            Debug.Log(e.Message);
        }
    }

    private IEnumerator UpdateLobbyLoop(){
        // host a görünen lobideki player list
        while (true){
            yield return new WaitForSeconds(LOBBY_LISTESINI_GUNCELLEME_PERYODU); // her 10 saniyede bir bekle 
            _ = UpdateLobbyAsync();
        }
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
    
    public async Task StartHostWithRelay(){
        try{
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening){
                NetworkManager.Singleton.Shutdown();  
                // Şu satırı ekle: Shutdown tamamlanana kadar bekle
                while (NetworkManager.Singleton.IsListening)
                    await Task.Delay(100);
            } 
            
            // NetworkManager prefab’ı sahnede yoksa instansela
            if (NetworkManager.Singleton == null){
                Instantiate(networkPlayerPrefab); // Inspector’dan atadığınız prefab
            }

            if (CurrentLobby.HostId != AuthenticationService.Instance.PlayerId){
                return;
            }

            try{
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                gameSeed = MainMenu.GetRandomSeed();

                await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) },
                        { "GameSeed", new DataObject(DataObject.VisibilityOptions.Public, gameSeed) },
                        { "isGameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                    }
                });
 
                if (!NetworkManager.Singleton.IsListening){
                    NetworkManager.Singleton.StartHost();
                }
            }
            catch (LobbyServiceException ex){
                Debug.LogError($"LobbyServiceException: {ex.Message}");
                Debug.LogError($"ErrorCode: {ex.ErrorCode}, Reason: {ex.Reason}, Message: {ex.Message}");
            }
        }
        catch (Exception ex){
            Debug.LogError("Genel hata: " + ex.ToString());
        }
    }
    
    public async Task StartClientRelay(string joinCode, string connectionType){
        if (IsHost) return;
        try{
            var nm = NetworkManager.Singleton;

            if (nm.IsListening){
                nm.Shutdown();

                while (nm.IsListening)
                    await Task.Delay(100);
            }

            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            nm.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, connectionType));
            nm.StartClient();
        }
        catch (Exception ex){
            Debug.LogError($"StartClientRelay HATASI: {ex.Message}");
        } 
    }

    private async Task UpdateLobbyAsync(){
        try{
            CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            LobbyListUI.Instance.RefreshPlayerList();
        }
        catch (LobbyServiceException ex){
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
            return null;
        }
    }

    public async Task<bool> JoinLobbyByID(string lobbyID){
        if (string.IsNullOrEmpty(lobbyID)) return false;
        try{
            // 1. Lobi bilgilerini al (henüz katılmadan)
            var lobbyInfo = await LobbyService.Instance.GetLobbyAsync(lobbyID);

            // 2. Mevcut oyuncu sayısı ile maxPlayers’ı karşılaştır
            if (lobbyInfo.Players.Count >= lobbyInfo.MaxPlayers){
                return false;
            }

            // 3.   Lobiyi al ve metadata'ya eriş

            if (lobbyInfo.Data.ContainsKey("isGameStarted") &&
                lobbyInfo.Data["isGameStarted"].Value == "true"){ 
                return false;
            }
            else{
                //Debug.Log("Oyun henüz başlamadı.");
            }


            var options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, myDisplayName) },
                        { "avatar", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AvadarID) }
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
                    OyunKurallari.Instance.InitializeSettings(); 
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
                    LobbyListUI.Instance.LobbyList.Clear();
                    await LobbyListUI.Instance.LobidenAyril();
                    await LobbyListUI.Instance.OnLobbyListButtonClicked();
                    CurrentLobby = null;
                }
            }
        }
    }

    private void LobiVerisiDegisti(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> data){
        if (data.TryGetValue("isGameStarted", out var isGameStarted)){
            IsGameStarted = (isGameStarted.Value.Value == "true") ? true : false;
        }

        if (data.TryGetValue("GameSeed", out var GameSeedData)){
            string gameSeedData = GameSeedData.Value.Value;
            gameSeed = gameSeedData;
        }

        if (data.TryGetValue("RelayCode", out var relayCodeData)){
            string newRelayCode = relayCodeData.Value.Value;
            if (relayIDForJoin != newRelayCode){
                relayIDForJoin = newRelayCode;
                _ = StartClientRelay(newRelayCode, "dtls");
            }
        }
    }

    private void OnClientPlayerLeft(List<int> playerIds){
        if (CurrentLobby.Players.Any(p => p.Id == AuthenticationService.Instance.PlayerId)){
            _ = LobbyListUI.Instance.OnLobbyListButtonClicked();
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
    
    public override void OnNetworkSpawn(){
        if (IsHost){
            NetworkManager.Singleton.SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
        }
    }

    // LOBBY SİLME İŞLEMLERİ
    public async Task OyunculariCikartVeLobiyiSil(string mevcutLobiId){
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
                LobbyListUI.Instance.CloseLobbyBtn.style.display = DisplayStyle.None;
                //LobbyListUI.Instance.CreateLobbyBtn.style.display = DisplayStyle.Flex;
                LobbyListUI.Instance.StartRelay.style.display = DisplayStyle.None;
                LobbyListUI.Instance.CreatedLobiCodeTxt.text = null;
                LobbyListUI.Instance.PlayerList.Clear();
                //LobbyListUI.Instance.HostListBtn.style.display = DisplayStyle.Flex; 
                //LobbyListUI.Instance.CrtLobBtn.style.display = DisplayStyle.Flex;
                LobbyListUI.Instance.CrtLobBtn.visible = true;
                LobbyListUI.Instance.OnLobbyListButtonClickedWrapper();

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
    // LOBBY SİLME İŞLEMLERİ 
    
    public async Task CikisIsteginiGonder(){
        GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
        // soloysa hemen çık
        if (MainMenu.isSoloGame){
            SceneManager.LoadScene("LobbyManager");
            return;
        }
        // 1) Lobby bayrağını sıfırla (sadece host)
        if (IsHost){
            await LobbyService.Instance.UpdateLobbyAsync(
                CurrentLobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            "isGameStarted",
                            new DataObject(DataObject.VisibilityOptions.Public, "false")
                        }
                    }
                });
            SceneManager.LoadScene("LobbyManager");
            return;
        }

        if (IsClient){  
            var nm = NetworkManager.Singleton;  
            if (nm != null && nm.IsListening){
                nm.Shutdown(); 
                while (nm.IsListening)
                    await Task.Delay(100);
            } 
            if (nm is not null)
                Destroy(nm.gameObject);
            await Task.Yield(); 
            NetcodeBootstrapper.CleanUp();
            SceneManager.LoadScene("MainMenu");
        }
        
 
   
    }

    public void AbonelikeriBitir(){
        if (clientCallbacks != null){
            clientCallbacks.DataChanged -= LobiVerisiDegisti;
            clientCallbacks.PlayerLeft -= OnClientPlayerLeft;
            clientCallbacks.LobbyChanged -= OnLobbyChangedForBtn;
            clientCallbacks = null; // Callback referansını temizle (isteğe bağlı) 
        }
    }

    public void StartSolo(){  
        SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
    }
}
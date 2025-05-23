using System;
using System.Collections; 
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using TextElement = UnityEngine.UIElements.TextElement;


public class LobbyListUI : MonoBehaviour{
    public static LobbyListUI Instance;
    public Button CreateLobbyBtn;
    public Button CrtLobBtn;
    public Button CloseLobbyBtn;
    public Button HostListBtn;
    public Button QuitToMainMenu;
    public TextElement CreatedLobiCodeTxt;
    public VisualElement HostList;
    public VisualElement CreateHostWindow;
    public QueryResponse response;
    public VisualElement PlayerList;
    public Button StartRelay;
    public bool joinedToLobby = false;
    private VisualElement rootElement;
    public Button katilBtn;
    public Button ayrilBtn;
    public Coroutine lobbyListUpdateCoroutine;
    public bool benLobininSahibiyim = false;
    const float LOBBY_LISTESINI_GUNCELLEME_PERYODU = 10f;
    
 

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }
        Instance = this; 

    }

    private void OnDisable(){
        if (lobbyListUpdateCoroutine!=null){ 
            StopCoroutine(lobbyListUpdateCoroutine); 
        }
        HostListBtn.clicked -= OnLobbyListButtonClickedWrapper;
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        HostList = rootElement.Q<VisualElement>("LobbyList");
        CreateHostWindow = rootElement.Q<VisualElement>("CreateHostWindow");
        CrtLobBtn = rootElement.Q<Button>("CrtLobBtn");
        HostListBtn = rootElement.Q<Button>("HostListBtn");
        CreateLobbyBtn = rootElement.Q<Button>("CreateLobbyBtn");
        CloseLobbyBtn = rootElement.Q<Button>("CloseLobby");
        CreatedLobiCodeTxt = rootElement.Q<TextElement>("CreatedLobiCodeTxt");
        PlayerList = rootElement.Q<VisualElement>("PlayerList");
        StartRelay = rootElement.Q<Button>("StartRelay");
        QuitToMainMenu = rootElement.Q<Button>("QuitToMainMenu");
        StartRelay.style.display = DisplayStyle.None; 
        benLobininSahibiyim = LobbyManager.Instance.CurrentLobby?.HostId == AuthenticationService.Instance.PlayerId;
        
        // Lobby Yaratma Butonu
        CreateLobbyBtn.style.display = (benLobininSahibiyim) ? DisplayStyle.None : DisplayStyle.Flex;
        CreateLobbyBtn.clicked += CreateLobby;

        // Lobby Kapatma Butonu 
        CloseLobbyBtn.style.display = (benLobininSahibiyim) ? DisplayStyle.Flex : DisplayStyle.None;
        CloseLobbyBtn.clicked += LobimiKapat;

        // Lobby Create Penceresi
        CrtLobBtn.clicked += LobiOlusturmaPenceresi;

        // loby listesi Penceresi
        HostListBtn.clicked += OnLobbyListButtonClickedWrapper;

        //start Relay
        StartRelay.clicked += async () => {
            await LobbyManager.Instance.StartHostWithRelay();
        };
        
        //AnaMenüye Dön
        QuitToMainMenu.clicked += AnaMenuyeDon;
        
        if (LobbyManager.Instance?.CurrentLobby!=null){
            if (benLobininSahibiyim){
                //CreatedLobiCodeTxt.text = OyunKurallari.Instance.GuncelOyunTipi.ToString() + " -- "+ LobbyManager.Instance.CurrentLobby.LobbyCode;
                CreatedLobiCodeTxt.text = $"Oyun Türü : {OyunKurallari.Instance.GuncelOyunTipi.ToString()}";
                HostListBtn.style.display = DisplayStyle.None;
                RefreshPlayerList();
            }
            else{    
                HostList.style.display = DisplayStyle.None;
                OnLobbyListButtonClicked(); 
            }
        }
        
        // hemen lobileri listele
        OnLobbyListButtonClickedWrapper();
    }

    private void LobimiKapat(){
        LobbyManager.Instance?.OyunculariCikartVeLobiyiSil(LobbyManager.Instance?.CurrentLobby.Id);
    }

    private void LobiOlusturmaPenceresi(){
        CreateHostWindow.style.display = DisplayStyle.Flex;
        HostList.style.display = DisplayStyle.None; 
        CreateLobby();
    }

    private async void CreateLobby(){
        var loading = YukleniyorBekleniyorMesajBox.Instance.CreateLoadingElement("Bağlanıyor..."); 
        CreateHostWindow.Insert(0, loading);
        await LobbyManager.Instance?.LobbyCreate();
        CreateHostWindow.Remove(loading);
    }

    private async void AnaMenuyeDon(){
        if (LobbyManager.Instance?.CurrentLobby != null){
            if (benLobininSahibiyim){
                LobbyManager.Instance?.OyunculariCikartVeLobiyiSil(LobbyManager.Instance?.CurrentLobby.Id);
            }
            else{
                await LobidenAyril(); 
            }
        }

        if (LobbyManager.Instance?.lobbyUpdateCoroutine != null){
            LobbyManager.Instance.StopCoroutine(LobbyManager.Instance.lobbyUpdateCoroutine);
            LobbyManager.Instance.lobbyUpdateCoroutine = null;
        }

        HostList.Clear();
        PlayerList.Clear();
        CreatedLobiCodeTxt.text = null;
        SceneManager.LoadScene("MainMenu");
    }

    public async void OnLobbyListButtonClickedWrapper(){
        CreateHostWindow.style.display = DisplayStyle.None;
        HostList.style.display = DisplayStyle.Flex;
        HostListBtn.style.display = DisplayStyle.None;
        var loading = YukleniyorBekleniyorMesajBox.Instance.CreateLoadingElement("Listeleniyor...");
        rootElement.Add(loading);
        await OnLobbyListButtonClicked(); 
        rootElement.Remove(loading);
    }

    public async Task OnLobbyListButtonClicked(){ 
        try{
            response = await LobbyManager.Instance.GetLobbyList();
            if (response != null){
                HostList.Clear();
                for (int i = 0; i < response.Results.Count; i++){
                    var lobby = response.Results[i];
                    if (LobbyManager.Instance.CurrentLobby?.HostId == AuthenticationService.Instance.PlayerId)
                        continue; 
                    var row = HostListRow(lobby);
                    HostList.Add(row);
                    row.AddToClassList("lobbyListRow");
                    HostListBtn.style.display = DisplayStyle.None;
                }

                if (response.Results.Count==0){
                    {
                        var label = new Label("Lobi Bulunamadı.");
                        label.style.fontSize = 10;
                        label.style.color = Color.white;
                        label.style.unityTextAlign = TextAnchor.MiddleCenter;
                        label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        label.style.position = new StyleEnum<Position>(Position.Absolute);
                        label.style.top       = Length.Percent(50);   // Dikey merkez
                        label.style.left      = Length.Percent(50);   // Yatay merkez
                        label.style.width        = Length.Percent(100); 
                        label.style.height        = 32; 
                        label.style.translate = new StyleTranslate(
                            new Translate(
                                new Length(-50, LengthUnit.Percent),              // -%50 X
                                new Length(-50, LengthUnit.Percent),              // -%50 Y
                                0));
                        HostList.Insert(0,label);
                        //HostListBtn.style.display = DisplayStyle.Flex;
                    } 
                }
            }
        }
        catch (Exception e){
            Debug.Log($"OnLobbyListButtonClicked hata eydan geldin {e.Message}"); 
        }

        if (lobbyListUpdateCoroutine==null){
            lobbyListUpdateCoroutine = StartCoroutine(LobbyListUpdateLoop());
        }  
    }

    private IEnumerator LobbyListUpdateLoop(){
        while (true){
            yield return new WaitForSeconds(LOBBY_LISTESINI_GUNCELLEME_PERYODU); // her 10 saniyede bir bekle 
            OnLobbyListButtonClicked();
        }
    }
    
    private VisualElement HostListRow(Lobby lobby){
        var benLobidemiyim =  lobby.Players.Any(p => p.Id == AuthenticationService.Instance.PlayerId);

        var lobbyID = lobby.Id;
        var row = new VisualElement();
        string oyunTipi = null;
        
        if (lobby.Data.TryGetValue("oyunTipi", out var relayData)){
            oyunTipi = relayData.Value;
        }
 
        katilBtn = new Button { text = $"{oyunTipi} - {lobby.Players.Count}/{lobby.MaxPlayers}" }; 
        ayrilBtn = new Button(); 
        katilBtn.style.display  = benLobidemiyim ? DisplayStyle.None : DisplayStyle.Flex;
        ayrilBtn.style.display  = benLobidemiyim ? DisplayStyle.Flex : DisplayStyle.None;
        katilBtn.AddToClassList("katilBtn");
        ayrilBtn.AddToClassList("ayrilBtn");
        
        katilBtn.clicked += async () => await OnJoinLobbyClicked(lobbyID);
        ayrilBtn.clicked += async () => await OnLeaveLobbyClicked();  
        row.Add(katilBtn);
        row.Add(ayrilBtn); 
        return row;
    }

    private async Task OnJoinLobbyClicked(string lobbyID){
        try{
            
            joinedToLobby = await LobbyManager.Instance.JoinLobbyByID(lobbyID);
            ayrilBtn.style.display = joinedToLobby ? DisplayStyle.Flex : DisplayStyle.None;
            katilBtn.style.display = joinedToLobby ? DisplayStyle.None : DisplayStyle.Flex;
            CrtLobBtn.style.display = joinedToLobby ? DisplayStyle.None : DisplayStyle.Flex; 
        }
        catch (Exception e){
            Console.WriteLine($"Hata OnJoinLobbyClicked içinde {e.Message}");
        }
    }

    private async Task OnLeaveLobbyClicked(){
        await LobidenAyril();
    }

    public async Task LobidenAyril(){
        if (LobbyManager.Instance.CurrentLobby == null)
            return;
        try{
            await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance.CurrentLobby.Id,
                AuthenticationService.Instance.PlayerId);
            ayrilBtn.style.display = DisplayStyle.None;
            katilBtn.style.display = DisplayStyle.Flex;
            HostListBtn.style.display = DisplayStyle.None;
            CrtLobBtn.style.display = DisplayStyle.Flex;
            LobbyManager.Instance.CurrentLobby = null; 
            LobbyManager.Instance.AbonelikeriBitir();
        }
        catch (Exception e){
            Console.WriteLine($"LobidenAyril başarısız {e.Message}");
        }
    }
    
    public void RefreshPlayerList(){  
        var players = LobbyManager.Instance.CurrentLobby.Players;
        StartRelay.style.display = players == null ? DisplayStyle.None : DisplayStyle.Flex;
        PlayerList.Clear();
        foreach (var player in players){
            if (player.Data == null){
                Debug.LogWarning("Player.Data null! DisplayName atanamadı.");
                continue; // Null gelen veriyi atla
            }

            var name = player.Data.ContainsKey("DisplayName")
                ? player.Data["DisplayName"].Value
                : "Bilinmeyen Oyuncu";

            var label = new Label(name);

            PlayerList.Add(label);
        }
    }
 
}
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine; 
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using TextElement = UnityEngine.UIElements.TextElement;


public class LobbyListUI : MonoBehaviour{
    public static LobbyListUI Instance;
    public Button CreateLobbyBtn;
    public Button CrtLobBtn;
    public Button CloseLobbyBtn;
    public Button LobyListBtn;
    public TextElement CreatedLobiCodeTxt;
    public VisualElement PublicList;
    public VisualElement CreateLobby;
    public QueryResponse response; 
    public VisualElement PlayerList;
    public Button StartRelay;
    public bool joinedToLobby = false;
    private VisualElement rootElement;
    public Button katilBtn;
    public Button ayrilBtn;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        PublicList = rootElement.Q<VisualElement>("PublicList");
        CreateLobby = rootElement.Q<VisualElement>("CreateLobby");
        CrtLobBtn = rootElement.Q<Button>("CrtLobBtn");
        LobyListBtn = rootElement.Q<Button>("LobyListBtn");
        CreateLobbyBtn = rootElement.Q<Button>("CreateLobbyBtn");
        CloseLobbyBtn = rootElement.Q<Button>("CloseLobby");
        CreatedLobiCodeTxt = rootElement.Q<TextElement>("CreatedLobiCodeTxt");
        PlayerList = rootElement.Q<VisualElement>("PlayerList");
        StartRelay = rootElement.Q<Button>("StartRelay");
        StartRelay.style.display = DisplayStyle.None;

        // Lobby Yaratma Butonu
        CreateLobbyBtn.style.display = (LobbyManager.Instance?.CurrentLobby == null)?DisplayStyle.Flex:DisplayStyle.None;
        CreateLobbyBtn.clicked += () => { _ = LobbyManager.Instance?.LobbyCreate(); };
        
        // Lobby Kapatma Butonu 
        CloseLobbyBtn.style.display = (LobbyManager.Instance?.CurrentLobby == null)?DisplayStyle.None:DisplayStyle.Flex;
        CloseLobbyBtn.clicked += () => {    LobbyManager.Instance?.OyunculariCikartVeLobiyiSil(LobbyManager.Instance?.CurrentLobby.Id); }; 

        // Lobby Create Penceresi
        CrtLobBtn.clicked += () => {
            CreateLobby.visible = true;
            PublicList.visible = false;
        };

        // loby listesi Penceresi
        LobyListBtn.clicked += OnLobbyListButtonClicked;
         
        //start Relay
        StartRelay.clicked += async () => {
            var maxConnections = 4;
            await LobbyManager.Instance.StartHostWithRelay(maxConnections);  
        };
    }

    public async void OnLobbyListButtonClicked()
    {
        PublicList.visible = true;
        CreateLobby.visible = false;

        response = await LobbyManager.Instance.GetLobbyList();
        if (response != null)
        {
            PublicList.Clear();
            for (int i = 0; i < response.Results.Count; i++)
            {
                var lobby = response.Results[i];
                if (LobbyManager.Instance.CurrentLobby?.HostId == AuthenticationService.Instance.PlayerId)
                    continue;

                var row = CreateLobbyRow(lobby);
                PublicList.Add(row);
                row.AddToClassList("lobbyListRow");
            }
        }
    }

    private VisualElement CreateLobbyRow(Lobby lobby)
    {
        var lobbyID = lobby.Id;
        var row = new VisualElement();
        var label = new Label
        {
            text = $"{lobby.Name} - {lobby.Players.Count}/{lobby.MaxPlayers}"
        };

        katilBtn = new Button { text = "Katıl" };
        ayrilBtn = new Button { text = "Ayrıl", style = { display = DisplayStyle.None } };

        katilBtn.clicked += async () => await OnJoinLobbyClicked(lobbyID, katilBtn, ayrilBtn);
        ayrilBtn.clicked += async () => await OnLeaveLobbyClicked(katilBtn, ayrilBtn);

        row.Add(label);
        row.Add(katilBtn);
        row.Add(ayrilBtn);

        return row;
    }

    private async Task OnJoinLobbyClicked(string lobbyID, Button katilBtn, Button ayrilBtn)
    {
        bool joinedToLobby = await LobbyManager.Instance.JoinLobbyByID(lobbyID);
        ayrilBtn.style.display = joinedToLobby ? DisplayStyle.Flex : DisplayStyle.None;
        katilBtn.style.display = joinedToLobby ? DisplayStyle.None : DisplayStyle.Flex;
    }
    
    private async Task OnLeaveLobbyClicked(Button katilBtn, Button ayrilBtn)
    {
        if (LobbyManager.Instance.CurrentLobby == null) return;

        await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance.CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
        ayrilBtn.style.display = DisplayStyle.None;
        katilBtn.style.display = DisplayStyle.Flex;
    }

    
    public void RefreshPlayerList()
    {
        var players = LobbyManager.Instance.CurrentLobby.Players;
        StartRelay.style.display = players == null?DisplayStyle.None:DisplayStyle.Flex;
        PlayerList.Clear();
        foreach (var player in players)
        {
            if (player.Data == null)
            {
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
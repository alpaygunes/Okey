using System.Threading.Tasks;
using Unity.Netcode;
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
    public Button LobyListBtn;
    public TextElement CreatedLobiCodeTxt;
    public VisualElement PublicList;
    public VisualElement CreateLobby;
    public QueryResponse response; 
    public VisualElement PlayerList;
    public Button StartRelay;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void OnEnable(){
        VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
        PublicList = rootElement.Q<VisualElement>("PublicList");
        CreateLobby = rootElement.Q<VisualElement>("CreateLobby");
        CrtLobBtn = rootElement.Q<Button>("CrtLobBtn");
        LobyListBtn = rootElement.Q<Button>("LobyListBtn");
        CreateLobbyBtn = rootElement.Q<Button>("CreateLobbyBtn");
        CreatedLobiCodeTxt = rootElement.Q<TextElement>("CreatedLobiCodeTxt");
        PlayerList = rootElement.Q<VisualElement>("PlayerList");
        StartRelay = rootElement.Q<Button>("StartRelay");
        StartRelay.style.display = DisplayStyle.None;

        // Lobby Yaratma Butonu
        CreateLobbyBtn.clicked += () => { _ = LobbyManager.Instance.CreateLobi(); };

        // Lobby Create Penceresi
        CrtLobBtn.clicked += () => {
            CreateLobby.visible = true;
            PublicList.visible = false;
        };

        // loby listesi Penceresi
        LobyListBtn.clicked += async () => {
            PublicList.visible = true;
            CreateLobby.visible = false;

            response = await LobbyManager.Instance.GetLobbyList();
            if (response != null){
                PublicList.Clear(); 
                for (int i = 0; i < response.Results.Count; i++){
                    var lobby = response.Results[i];
                    var lobbyID = lobby.Id;
                    var row = new VisualElement();
                    var label = new Label();
                    label.text = $"{lobby.Name} - {lobby.Players.Count}/{lobby.MaxPlayers}";
                    var button = new Button();
                    button.text = "Katıl";
                    button.clicked += async () => {
                        await LobbyManager.Instance.JoinLobbyByID(lobbyID);
                    };

                    row.Add(label);
                    row.Add(button);
                    PublicList.Add(row);
                    row.AddToClassList("lobbyListRow"); 
                }
            }
        };
        
        //start Relay
        StartRelay.clicked += async () => {
            var maxConnections = 2;
            await LobbyManager.Instance.StartHostWithRelay(maxConnections);  
        };
    } 
    

    
    public void RefreshPlayerList()
    {
        var players = LobbyManager.Instance.currentLobby.Players;
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
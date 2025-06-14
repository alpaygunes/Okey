using System.Threading.Tasks;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UIElements;

public class OyunSahnesiUI : MonoBehaviour
{
    private VisualElement rootElement;
    public static OyunSahnesiUI Instance;
    public Label SkorTxt;
    public Label KalanTasSayisi;
    public Label HamleSayisi; 
    public Label GeriSayim;
    private Button exit;
    public Button puanlamaYap; 
    public Label GorevSayisiLbl;
    private VisualElement avatars;
 
    private async Task AvatarlariGoster(){
        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(LobbyManager.Instance.CurrentLobby.Id);
        LobbyManager.Instance.CurrentLobby = updatedLobby;
        avatars.Clear();
        var Players = LobbyManager.Instance.CurrentLobby.Players;
        foreach (var player in Players){
            Button avatarBtn = new Button();
            string avatarName = player.Data.ContainsKey("avatar")
                ? player.Data["avatar"].Value
                : "avatar0";
            string displayName = player.Data.ContainsKey("DisplayName")
                ? player.Data["DisplayName"].Value
                : "NoDisplayName";
            Sprite avatarSprite = Resources.Load<Sprite>($"avatars/{avatarName}");
            if (avatarSprite != null)
                avatarBtn.style.backgroundImage = new StyleBackground(avatarSprite);

            avatarBtn.tooltip = displayName; 
            avatarBtn.AddToClassList("avatar"); 
            avatarBtn.RegisterCallback<GeometryChangedEvent>(e =>
            {
                float h = avatarBtn.resolvedStyle.height;    
                avatarBtn.style.width = h;
            });
            avatarBtn.style.borderTopWidth = 0;
            avatarBtn.style.borderBottomWidth = 0;
            avatarBtn.style.borderLeftWidth = 0;
            avatarBtn.style.borderRightWidth = 0; 
            avatars.Add(avatarBtn);
        }
    }

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        } 
        Instance = this; 
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SkorTxt = rootElement.Q<Label>("Skor");
        KalanTasSayisi = rootElement.Q<Label>("KalanTasSayisi");
        HamleSayisi = rootElement.Q<Label>("HamleSayisi");
        GeriSayim = rootElement.Q<Label>("GeriSayim"); 
        GorevSayisiLbl = rootElement.Q<Label>("GorevSayisi");
        puanlamaYap = rootElement.Q<Button>("PuanlamaYap");
        exit = rootElement.Q<Button>("Exit");
        //GeriSayimBari = rootElement.Q<ProgressBar>("GeriSayimBari");  
        //PregressBarContainer = rootElement.Q<VisualElement>("PregressBarContainer"); 
        avatars = rootElement.Q<VisualElement>("Avtars"); 
        puanlamaYap.clicked += PerleriDegerlendir;  
        puanlamaYap.style.display = DisplayStyle.None;
        //PregressBarContainer.style.display = GameManager.Instance.OtomatikPerkontrolu ? DisplayStyle.Flex : DisplayStyle.None;
        //GeriSayim.style.display =    PregressBarContainer.style.display;
        GorevSayisiLbl.style.display =    DisplayStyle.None; 
        GeriSayim.text = null;
        GorevSayisiLbl.text = null;
        HamleSayisi.text = "0"; 
        HamleSayisi.style.display =    DisplayStyle.None; 
        exit.clicked += () => {
            _ = LobbyManager.Instance.CikisIsteginiGonder();
        }; 
        
 
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            GeriSayim.text = OyunKurallari.Instance.ZamanLimiti.ToString();
            GeriSayim.style.display =    DisplayStyle.Flex;
        } else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            GorevSayisiLbl.style.display =    DisplayStyle.Flex;
            GorevSayisiLbl.text  = "1/"+OyunKurallari.Instance.GorevYap.ToString();
        } else if(OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            HamleSayisi.style.display =    DisplayStyle.Flex;
            HamleSayisi.text  = "1/"+OyunKurallari.Instance.HamleLimit.ToString();
        } 
        _ = AvatarlariGoster();
    }
     
    public void PerleriDegerlendir(){  
        Puanlama.Instance.Puanla();
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
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
    public Button degerlendirmeYap; 
    public Label GorevSayisiLbl;
    public Label CanSayisi;
    private VisualElement avatars;
    
    public Label AltinSayisi; 
    public Label ElmasSayisi; 
 
 
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
        AltinSayisi = rootElement.Q<Label>("AltinSayisi");
        ElmasSayisi = rootElement.Q<Label>("ElmasSayisi");
        CanSayisi = rootElement.Q<Label>("CanSayisi");
        degerlendirmeYap = rootElement.Q<Button>("DegerlendirmeYap");
        exit = rootElement.Q<Button>("Exit"); 
        avatars = rootElement.Q<VisualElement>("Avtars"); 
        degerlendirmeYap.clicked += PerleriDegerlendir;  
        degerlendirmeYap.style.display = DisplayStyle.None; 
        GorevSayisiLbl.style.display =    DisplayStyle.None; 
        CanSayisi.style.display =    DisplayStyle.Flex; 
        GeriSayim.text = null;
        GorevSayisiLbl.text = null;
        CanSayisi.text = null;
        HamleSayisi.text = "0"; 
        HamleSayisi.style.display =    DisplayStyle.None; 
        AltinSayisi.text = "0"; 
        ElmasSayisi.text = "0";
        exit.clicked += () => {
            _ = LobbyManager.Instance.CikisIsteginiGonder();
        }; 
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            GeriSayim.text = OyunKurallari.Instance.ZamanLimiti.ToString();
            GeriSayim.style.display =    DisplayStyle.Flex;
        } else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            GorevSayisiLbl.style.display =    DisplayStyle.Flex;
            GorevSayisiLbl.text  = "1/"+OyunKurallari.Instance.GorevLimit.ToString();
        } else if(OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            HamleSayisi.style.display =    DisplayStyle.Flex;
            HamleSayisi.text  = "1/"+OyunKurallari.Instance.HamleLimit.ToString();
        }  
    }
     
    public void PerleriDegerlendir(){ 
        IsaretleBelirtYoket.Instance.Degerlendir(); 
    }

    public void AvatarSirasiniGuncelle(){
        Debug.Log("Avatar Sirasini Guncellendi ");
        NetworkList<MultiPlayerVeriYoneticisi.PlayerData> oyuncuListesi = MultiPlayerVeriYoneticisi.Instance.OyuncuListesi;
        var localList = new List<MultiPlayerVeriYoneticisi.PlayerData>(); 
        for (int i = 0; i < oyuncuListesi.Count; i++) {
            var oyuncu = oyuncuListesi[i];
            oyuncu.Skor = oyuncu.BonusMeyveSayisi + oyuncu.AltinSayisi + oyuncu.ElmasSayisi; 
            localList.Add(oyuncu);
        }
        var Players = localList.OrderByDescending(p => p.Skor).ToList();
        avatars.Clear(); 
        foreach (var player in Players){
            Button avatarBtn = new Button();
            string avatarName = player.AvadarID != default
                ? player.AvadarID.ToString()
                : "avatar0";
            string displayName = player.ClientName!= default
                ? player.AvadarID.ToString()
                : "NoDisplayName";
            Sprite avatarSprite = Resources.Load<Sprite>($"avatars/{avatarName}");
            if (avatarSprite != null)
                avatarBtn.style.backgroundImage = new StyleBackground(avatarSprite);

            avatarBtn.tooltip = displayName;
            avatarBtn.AddToClassList("avatar");
            avatarBtn.RegisterCallback<GeometryChangedEvent>(e =>{
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
}

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public Button DegerlendirmeYap;
    public Label GorevSayisiLbl;
    public Label CanSayisi;
    private VisualElement avatars;
    public Label AltinSayisi;
    public Label ElmasSayisi;

    public VisualElement OnbilgiPopUp;
    public Label LevelID;
    private Button OynaBtn;
    public Label KalipSayisi;
    public Label RenkSayisi;
    public Label MeyveSayisi;

    public VisualElement SonbilgiPopUp;
    public Label SonBilgi_LevelID;
    private Button SonBilgi_OynaBtn;
    public Label SonBilgi_KalipSayisi;
    public Label SonBilgi_RenkSayisi;
    public Label SonBilgi_MeyveSayisi;
    public Label SonBilgi_YeniLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SkorTxt = rootElement.Q<Label>("Skor");
        //SkorTxt.style.display =    DisplayStyle.None;
        KalanTasSayisi = rootElement.Q<Label>("KalanTasSayisi");
        KalanTasSayisi.style.display = DisplayStyle.None;
        HamleSayisi = rootElement.Q<Label>("HamleSayisi");
        GeriSayim = rootElement.Q<Label>("GeriSayim");
        GeriSayim.style.display = DisplayStyle.None;
        GorevSayisiLbl = rootElement.Q<Label>("GorevSayisi");
        AltinSayisi = rootElement.Q<Label>("AltinSayisi");
        ElmasSayisi = rootElement.Q<Label>("ElmasSayisi");
        CanSayisi = rootElement.Q<Label>("CanSayisi");
        DegerlendirmeYap = rootElement.Q<Button>("DegerlendirmeYap");
        exit = rootElement.Q<Button>("Exit");
        avatars = rootElement.Q<VisualElement>("Avtars");
        DegerlendirmeYap.clicked += PerleriDegerlendir;
        DegerlendirmeYap.style.display = DisplayStyle.None;
        GorevSayisiLbl.style.display = DisplayStyle.None;
        CanSayisi.style.display = DisplayStyle.None;
        GeriSayim.text = null;
        GorevSayisiLbl.text = null;
        CanSayisi.text = null;
        HamleSayisi.text = "0";
        HamleSayisi.style.display = DisplayStyle.None;
        AltinSayisi.text = "0";
        ElmasSayisi.text = "0";
        exit.clicked += () => { _ = LobbyManager.Instance.CikisIsteginiGonder(); };

        OnbilgiPopUp = rootElement.Q<VisualElement>("OnbilgiPopUp");
        LevelID = rootElement.Q<Label>("LevelID");
        MeyveSayisi = rootElement.Q<Label>("MeyveSayisi");
        RenkSayisi = rootElement.Q<Label>("RenkSayisi");
        KalipSayisi = rootElement.Q<Label>("KalipSayisi");
        OynaBtn = rootElement.Q<Button>("Oyna");
        OynaBtn.clicked += () =>
        {
            GameManager.Instance.OyunDurumu = GameManager.OyunDurumlari.DevamEdiyor;
            OnbilgiPopUp.style.display = DisplayStyle.None;
        };

        SonbilgiPopUp = rootElement.Q<VisualElement>("SonbilgiPopUp");
        SonBilgi_LevelID = rootElement.Q<Label>("SonBilgi_LevelID");
        SonBilgi_MeyveSayisi = rootElement.Q<Label>("SonBilgi_MeyveSayisi");
        SonBilgi_RenkSayisi = rootElement.Q<Label>("SonBilgi_RenkSayisi");
        SonBilgi_KalipSayisi = rootElement.Q<Label>("SonBilgi_KalipSayisi");
        SonBilgi_YeniLevel = rootElement.Q<Label>("SonBilgi_YeniLevel");
        SonBilgi_OynaBtn = rootElement.Q<Button>("SonBilgi_Oyna");
        SonBilgi_OynaBtn.clicked += () =>
        {
            SonbilgiPopUp.style.display = DisplayStyle.None;
            SceneManager.LoadScene("LobbyManager", LoadSceneMode.Single);
        };


        if (MainMenu.isSoloGame)
        {
            avatars.style.display = DisplayStyle.None;
        }

        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli)
        {
            GeriSayim.text = OyunKurallari.Instance.ZamanLimiti.ToString();
            GeriSayim.style.display = DisplayStyle.None;
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap)
        {
            GorevSayisiLbl.style.display = DisplayStyle.None;
            GorevSayisiLbl.text = "1/" + OyunKurallari.Instance.GorevLimit.ToString();
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli)
        {
            HamleSayisi.style.display = DisplayStyle.Flex;
            HamleSayisi.text = "1/" + OyunKurallari.Instance.HamleLimit.ToString();
        }

        if (MainMenu.isSoloGame)
        {
            HamleSayisi.style.display = DisplayStyle.Flex;
            HamleSayisi.text = "1/" + GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")].HamleLimiti.ToString();
        }
    }


    public void PerleriDegerlendir()
    {
        IsaretleBelirtYoket.Instance.Degerlendir();
    }

    public void AvatarSirasiniGuncelle()
    {
        NetworkList<MultiPlayerVeriYoneticisi.PlayerData> oyuncuListesi =
            MultiPlayerVeriYoneticisi.Instance.OyuncuListesi;
        var localList = new List<MultiPlayerVeriYoneticisi.PlayerData>();
        for (int i = 0; i < oyuncuListesi.Count; i++)
        {
            var oyuncu = oyuncuListesi[i];
            oyuncu.Skor = oyuncu.BonusMeyveSayisi + oyuncu.AltinSayisi + oyuncu.ElmasSayisi;
            localList.Add(oyuncu);
        }

        var Players = localList.OrderByDescending(p => p.Skor).ToList();
        avatars.Clear();
        foreach (var player in Players)
        {
            Button avatarBtn = new Button();
            string avatarName = player.AvadarID != default
                ? player.AvadarID.ToString()
                : "avatar0";
            string displayName = player.NickName != default
                ? player.NickName.ToString()
                : "NoDisplayName";
            Sprite avatarSprite = Resources.Load<Sprite>($"avatars/{avatarName}");
            if (avatarSprite != null)
                avatarBtn.style.backgroundImage = new StyleBackground(avatarSprite);

            avatarBtn.tooltip = displayName;
            avatarBtn.AddToClassList("AvatarName");
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

    public void SeviyeBilgisiGoster()
    {
        OnbilgiPopUp.style.display = DisplayStyle.Flex;
        LevelID.text = "Level : " + GameManager.Instance.kayitliOyuncuVerisi.Level.ToString();
        MeyveSayisi.text = "MeyveSirasi : " + GameManager.Instance.kayitliOyuncuVerisi.MeyveSirasi.ToString();
        RenkSayisi.text = "RenkSayisi : " + GameManager.Instance.kayitliOyuncuVerisi.RenkSirasi.ToString();
        KalipSayisi.text = "Kalip : " + GameManager.Instance.kayitliOyuncuVerisi.KalipSirasi.ToString();
    }

    public void YeniBasariGoster(string asilanLimitAdi)
    {
        if (asilanLimitAdi != "")
        {
            GameManager.Instance.OyunDurumu = GameManager.OyunDurumlari.OyunDurdu;
            SonbilgiPopUp.style.display = DisplayStyle.Flex;
        }

        var kayitliOyuncuVerisi = SoloLevelManager.Instance.GetLevelVerisi(PlayerPrefs.GetInt("OynananLevelID"));

        if (asilanLimitAdi == "Meyve")
        {
            SonBilgi_MeyveSayisi.text = "MeyveSirasi : " + kayitliOyuncuVerisi.MeyveSirasi.ToString();
            SonBilgi_MeyveSayisi.style.display = DisplayStyle.Flex;
        }
        else if (asilanLimitAdi == "Renk")
        {
            SonBilgi_RenkSayisi.text = "RenkSirasi : " + kayitliOyuncuVerisi.RenkSirasi.ToString();
            SonBilgi_RenkSayisi.style.display = DisplayStyle.Flex;
        }
        else if (asilanLimitAdi == "Kalip")
        {
            SonBilgi_KalipSayisi.text = "RenkSirasi : " + kayitliOyuncuVerisi.KalipSirasi.ToString();
            SonBilgi_KalipSayisi.style.display = DisplayStyle.Flex;
        }
        else if (asilanLimitAdi == "PuanLimiti")
        {
            SonBilgi_YeniLevel.text = "Yeni Level : " + kayitliOyuncuVerisi.Level.ToString();
            SonBilgi_YeniLevel.style.display = DisplayStyle.Flex;
        }
    }
}
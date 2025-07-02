using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class OyunSonu : NetworkBehaviour {
    private VisualElement rootElement;
    private VisualElement container;
    private VisualElement sonucListesi;
    private VisualElement footer;
    private Button yeniOyunuBaslat;

    private Button quit;

    public static OyunSonu Instance;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() {
        if (MainMenu.isSoloGame) {
            SoloSonucListesiniGoster();
        }
    }

    private void SoloSonucListesiniGoster() {
        // alanları doldur
        sonucListesi.Clear();
        var BonusSayisiBtn = new Button();
        BonusSayisiBtn.text = PuanlamaIStatistikleri.BonusMeyveSayisi.ToString();
        BonusSayisiBtn.AddToClassList("bonus_sayisi");
        BonusSayisiBtn.AddToClassList("kutu");

        var AltinBtn = new Button();
        AltinBtn.text = PuanlamaIStatistikleri.AltinSayisi.ToString();
        AltinBtn.AddToClassList("altin_sayisi");
        AltinBtn.AddToClassList("kutu");

        var ElmasBtn = new Button();
        ElmasBtn.text = PuanlamaIStatistikleri.ElmasSayisi.ToString();
        ElmasBtn.AddToClassList("elmas_sayisi");
        ElmasBtn.AddToClassList("kutu");

        var SkorBtn = new Button();
        var Skor = (PuanlamaIStatistikleri.BonusMeyveSayisi + PuanlamaIStatistikleri.AltinSayisi +
                    PuanlamaIStatistikleri.ElmasSayisi);
        SkorBtn.text = Skor.ToString();
        SkorBtn.AddToClassList("skor_sayisi");
        SkorBtn.AddToClassList("kutu");

        // solo oyuncunun skorlimiti taamase renkSeviyesini artır 
        if (OyunKurallari.Instance.SkorLimiti <= Skor) {
            LevelManager.RenkSayisiniArtir();
        }


        var playerNameBtn = new Button();
        playerNameBtn.text = "BAŞARDINIZ";
        playerNameBtn.AddToClassList("name");

        var ListRowVisuElm = new VisualElement();
        ListRowVisuElm.Add(playerNameBtn);
        ListRowVisuElm.Add(BonusSayisiBtn);
        ListRowVisuElm.Add(AltinBtn);
        ListRowVisuElm.Add(ElmasBtn);
        ListRowVisuElm.Add(SkorBtn);
        ListRowVisuElm.AddToClassList("ListRow");
        sonucListesi.Add(ListRowVisuElm);
    }

    public void MultySonucListesiniGoster() {
        try {
            NetworkList<MultiPlayerVeriYoneticisi.PlayerData> oyuncuListesi =
                MultiPlayerVeriYoneticisi.Instance.OyuncuListesi;
            sonucListesi.Clear();
            // Burada yeni bir kopya liste oluştur 
            var localList = new List<MultiPlayerVeriYoneticisi.PlayerData>();
            for (int i = 0; i < oyuncuListesi.Count; i++) {
                var oyuncu = oyuncuListesi[i];
                oyuncu.Skor = oyuncu.BonusMeyveSayisi + oyuncu.AltinSayisi + oyuncu.ElmasSayisi;
                localList.Add(oyuncu);
            }

            // Bu kopyayı sıralıyoruz
            var siraliListe = localList.OrderByDescending(p => p.Skor).ToList();

            foreach (var oyuncu in siraliListe) {
                ulong clientID = oyuncu.ClientId;
                FixedString64Bytes clientName = oyuncu.ClientName;

                var BonusSayisiBtn = new Button();
                BonusSayisiBtn.text = oyuncu.BonusMeyveSayisi.ToString();
                BonusSayisiBtn.AddToClassList("bonus_sayisi");
                BonusSayisiBtn.AddToClassList("kutu");

                var AltinBtn = new Button();
                AltinBtn.text = oyuncu.AltinSayisi.ToString();
                AltinBtn.AddToClassList("altin_sayisi");
                AltinBtn.AddToClassList("kutu");

                var ElmasBtn = new Button();
                ElmasBtn.text = oyuncu.ElmasSayisi.ToString();
                ElmasBtn.AddToClassList("elmas_sayisi");
                ElmasBtn.AddToClassList("kutu");

                var SkorBtn = new Button();
                SkorBtn.text = oyuncu.Skor.ToString();
                SkorBtn.AddToClassList("skor_sayisi");
                SkorBtn.AddToClassList("kutu");

                var playerNameBtn = new Button();
                playerNameBtn.text = clientName.ToString();
                playerNameBtn.AddToClassList("name");

                var ListRowVisuElm = new VisualElement();
                ListRowVisuElm.Add(playerNameBtn);
                ListRowVisuElm.Add(BonusSayisiBtn);
                ListRowVisuElm.Add(AltinBtn);
                ListRowVisuElm.Add(ElmasBtn);
                ListRowVisuElm.Add(SkorBtn);
                ListRowVisuElm.AddToClassList("ListRow");
                sonucListesi.Add(ListRowVisuElm);
            }
        }
        catch (Exception e) {
            Debug.Log($"SonucListesiniGoster içinde HATA : {e.Message}");
        }
    }

    private void OnEnable() {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        container = rootElement.Q<VisualElement>("Container");
        sonucListesi = rootElement.Q<VisualElement>("SonucListesi");
        footer = rootElement.Q<VisualElement>("Footer");
        yeniOyunuBaslat = container.Q<Button>("YeniOyunuBaslat");
        quit = rootElement.Q<Button>("Quit");
        //Hazirim = rootElement.Q<Button>("Hazirim"); 
        yeniOyunuBaslat.clicked += OnYeniOyunuBaslatClick;
        quit.clicked += OnQuitClick;
        //Hazirim.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.None : DisplayStyle.Flex;
        yeniOyunuBaslat.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnQuitClick() {
        _ = LobbyManager.Instance.CikisIsteginiGonder();
    }

    private void OnYeniOyunuBaslatClick() {
        // Multy ise
        if (!MainMenu.isSoloGame)
            MultiPlayerVeriYoneticisi.Instance.OyunuYenidenBaslatServerRpc();
        // Solo ise
        if (MainMenu.isSoloGame) {
            GameManager.Instance.Seed = MainMenu.GetRandomSeed();
            SceneManager.LoadScene("OyunSahnesi", LoadSceneMode.Single);
        }
    }
    
}
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class OyunSonu : NetworkBehaviour{
    private VisualElement rootElement;
    private VisualElement sonucListesi;
    private VisualElement footer;
    public Button YeniOyunuBaslat;
    public Button Quit;
    public Button Hazirim;
    public static OyunSonu Instance; 

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

   

    public void SonucListesiniGoster(){
        try{
            NetworkList<NetworkDataManager.PlayerData> oyuncuListesi = NetworkDataManager.Instance.oyuncuListesi;
      
            sonucListesi.Clear();
            // Burada yeni bir kopya liste oluştur 
            var localList = new List<NetworkDataManager.PlayerData>();
            foreach (var oyuncu in oyuncuListesi){
                localList.Add(oyuncu);
            }

            // Bu kopyayı sıralıyoruz
            var siraliListe = localList.OrderByDescending(p => p.Skor).ToList();
            foreach (var oyuncu in siraliListe){
                ulong clientID = oyuncu.ClientId;
                FixedString64Bytes clientName = oyuncu.ClientName;
                //int puan = oyuncu.Skor;
                //int HamleSayisi = oyuncu.HamleSayisi;
                var puanBtn = new Button(); 
                puanBtn.text =  oyuncu.Skor.ToString(); 
                puanBtn.AddToClassList("puan");
                sonucListesi.Add(puanBtn);
                var playerNameBtn = new Button(); 
                playerNameBtn.text =  clientName.ToString(); 
                playerNameBtn.AddToClassList("name");
                var ListRowVisuElm = new VisualElement();
                ListRowVisuElm.Add(puanBtn);
                ListRowVisuElm.Add(playerNameBtn);
                ListRowVisuElm.AddToClassList("ListRow");
                sonucListesi.Add(ListRowVisuElm);
                
            }
        }
        catch(Exception e){
            Debug.Log($"SonucListesiniGoster içinde HATA : {e.Message}");
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        sonucListesi = rootElement.Q<VisualElement>("SonucListesi"); 
        footer = rootElement.Q<VisualElement>("Footer");
        YeniOyunuBaslat = footer.Q<Button>("YeniOyunuBaslat");
        Quit = rootElement.Q<Button>("Quit");
        Hazirim = rootElement.Q<Button>("Hazirim"); 
        YeniOyunuBaslat.clicked += OnYeniOyunuBaslatClick;
        Quit.clicked += OnQuitClick;
        Hazirim.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.None : DisplayStyle.Flex;
        YeniOyunuBaslat.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnQuitClick(){
        _ = LobbyManager.Instance.CikmakIisteginiGonder();
    }

    private void OnYeniOyunuBaslatClick(){ 
        NetworkDataManager.Instance.OyunuYenidenBaslatServerRpc(); 
    }
 
}
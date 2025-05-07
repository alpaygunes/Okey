using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        NetworkList<NetworkDataManager.PlayerData> oyuncuListesi = NetworkDataManager.Instance.oyuncuListesi;;
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
            int puan = oyuncu.Skor;
            int HamleSayisi = oyuncu.HamleSayisi;
            var listeOgesi = new Button();
            listeOgesi.text = clientID.ToString();
            listeOgesi.text += " ClientName :" + clientName;
            listeOgesi.text += " Puan :" + puan;
            listeOgesi.text += " HamleSayisi :" + HamleSayisi;
            sonucListesi.Add(listeOgesi); 
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
        
        YeniOyunuBaslat.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnYeniOyunuBaslatClick(){
        NetworkDataManager.Instance.OyunuYenidenBaslatServerRpc(); 
    }
 
}
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class OyunSonu : MonoBehaviour{
    private VisualElement rootElement;
    private VisualElement SonucListesi;
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
        SonucListesi.Clear();
        // Burada yeni bir kopya liste oluştur 
        var localList = new List<NetworkDataManager.PlayerData>();
        foreach (var oyuncu in oyuncuListesi){
            localList.Add(oyuncu);
        } 
        // Bu kopyayı sıralıyoruz
        var siraliListe = localList.OrderByDescending(p => p.Skor).ToList();
        Debug.Log($"SiraliListe {siraliListe.Count}");
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
            SonucListesi.Add(listeOgesi); 
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SonucListesi = rootElement.Q<VisualElement>("SonucListesi");
        YeniOyunuBaslat = rootElement.Q<Button>("YeniOyunuBaslat");
        Quit = rootElement.Q<Button>("Quit");
        Hazirim = rootElement.Q<Button>("Hazirim");
 
        YeniOyunuBaslat.clicked += OnYeniOyunuBaslatClick;
    }

    private void OnYeniOyunuBaslatClick(){
        
    }
}
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class OyunSonu : MonoBehaviour{
    private VisualElement rootElement;
    private VisualElement SonucListesi;
    public static OyunSonu Instance;


    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SonucListesiniGoster(NetworkList<NetworkDataManager.PlayerData> _oyuncuListesi){ 
        SonucListesi.Clear();
        // Burada yeni bir kopya liste oluştur 
        var localList = new List<NetworkDataManager.PlayerData>();
        foreach (var oyuncu in _oyuncuListesi){
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
            SonucListesi.Add(listeOgesi); 
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SonucListesi = rootElement.Q<VisualElement>("SonucListesi");
    }
}
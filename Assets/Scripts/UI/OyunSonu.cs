using System;
using System.Collections.Generic;
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

    public void SonucListesiniGoster(NetworkList<NetwokDataManager.PlayerData> _oyuncuListesi){
        Debug.Log($"  LİSTE HAZIRLANIYOR  ");
        //Debug.Log($"Oyuncu {oyuncu.ClientId} - Skor: {oyuncu.Skor}, Hamle: {oyuncu.HamleSayisi}");
        SonucListesi.Clear();
        foreach (var oyuncu in _oyuncuListesi){
            ulong clientID = oyuncu.ClientId;
            ulong myDisplayName = oyuncu.ClientId;
            int puan = oyuncu.Skor;
            int HamleSayisi = oyuncu.HamleSayisi;
            var listeOgesi = new Button();
            listeOgesi.text = clientID.ToString();
            listeOgesi.text += " " + myDisplayName;
            listeOgesi.text += " " + puan;
            listeOgesi.text += " " + HamleSayisi;
            SonucListesi.Add(listeOgesi);
            Debug.Log($" {clientID.ToString()}  --- {clientID.ToString()}  ");
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SonucListesi = rootElement.Q<VisualElement>("SonucListesi");
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OyunSonu : MonoBehaviour{
    private VisualElement rootElement;
    private VisualElement SonucListesi;
    public static OyunSonu Instance;
    public event Action<Dictionary<ulong,Dictionary<string,string>>> OnSkorListesiDegisti;
    private Dictionary<ulong,Dictionary<string,string>> _skorListesi = new Dictionary<ulong,Dictionary<string,string>>();
    
    public Dictionary<ulong,Dictionary<string,string>> SkorListesiDic{
        get => _skorListesi;
        set{
            if (_skorListesi != value){
                _skorListesi = value;
                if (_skorListesi != null && OnSkorListesiDegisti != null) {
                    OnSkorListesiDegisti?.Invoke(_skorListesi);  
                } 
            }
        }
    }

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start(){
        
        OnSkorListesiDegisti += (_skorListesi) => {
            Debug.Log($"  LİSTE HAZIRLANIYOR  ");
            SonucListesi.Clear();
            foreach (var item in _skorListesi){
                ulong clientID = item.Key;
                string myDisplayName = item.Value["myDisplayName"];
                string puan = item.Value["puan"];
                var listeOgesi = new Button();
                listeOgesi.text = clientID.ToString();
                listeOgesi.text += " " + myDisplayName;
                listeOgesi.text += " " + puan;
                SonucListesi.Add(listeOgesi);
                Debug.Log($" {clientID.ToString()}  --- {clientID.ToString()}  ");
            }
        };
    }


    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        SonucListesi = rootElement.Q<VisualElement>("SonucListesi");
    }
}
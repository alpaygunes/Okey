using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class KareManeger : MonoBehaviour{
    public List<GameObject> KarelerList = new List<GameObject>();
    private RangeInt _renkAraligi = new RangeInt(1,10);
    public static KareManeger Instance{ get; private set; }
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void KareleriHazirla(){
        for (int i = 0; i < PlatformManager.Instance.kareCount; i++){ 
            GameObject kare = Resources.Load<GameObject>("Prefabs/Kare");
            var Kare = Instantiate(kare, new Vector2(0, 0), Quaternion.identity);
            var rakam = Random.Range(_renkAraligi.start, _renkAraligi.end);  
            Kare.transform.GetComponent<Renderer>().material.color =  
                RenklerYonetimi.RenkSozlugu[Random.Range(_renkAraligi.start, _renkAraligi.end + 1)];
            Kare.GetComponentInChildren<TextMeshPro>().text = rakam.ToString();
            Kare.GetComponentInChildren<Kare>().Rakam = rakam;
            Kare.SetActive(false);
            KarelerList.Add(Kare);
        } 
    }
}
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>();
    private RangeInt _renkAraligi = new RangeInt(1,3);
    private RangeInt _rakamAraligi = new RangeInt(1,10);
    public static TasManeger Instance{ get; private set; }
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void TaslariHazirla(){
        for (int i = 0; i < PlatformManager.Instance.tasCount; i++){ 
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector2(0, 0), Quaternion.identity);
            var rakam = Random.Range(_rakamAraligi.start, _rakamAraligi.end);  
            Tas.transform.GetComponent<Renderer>().material.color =  
                RenklerYonetimi.RenkSozlugu[Random.Range(_renkAraligi.start, _renkAraligi.end + 1)];
            Tas.GetComponentInChildren<TextMeshPro>().text = rakam.ToString();
            Tas.GetComponentInChildren<Tas>().Rakam = rakam;
            Tas.SetActive(false);
            TasList.Add(Tas);
        } 
    }
}
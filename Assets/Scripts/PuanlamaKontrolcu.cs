using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance{ get; private set; }
    //private Vector2 size;
    public int PerlerdenKazanilanPuan = 1;
    public TextMeshProUGUI ToplamPuanTMP;
    [FormerlySerializedAs("perlerdenKazanilanPuanTMP")] public TextMeshProUGUI PerlerdenKazanilanPuanTMP;
    public int merkezeKayGecikmesi = 0;
    public int ToplamPuan = 0;
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        } 
        Instance = this;
    }

    private void Start(){
        ToplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        PerlerdenKazanilanPuanTMP = GameObject.Find("PerlerdenKazanilanPuan").GetComponent<TextMeshProUGUI>();
    }

    public void PuanlamaYap(){
        merkezeKayGecikmesi = 0;
        var satir = 0;
        PerlerdenKazanilanPuan = 1;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){ 
                merkezeKayGecikmesi++;
                item.Value.GetComponent<Tas>().merkezeKay(item.Key); 
                sayac++;
            }
        }

        //AyniRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){ 
                merkezeKayGecikmesi++;
                item.Value.GetComponent<Tas>().merkezeKay(item.Key);
                sayac++;
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){ 
                merkezeKayGecikmesi++;
                item.Value.GetComponent<Tas>().merkezeKay(item.Key);
                sayac++;
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            foreach (var item in grupList){
                merkezeKayGecikmesi++;
                item.Value.GetComponent<Tas>().merkezeKay(item.Key);
                sayac++;
            }
        }
        StartCoroutine(WaitAndExecute());
    }
    
    IEnumerator WaitAndExecute(){
        yield return new WaitForSeconds(merkezeKayGecikmesi);
        IstakaKontrolcu.Instance.GruplariTemizle();
        ToplamPuan += PerlerdenKazanilanPuan;
        ToplamPuanTMP.text = ToplamPuan.ToString();
    }
    
}
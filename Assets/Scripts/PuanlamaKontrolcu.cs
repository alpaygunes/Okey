using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance{ get; private set; }

    //private Vector2 size;
    public int PerlerdenKazanilanPuan = 1;
    public TextMeshProUGUI toplamPuanTMP;
    public TextMeshProUGUI PerlerdenKazanilanPuanTMP;
    private float _merkezeKayGecikmesi;
    [FormerlySerializedAs("_toplamPuan")] public int toplamPuan;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start(){
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        PerlerdenKazanilanPuanTMP = GameObject.Find("PerlerdenKazanilanPuan").GetComponent<TextMeshProUGUI>();
    }

    public void PerdekiTaslariToparla(){
        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>();
        PerlerdenKazanilanPuan = 0;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari){
            //GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }
            }
        }

        //AyniRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari){
            foreach (var item in grupList){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari){
            foreach (var item in grupList){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari){
            foreach (var item in grupList){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }
            }
        }

        // perdeki itemleri sıralayalım
        _merkezeKayGecikmesi = 0;
        SortedDictionary<int, GameObject> siralanmisTumPerTaslari = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
        foreach (var tas in siralanmisTumPerTaslari){
            _merkezeKayGecikmesi += 0.2f;
            TasManeger.Instance.TasIstances[tas.Value].PuaniVerMerkezeKay(_merkezeKayGecikmesi);
        }
        StartCoroutine(SkorTMPleriGuncelle());
    }

    IEnumerator SkorTMPleriGuncelle(){
        yield return new WaitForSeconds(_merkezeKayGecikmesi);
        IstakaKontrolcu.Instance.GruplariTemizle();
        toplamPuan += PerlerdenKazanilanPuan;
        toplamPuanTMP.text = toplamPuan.ToString();
    }
}
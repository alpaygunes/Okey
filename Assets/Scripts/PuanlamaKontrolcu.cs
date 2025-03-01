using System.Collections;
using System.Collections.Generic; 
using TMPro;
using UnityEngine;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance{ get; private set; }
    //private Vector2 size;
    public int PerlerdenKazanilanPuan = 1;
    private TextMeshProUGUI _toplamPuanTMP;
    public TextMeshProUGUI PerlerdenKazanilanPuanTMP;
    private float _merkezeKayGecikmesi ;
    private int _toplamPuan ;
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        } 
        Instance = this;
    }

    private void Start(){
        _toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        PerlerdenKazanilanPuanTMP = GameObject.Find("PerlerdenKazanilanPuan").GetComponent<TextMeshProUGUI>();
    }

    public void PuanlamaYap(){
        Dictionary<int,GameObject> perdekiTumTaslarDic  = new Dictionary<int,GameObject>();
        //var satir = 0;
        PerlerdenKazanilanPuan = 1;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari){
            //GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){  
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key,item.Value);
                } 
            }
        }

        //AyniRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari){
            //GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){ 
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key,item.Value);
                } 
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari){
            //GameObject tas = grupList.First().Value; 
            foreach (var item in grupList){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key,item.Value);
                }  
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari){
            //GameObject tas = grupList.First().Value;
            foreach (var item in grupList){ 
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key,item.Value);
                } 
            }
        }
        
        // perdeki itemleri sıralayalım
        _merkezeKayGecikmesi = 0;
        SortedDictionary<int, GameObject> siralanmisTumPerTaslari = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
        foreach (var tas in siralanmisTumPerTaslari){
            _merkezeKayGecikmesi += 0.2f;
            tas.Value.GetComponent<Tas>().MerkezeKay(_merkezeKayGecikmesi); 
        }
        StartCoroutine(WaitAndExecute());
    }
    
    IEnumerator WaitAndExecute(){
        yield return new WaitForSeconds(_merkezeKayGecikmesi);
        IstakaKontrolcu.Instance.GruplariTemizle();
        _toplamPuan += PerlerdenKazanilanPuan;
        _toplamPuanTMP.text = _toplamPuan.ToString();
    }
    
}
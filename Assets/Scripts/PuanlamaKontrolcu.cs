using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance { get; private set; }

    //private Vector2 size;
    public int PerlerdenKazanilanPuan = 1;
    public TextMeshProUGUI toplamPuanTMP;
    //public TextMeshProUGUI PerlerdenKazanilanPuanTMP;
    private float _merkezeKayGecikmesi;
    public int toplamPuan;


    List<Tas> BonusAyniRenkAyniRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkAyniRakam = new List<Tas>();
    List<Tas> BonusAyniRenkArdisikRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkArdisikRakam = new List<Tas>();

    void Awake(){
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start(){
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        //PerlerdenKazanilanPuanTMP = GameObject.Find("PerlerdenKazanilanPuan").GetComponent<TextMeshProUGUI>();
    }

    public void PerdekiTaslariToparla(){
        BonusAyniRenkArdisikRakam.Clear();
        BonusAyniRenkAyniRakam.Clear();
        BonusFarkliRenkAyniRakam.Clear();
        BonusFarkliRenkArdisikRakam.Clear();

        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>();
        PerlerdenKazanilanPuan = 0;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkArdisikRakam.Add(Instantiate(TasManeger.Instance.TasIstances[item.Value]));
                }
            }
        }

        //AyniRakamAyniRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkAyniRakam.Add(Instantiate(TasManeger.Instance.TasIstances[item.Value]));
                }
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusFarkliRenkAyniRakam.Add(Instantiate(TasManeger.Instance.TasIstances[item.Value]));
                }
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusFarkliRenkArdisikRakam.Add(Instantiate(TasManeger.Instance.TasIstances[item.Value]));
                }
            }
        }

        // perdeki itemleri sıralayalım
        _merkezeKayGecikmesi = 0;
        SortedDictionary<int, GameObject> siralanmisTumPerTaslari =
            new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
        foreach (var tas in siralanmisTumPerTaslari) {
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

    public void BonuslariVer(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
 
        if (BonusAyniRenkArdisikRakam.Count > 0) {
            foreach (var tasInstance in BonusAyniRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        itemIstance.ZenminRenginiDegistir();
                    }
                }
            }
        }

 
        if (BonusAyniRenkAyniRakam.Count > 0) {
            foreach (var tasInstance in BonusAyniRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        itemIstance.ZenminRenginiDegistir();
                    }
                }
            }
        }

 
        if (BonusFarkliRenkAyniRakam.Count > 0) {
            foreach (var tasInstance in BonusFarkliRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        itemIstance.ZenminRenginiDegistir();
                    }
                }
            }
        }

        
        if (BonusFarkliRenkArdisikRakam.Count > 0) {
            foreach (var tasInstance in BonusFarkliRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        itemIstance.ZenminRenginiDegistir();
                    }
                }
            }
        }
    }
}
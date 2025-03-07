using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine; 

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance { get; private set; } 
    private TextMeshProUGUI textMesh;  
    public TextMeshProUGUI toplamPuanTMP;
    //private Transform textMeshTransFrm;
    public int PerlerdenKazanilanPuan = 1; 
    private float _merkezeKayGecikmesi;
    public int toplamPuan;
    private Camera uiCamera;

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
        uiCamera = Camera.main; 
    }

    private void Start(){
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        textMesh = GameObject.Find("FlatingText0").GetComponent<TextMeshProUGUI>();
        //textMeshTransFrm = GameObject.Find("FlatingText").transform;
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
        //toplamPuanTMP.text = toplamPuan.ToString(); 
        //print($"Puanlama...  toplamPuan : {toplamPuan}");
        
        // floatingText 
        if (toplamPuan>0) {
            Vector3 _targetPosition = uiCamera.WorldToScreenPoint(new Vector3(0,2,0));
            textMesh.text = "+" + PerlerdenKazanilanPuan.ToString();
            textMesh.transform.position = uiCamera.WorldToScreenPoint(Istaka.Instance.transform.position);
            textMesh.transform.DOMoveY(_targetPosition.y, 2f).SetEase(Ease.OutQuad);
            var color = textMesh.color;
            color.a = 1f;
            textMesh.color = color;
            textMesh.DOFade(0, 3f);
        }
    }


    public void BonuslariVer(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        if (BonusAyniRenkArdisikRakam.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (BonusAyniRenkArdisikRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                }
            }
        }


        if (BonusAyniRenkAyniRakam.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        itemIstance.ZenminRenginiDegistir();
                    }
                }
            }
        }


        if (BonusFarkliRenkAyniRakam.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (BonusFarkliRenkAyniRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                }
            }
        }


        if (BonusFarkliRenkArdisikRakam.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasIstances[item];
                    if (BonusFarkliRenkArdisikRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            itemIstance.ZenminRenginiDegistir();
                        }
                    }
                }
            }
        }
    }
}
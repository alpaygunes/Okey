using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Puanlama : MonoBehaviour{
    public static Puanlama Instance { get; private set; }
    private TextMeshProUGUI textMesh0; 
    private TextMeshProUGUI textMesh1;
    public TextMeshProUGUI toplamPuanTMP; 
    private TextMeshProUGUI hamleSayisiTMP; 
    public int PerlerdenKazanilanPuan = 1;
    private float _merkezeKayGecikmesi;
    public int toplamPuan;
    private Camera uiCamera; 
    
    private AudioSource _audioSource_puan_sayac;

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
        
        _audioSource_puan_sayac = gameObject.AddComponent<AudioSource>();
        _audioSource_puan_sayac.playOnAwake = false;
        _audioSource_puan_sayac.clip = Resources.Load<AudioClip>("Sounds/puan_sayac");
    }

    private void Start(){
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        hamleSayisiTMP = GameObject.Find("HamleSayisi").GetComponent<TextMeshProUGUI>();
        textMesh0 = GameObject.Find("FlatingText0").GetComponent<TextMeshProUGUI>();
        textMesh1 = GameObject.Find("FlatingText1").GetComponent<TextMeshProUGUI>();
    }

    public void IstakadakiPerdekiTaslariToparla(){
        
        BonusAyniRenkArdisikRakam.Clear();
        BonusAyniRenkAyniRakam.Clear();
        BonusFarkliRenkAyniRakam.Clear();
        BonusFarkliRenkArdisikRakam.Clear();

        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>();
        PerlerdenKazanilanPuan = 0;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.SiraliRakamAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkArdisikRakam.Add(Instantiate(TasManeger.Instance.TasInstances[item.Value]));
                }
            }
        }

        //AyniRakamAyniRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.AyniRakamAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkAyniRakam.Add(Instantiate(TasManeger.Instance.TasInstances[item.Value]));
                }
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusFarkliRenkAyniRakam.Add(Instantiate(TasManeger.Instance.TasInstances[item.Value]));
                }
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusFarkliRenkArdisikRakam.Add(Instantiate(TasManeger.Instance.TasInstances[item.Value]));
                }
            }
        }

        // perdeki itemleri sıralayalım
        _merkezeKayGecikmesi = 0;
        SortedDictionary<int, GameObject> siralanmisTumPerTaslari =
            new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
        foreach (var tas in siralanmisTumPerTaslari) {
            _merkezeKayGecikmesi += 0.2f;
            TasManeger.Instance.TasInstances[tas.Value].PuaniVerMerkezeKay(_merkezeKayGecikmesi); 
        }

        StartCoroutine(SkorTMPleriGuncelle());
    }
    
    public void BonuslariVer(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        if (BonusAyniRenkArdisikRakam.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusAyniRenkArdisikRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                }
            }
        }


        if (BonusAyniRenkAyniRakam.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                        TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                        StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                    }
                }
            }
        }


        if (BonusFarkliRenkAyniRakam.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkAyniRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusFarkliRenkAyniRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                }
            }
        }


        if (BonusFarkliRenkArdisikRakam.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusFarkliRenkArdisikRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                            StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
                        }
                    }
                }
            }
        }
    }

    IEnumerator SkorTMPleriGuncelle(){
        Istaka.Instance.GruplariTemizle();
        yield return new WaitForSeconds(_merkezeKayGecikmesi); 
        toplamPuan += PerlerdenKazanilanPuan;

        // floatingText 
        if (toplamPuan > 0) {
            Vector3 _targetPosition = uiCamera.WorldToScreenPoint(new Vector3(0, 2, 0));
            textMesh0.text = "+" + PerlerdenKazanilanPuan.ToString();
            textMesh0.transform.position = uiCamera.WorldToScreenPoint(Istaka.Instance.transform.position);
            textMesh0.transform.DOMoveY(_targetPosition.y, 2f).SetEase(Ease.OutQuad);
            var color = textMesh0.color;
            color.a = 1f;
            textMesh0.color = color;
            textMesh0.DOFade(0, 3f);
        }
    }
    
    public void _kartdakiPerleriIsle(){
        PerlerdenKazanilanPuan = 0;
        foreach (var grup in Card.Instance._siraliAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in Card.Instance._siraliFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in Card.Instance._ayniRakamAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in Card.Instance._ayniRakamFarkliRenkliGruplar) {
            foreach (var item in grup) { 
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasInstances[item].RakamiPuanaEkle(1));
            }
        }
        StartCoroutine(SkorTMPleriGuncelle1());
    }
    
    IEnumerator SkorTMPleriGuncelle1(){
        yield return new WaitForSeconds(1);

        // puan artış animasyonu
        var startScore = Instance.toplamPuan;
        var currentScore = Instance.toplamPuan + Instance.PerlerdenKazanilanPuan;
        DOTween.To(() => startScore, x => Instance.toplamPuanTMP.text = x.ToString(), currentScore,
            1f).SetEase(Ease.OutQuad);
        Instance.toplamPuan += Instance.PerlerdenKazanilanPuan;
        Instance.toplamPuanTMP.transform.DOPunchPosition(new Vector3(Screen.width*.01f, Screen.height*.02f, 0f), 1f, 30, 0.5f);
        hamleSayisiTMP.text = GameManager.Instance.HamleSayisi.ToString();
        
        // sayaç sesi
        _audioSource_puan_sayac.Play();
        
        // floatingText
        Vector3 targetPosition = uiCamera.WorldToScreenPoint(new Vector3(0, 2, 0));
        textMesh1.text =   Instance.PerlerdenKazanilanPuan.ToString();
        if (Instance.PerlerdenKazanilanPuan>0) {
            textMesh1.text = "+" + textMesh1.text;
        } 
        textMesh1.transform.position = uiCamera.WorldToScreenPoint(new Vector3(0, 0, 0));
        textMesh1.transform.DOMoveY(targetPosition.y, 2f).SetEase(Ease.OutQuad);
        var color = textMesh1.color;
        color.a = 1f;
        textMesh1.color = color;
        textMesh1.DOFade(0, 3f);
    }

    public void PuanlamaYap(){
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){
                IstakadakiPerdekiTaslariToparla();
                BonuslariVer();
        }
        else {
            Istaka.Instance.PersizFullIstakayiBosalt(); 
        }
        Card.Instance.KarttakiPerleriBul();
        _kartdakiPerleriIsle();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Puanlama : MonoBehaviour{
    public static Puanlama Instance { get; private set; }
    private TextMeshProUGUI textMesh0; 
    private TextMeshProUGUI textMesh1;
    public TextMeshProUGUI toplamPuanTMP; 
    private TextMeshProUGUI hamleSayisiTMP;
    public TextMeshProUGUI KalanTasOraniTMP;
    public int PerlerdenKazanilanPuan = 1; 
    private Camera uiCamera;  
    private AudioSource _audioSource_puan_sayac; 
    List<Tas> BonusAyniRenkAyniRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkAyniRakam = new List<Tas>();
    List<Tas> BonusAyniRenkArdisikRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkArdisikRakam = new List<Tas>();
    
    public event Action<int> OnToplamPuanChanged;
    private int _toplamPuan;
    public int ToplamPuan
    {
        get => _toplamPuan;
        set
        {
            if (_toplamPuan != value)
            {
                _toplamPuan = value;
                OnToplamPuanChanged?.Invoke(_toplamPuan); // Event tetikleniyor
            }
        }
    }

    private SortedDictionary<int, GameObject> siralanmisTumPerTaslari;

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
        OnToplamPuanChanged += (_toplamPuan) => {
            NetwokDataManager.Instance?.RequestToplamPuanGuncelleServerRpc(_toplamPuan);
        };
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        hamleSayisiTMP = GameObject.Find("HamleSayisi").GetComponent<TextMeshProUGUI>();
        KalanTasOraniTMP = GameObject.Find("KalanTasOrani").GetComponent<TextMeshProUGUI>();
        textMesh0 = GameObject.Find("FlatingText0").GetComponent<TextMeshProUGUI>();
        textMesh1 = GameObject.Find("FlatingText1").GetComponent<TextMeshProUGUI>();
        Puanlama.Instance.KalanTasOraniTMP.text = GameManager.Instance.TasCount.ToString();
    }

    public void IstakadakiPerdekiTaslariToparla(){
        
        BonusAyniRenkArdisikRakam.Clear();
        BonusAyniRenkAyniRakam.Clear();
        BonusFarkliRenkAyniRakam.Clear();
        BonusFarkliRenkArdisikRakam.Clear();
        
        siralanmisTumPerTaslari = new SortedDictionary<int, GameObject>();

        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>();
        PerlerdenKazanilanPuan = 0;
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.SiraliRakamAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkArdisikRakam.Add(TasManeger.Instance.TasInstances[item.Value]);
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
                    BonusAyniRenkAyniRakam.Add(TasManeger.Instance.TasInstances[item.Value]);
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
                    BonusFarkliRenkAyniRakam.Add(TasManeger.Instance.TasInstances[item.Value]);
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
                    BonusFarkliRenkArdisikRakam.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }
        
        siralanmisTumPerTaslari = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
        
    }
    
    public void BonuslariVer(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        if (BonusAyniRenkArdisikRakam.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkArdisikRakam) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusAyniRenkArdisikRakam.Count > 3) {
                        if (tasInstance.rakam == itemIstance.rakam || tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
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
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
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
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
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
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.rakam == itemIstance.rakam) {
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                }
            }
        }

        float gecikme = 0f;
        foreach (var tas in siralanmisTumPerTaslari){ 
            StartCoroutine(TasManeger.Instance.TasInstances[tas.Value].TaslarinRakaminiPuanaEkle(gecikme));
            gecikme++;
        } 
        Istaka.Instance.GruplariTemizle();
        
        
        
    }

    public void _kartdakiPerleriIsle(){
        PerlerdenKazanilanPuan = 0;
        foreach (var grup in Card.Instance._siraliAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._siraliFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._ayniRakamAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._ayniRakamFarkliRenkliGruplar) {
            foreach (var item in grup) { 
                TasManeger.Instance.TasInstances[item].ZeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        } 
        
    }
    
    public void Puanla(){ 
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){
                IstakadakiPerdekiTaslariToparla();
                BonuslariVer();
                GameManager.Instance.HamleSayisi++; 
                PerIcinTasTavsiye.Instance.Sallanma();
        } 
        _ = Card.Instance.KarttakiPerleriBul();
        _kartdakiPerleriIsle();
    }

    public void SkorBoardiGuncelle(){
        hamleSayisiTMP.text = GameManager.Instance.HamleSayisi.ToString();
        Instance.ToplamPuan += Instance.PerlerdenKazanilanPuan; 
        Instance.toplamPuanTMP.text = Instance.ToplamPuan .ToString();
        Instance.PerlerdenKazanilanPuan = 0; 
    }

    public void ButtonlaPuanlamaYap(){ 
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){ 
            Puanla(); 
        }
        else{
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
    }
}
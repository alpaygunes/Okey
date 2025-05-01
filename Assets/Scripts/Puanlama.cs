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
    //public int PerlerdenKazanilanPuan = 1; 
    private Camera uiCamera;  
    private AudioSource _audioSource_puan_sayac; 
    List<Tas> BonusAyniRenkAyniRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkAyniRakam = new List<Tas>();
    List<Tas> BonusAyniRenkArdisikRakam = new List<Tas>();
    List<Tas> BonusFarkliRenkArdisikRakam = new List<Tas>();
    public int HamleSayisi;
    public event Action<int> OnNetworkDataIicinPuanChanged;
    private int _toplamPuan;
    public int ToplamPuan
    {
        get => _toplamPuan;
        set
        {
            if (_toplamPuan != value)
            {
                _toplamPuan = value; 
                OnNetworkDataIicinPuanChanged?.Invoke(_toplamPuan); // Event tetikleniyor
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
        OnNetworkDataIicinPuanChanged += (_netwokrDataToplamPuan) => { 
            HamleSayisi++;
            var skor = ToplamPuan;
            var _hameleSayisi = HamleSayisi;
            var clientName = LobbyManager.Instance.myDisplayName; 
            Instance.SkorBoardiGuncelle();
            OyunKurallari.Instance.DurumuKontrolEt();
            NetwokDataManager.Instance?.SkorVeHamleGuncelleServerRpc(skor,_hameleSayisi,clientName);
        };
        // boş bir değişim yaratalım. skorlsitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
        NetwokDataManager.Instance?.SkorVeHamleGuncelleServerRpc(0,0,LobbyManager.Instance.myDisplayName);
 
        toplamPuanTMP = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
        hamleSayisiTMP = GameObject.Find("HamleSayisi").GetComponent<TextMeshProUGUI>();
        KalanTasOraniTMP = GameObject.Find("KalanTasOrani").GetComponent<TextMeshProUGUI>();
        textMesh0 = GameObject.Find("FlatingText0").GetComponent<TextMeshProUGUI>();
        textMesh1 = GameObject.Find("FlatingText1").GetComponent<TextMeshProUGUI>();
        KalanTasOraniTMP.text = GameManager.Instance.TasCount.ToString();
    }

    public void IstakadakiPerdekiTaslariToparla(){
        
        BonusAyniRenkArdisikRakam.Clear();
        BonusAyniRenkAyniRakam.Clear();
        BonusFarkliRenkAyniRakam.Clear();
        BonusFarkliRenkArdisikRakam.Clear();
        
        siralanmisTumPerTaslari = new SortedDictionary<int, GameObject>();

        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>(); 
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
        
        // networke göndermek için animasyon vb gecikme leri beklemeden kaznılan net puan 
        int nwrkPuan = 0;
        foreach (var tas in siralanmisTumPerTaslari){
            //taşın kendisi 
            nwrkPuan += TasManeger.Instance.TasInstances[tas.Value].rakam;
            // perdeki taşın carddaki eşleşenleri
            foreach (var bonusTaslari in TasManeger.Instance.TasInstances[tas.Value].BonusOlarakEslesenTaslar){
                if (bonusTaslari.Value && !bonusTaslari.Value.NetworkDatayaEklendi){
                    bonusTaslari.Value.NetworkDatayaEklendi = true;
                    nwrkPuan += bonusTaslari.Value.rakam;
                }
            }
        } 
        ToplamPuan += nwrkPuan;
        
        // --- son

        float gecikme = 0f;
        foreach (var tas in siralanmisTumPerTaslari){ 
            StartCoroutine(TasManeger.Instance.TasInstances[tas.Value].TaslarinRakaminiPuanaEkle(gecikme));
            gecikme++;
        } 
        
        Istaka.Instance.GruplariTemizle(); 
    }

    public void _kartdakiPerleriIsle(){
         
        foreach (var grup in Card.Instance._siraliAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].zeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._siraliFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].zeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._ayniRakamAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasInstances[item].zeminSpriteRenderer.color = Color.green;
                TasManeger.Instance.TasInstances[item].RakamiPuanaEkle();
            }
        }

        foreach (var grup in Card.Instance._ayniRakamFarkliRenkliGruplar) {
            foreach (var item in grup) { 
                TasManeger.Instance.TasInstances[item].zeminSpriteRenderer.color = Color.green;
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
                PerIcinTasTavsiye.Instance.Sallanma();
        } 
        _ = Card.Instance.KarttakiPerleriBul();
        _kartdakiPerleriIsle();
    }

    public void SkorBoardiGuncelle(){
        hamleSayisiTMP.text = HamleSayisi.ToString(); 
        toplamPuanTMP.text = ToplamPuan .ToString();
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
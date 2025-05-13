using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Puanlama : MonoBehaviour{
    public static Puanlama Instance { get; private set; } 
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
     
    
    public SortedDictionary<int, GameObject> siralanmisTumPerTaslari;
    
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
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
            string clientName = "Bos_clientName";
            if (LobbyManager.Instance){
                clientName = LobbyManager.Instance.myDisplayName; 
            } 
            SkorBoardiGuncelle();
            if (OyunKurallari.Instance){ 
                LimitleriKontrolEt();
                NetworkDataManager.Instance?.SkorVeHamleGuncelleServerRpc(skor,_hameleSayisi,clientName); 
            } 
            
        };
        // boş bir değişim yaratalım. skorlsitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
        NetworkDataManager.Instance?.SkorVeHamleGuncelleServerRpc(1,0,LobbyManager.Instance.myDisplayName);
        OyunSahnesiUI.Instance.KalanTasSayisi.text = GameManager.Instance.TasCount.ToString();
    }

    public void LimitleriKontrolEt(){
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                GameManager.Instance.oyunDurumu = GameManager.OynanmaDurumu.bitti; 
                SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
            }
        } 
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            if (GameManager.Instance.oyununBitimineKalanZaman<=0){
                GameManager.Instance.oyunDurumu = GameManager.OynanmaDurumu.bitti; 
                SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                if (NetworkDataManager.Instance.oyuncuListesi != null){
                    IEnumerator enumerator;
                    try{
                        enumerator = NetworkDataManager.Instance.SkorListesiniYavasGuncelle();
                        NetworkDataManager.Instance.skorListesiniYavasGuncelleCoroutine  
                            = NetworkDataManager.Instance.StartCoroutine(enumerator);
                    }
                    catch (Exception e){
                        Debug.Log($"Host ayrışmış olabilir . LimitleriKontrolEt HATA : {e.Message}"); 
                    }
                }

            }
        }
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            GorevYoneticisi.Instance.GorevYapildimiKontrolEt();
        }
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
        //hamleSayisiTMP.text = HamleSayisi.ToString(); 
        //toplamPuanTMP.text = ToplamPuan .ToString();
        OyunSahnesiUI.Instance.Skor.text = ToplamPuan.ToString();
        OyunSahnesiUI.Instance.HamleSayisi.text = HamleSayisi.ToString();
    }

    public void ButtonlaPuanlamaYap(){ 
        if (PuanlamaCounter.Instance.CountdownCoroutine != null){
            PuanlamaCounter.Instance.StopCoroutine(PuanlamaCounter.Instance.CountdownCoroutine);
        }
        
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
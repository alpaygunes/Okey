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
using UnityEngine.UIElements;

public class Puanlama : MonoBehaviour{
    public static Puanlama Instance{ get; private set; }
    private Camera uiCamera;
    private AudioSource _audioSource_puan_sayac;
    List<Tas> PtasIleUyumluMeyveAyniRenkAyni = new List<Tas>();
    List<Tas> PtasIleUyumluMeyveAyniFarkliRenk = new List<Tas>();
    List<Tas> PtasIleUyumluMeyveFarkliRenkAyni = new List<Tas>();
    public int HamleSayisi;


    public SortedDictionary<int, GameObject> SiralanmisTumPerlerdekiTaslar;

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
        if (!MainMenu.isSoloGame){
            // boş bir değişim yaratalım. skorlsitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
            NetworkDataManager.Instance?.SkorVeHamleGuncelleServerRpc(1, 0, LobbyManager.Instance.myDisplayName);
        } 
    }

    public void LimitleriKontrolEt(){
        // eğer multi player ise
        if (!MainMenu.isSoloGame){
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
                if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Single);
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Single);
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
        } 
        
        // eğer solo ise
        if(MainMenu.isSoloGame){ 
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
                if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Single);
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Single); 
                }
            }
        }
    }

    public void PerlerleriNetlestir(){
        PtasIleUyumluMeyveFarkliRenkAyni.Clear();
        PtasIleUyumluMeyveAyniRenkAyni.Clear();
        PtasIleUyumluMeyveAyniFarkliRenk.Clear();

        SiralanmisTumPerlerdekiTaslar = new SortedDictionary<int, GameObject>();
        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>();

        foreach (var MfRa_Peri in Istaka.Instance.FarkliMeyveAyniRenkPerleri){
            foreach (var item in MfRa_Peri){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MfRa_Peri.Count > 2){
                    PtasIleUyumluMeyveFarkliRenkAyni.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }


        foreach (var MaRa_Peri in Istaka.Instance.AyniMeyveAyniRenkPerleri){
            foreach (var item in MaRa_Peri){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MaRa_Peri.Count > 2){
                    PtasIleUyumluMeyveAyniRenkAyni.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }


        foreach (var MaRf_Peri in Istaka.Instance.AyniMeyveFarkliRenkPerleri){
            foreach (var item in MaRf_Peri){
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)){
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MaRf_Peri.Count > 2){
                    PtasIleUyumluMeyveAyniFarkliRenk.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }

        SiralanmisTumPerlerdekiTaslar = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic);
    }

    public void RengiVeMeyvesiUyumluKarttakiTaslariIsaretle(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        // Meyveler Farklı Renkler Aynı   A  B  C  D  E 
        if (PtasIleUyumluMeyveFarkliRenkAyni.Count > 2){
            foreach (var tasInstance in PtasIleUyumluMeyveFarkliRenkAyni){
                foreach (var cTas in cardtakiTaslar){
                    var cTasInstance = TasManeger.Instance.TasInstances[cTas];
                    if (PtasIleUyumluMeyveFarkliRenkAyni.Count == 3){
                        // bonus yok
                    }

                    if (PtasIleUyumluMeyveFarkliRenkAyni.Count == 4){
                        // R a M a 
                        if (tasInstance.MeyveID == cTasInstance.MeyveID && tasInstance.Renk == cTasInstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.PtasIleUyumlu = true;
                            cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                            cTasInstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveFarkliRenkAyni.Count == 5){
                        // R a M * 
                        if (tasInstance.Renk == cTasInstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.PtasIleUyumlu = true;
                            cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                            cTasInstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveFarkliRenkAyni.Count >= 6){
                        // R * M *  
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count, cTasInstance);
                        cTasInstance.PtasIleUyumlu = true;
                        cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                        cTasInstance.TiklanaBilir = false;
                    }
                }
            }
        }

        //  Meyve Aynı Renk Aynı  M M M M M (Aynı renk)
        if (PtasIleUyumluMeyveAyniRenkAyni.Count > 2){
            foreach (var tasInstance in PtasIleUyumluMeyveAyniRenkAyni){
                foreach (var item in cardtakiTaslar){
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (PtasIleUyumluMeyveAyniRenkAyni.Count == 3){
                        // R a M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID && tasInstance.Renk == itemIstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemIstance);
                            itemIstance.PtasIleUyumlu = true;
                            itemIstance.PtasIleUyumluGostergesi.SetActive(true);
                            itemIstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveAyniRenkAyni.Count == 4){
                        // R *  M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemIstance);
                        }
                    }

                    if (PtasIleUyumluMeyveAyniRenkAyni.Count == 5){
                        // R a M * 
                        if (tasInstance.Renk == itemIstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemIstance);
                            itemIstance.PtasIleUyumlu = true;
                            itemIstance.PtasIleUyumluGostergesi.SetActive(true);
                            itemIstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveAyniRenkAyni.Count >= 6){
                        // R *  M * 
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                            itemIstance);
                        itemIstance.PtasIleUyumlu = true;
                        itemIstance.PtasIleUyumluGostergesi.SetActive(true);
                        itemIstance.TiklanaBilir = false;
                    }
                }
            }
        }

        // Meyve Aynı Renk Farklı  M M M M M (farklı renk)
        if (PtasIleUyumluMeyveAyniFarkliRenk.Count > 2){
            foreach (var tasInstance in PtasIleUyumluMeyveAyniFarkliRenk){
                foreach (var item in cardtakiTaslar){
                    var itemInstance = TasManeger.Instance.TasInstances[item];
                    if (PtasIleUyumluMeyveAyniFarkliRenk.Count == 3){
                        // R a M a 
                        if (tasInstance.MeyveID == itemInstance.MeyveID && tasInstance.Renk == itemInstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemInstance);
                            itemInstance.PtasIleUyumlu = true;
                            itemInstance.PtasIleUyumluGostergesi.SetActive(true);
                            itemInstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveAyniFarkliRenk.Count == 4){
                        // R * M a 
                        if (tasInstance.MeyveID == itemInstance.MeyveID){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemInstance);
                            itemInstance.PtasIleUyumlu = true;
                            itemInstance.PtasIleUyumluGostergesi.SetActive(true);
                            itemInstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveAyniFarkliRenk.Count == 5){
                        // R a M * 
                        if (tasInstance.Renk == itemInstance.Renk){
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                                itemInstance);
                            itemInstance.PtasIleUyumlu = true;
                            itemInstance.PtasIleUyumluGostergesi.SetActive(true);
                            itemInstance.TiklanaBilir = false;
                        }
                    }

                    if (PtasIleUyumluMeyveAyniFarkliRenk.Count >= 6){
                        // R * M *  
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,
                            itemInstance);
                        itemInstance.PtasIleUyumlu = true;
                        itemInstance.PtasIleUyumluGostergesi.SetActive(true);
                        itemInstance.TiklanaBilir = false;
                    }
                }
            }
        }

        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap &&
            SiralanmisTumPerlerdekiTaslar != null){
            GorevYoneticisi.Instance.GoreveUygunCEPLERIIsaretle();
            GorevYoneticisi.Instance.SiradakiGoreviSahnedeGoster();
        }

        foreach (var pTas in SiralanmisTumPerlerdekiTaslar){
            var tasScript = TasManeger.Instance.TasInstances[pTas.Value];
            tasScript.AltinVeElmasGoster(); 
            //tasScript.TaslarinRakaminiPuanaEkle();
        }
 
        /*float gecikme = 1f;
        Invoke(nameof(Deneme), gecikme);*/
    }

   /* void Deneme(){
        GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.DevamEdiyor;
    }*/

    public void Puanla(){
        Card.Instance.Sallanma(); 
        if (Istaka.Instance.FarkliMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveFarkliRenkPerleri.Count > 0){
            //GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.PuanlamaYapiliyor; 
            OyunSahnesiUI.Instance.puanlamaYap.style.display = DisplayStyle.None;
            PerlerleriNetlestir();
            RengiVeMeyvesiUyumluKarttakiTaslariIsaretle();
            Card.Instance.GoreveUyumluCtasYoket(); 
            Card.Instance.PtasIleUyumluCtaslariYoket();
            Card.Instance.PtaslariYoket();
            LimitleriKontrolEt(); 
            Istaka.Instance.PerlerListObjeleriTemizle(); // en son temizlenmeli
        }
    } 
}
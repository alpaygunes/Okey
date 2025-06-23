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
    public int hamleSayisi;
 

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
                if (hamleSayisi >= OyunKurallari.Instance.HamleLimit){
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
                if (hamleSayisi >= OyunKurallari.Instance.HamleLimit){
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
    
    public void Puanla(){ 
        if ( PerKontrolBirimi.Instance.Gruplar.Count>0){ 
            OyunSahnesiUI.Instance.puanlamaYap.style.display = DisplayStyle.None; 
            Card.Instance.CardtakiBonusTaslariBelirt(); 
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap &&
                PerKontrolBirimi.Instance.Gruplar != null){
                GorevYoneticisi.Instance.GoreveUygunCeplereBayrakKoy();
                GorevYoneticisi.Instance.SiradakiGoreviSahnedeGoster();
            }
            Card.Instance.TaslariAltinVeElmasaDonustur(); 
            Card.Instance.GoreveUyumluCtasYoket(); 
            Card.Instance.PtasIleUyumluCtaslariYoket();
            Istaka.Instance.PtaslariYoket();
            LimitleriKontrolEt(); 
            Istaka.Instance.PerlerListObjeleriTemizle(); // en son temizlenmeli
        }
    } 
}
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UIElements;

public class IsaretleBelirtYoket : MonoBehaviour{
    public static IsaretleBelirtYoket Instance{ get; private set; }
    //private Camera uiCamera; 
    public int HamleSayisi;
 

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        //uiCamera = Camera.main; 
    }

    private void Start(){
        if (!MainMenu.isSoloGame){
            // boş bir değişim yaratalım. skorl sitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
            MultiPlayerVeriYoneticisi.Instance?.OyuncuVerileriniGuncelle();
        } 
    }

    public void LimitleriKontrolEt(){
        // eğer multi player ise
        if (!MainMenu.isSoloGame){
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
                if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                    if (MultiPlayerVeriYoneticisi.Instance.OyuncuListesi != null){
                        IEnumerator enumerator;
                        try{
                            enumerator = MultiPlayerVeriYoneticisi.Instance.SkorListesiniYavasGuncelle();
                            MultiPlayerVeriYoneticisi.Instance.skorListesiniYavasGuncelleCoroutine
                                = MultiPlayerVeriYoneticisi.Instance.StartCoroutine(enumerator);
                        }
                        catch (Exception e){
                            Debug.Log($"Host ayrışmış olabilir . LimitleriKontrolEt HATA : {e.Message}");
                        }
                    }
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                    if (MultiPlayerVeriYoneticisi.Instance.OyuncuListesi != null){
                        IEnumerator enumerator;
                        try{
                            enumerator = MultiPlayerVeriYoneticisi.Instance.SkorListesiniYavasGuncelle();
                            MultiPlayerVeriYoneticisi.Instance.skorListesiniYavasGuncelleCoroutine
                                = MultiPlayerVeriYoneticisi.Instance.StartCoroutine(enumerator);
                        }
                        catch (Exception e){
                            Debug.Log($"Host ayrışmış olabilir . LimitleriKontrolEt HATA : {e.Message}");
                        }
                    }
                }
            }

            // görev yap modunun zaman kontrolu burada. groevsayısı kontroulu gerevyonetimi içinde yapılıyor
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    if (NetworkManager.Singleton.IsHost){
                        NetworkManager.Singleton.SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                    }
                    else{
                        SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                    }
                   
                    if (MultiPlayerVeriYoneticisi.Instance.OyuncuListesi != null){
                        IEnumerator enumerator;
                        try{
                            enumerator = MultiPlayerVeriYoneticisi.Instance.SkorListesiniYavasGuncelle();
                            MultiPlayerVeriYoneticisi.Instance.skorListesiniYavasGuncelleCoroutine
                                = MultiPlayerVeriYoneticisi.Instance.StartCoroutine(enumerator);
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
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive); 
                }
            } 

            // görev yap modunun zaman kontrolu burada. groevsayısı kontroulu gerevyonetimi içinde yapılıyor
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.oyunDurumu = GameManager.OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive); 
                }
            } 
        }
    }
    
    public void Degerlendir(){ 
        Card.Instance.Sallanma();
        HamleSayisi++; 
        if ( PerKontrolBirimi.Instance.Gruplar.Count>0){
            OyunSahnesiUI.Instance.degerlendirmeYap.style.display = DisplayStyle.None; 
            Card.Instance.CardtakiBonusTaslariBelirt();
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap &&
                PerKontrolBirimi.Instance.Gruplar != null){
                GorevYoneticisi.Instance.GoreveUygunCeplereBayrakKoy(); 
                Card.Instance.TaslariAltinVeElmasaDonustur();
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster(); 
            }
              
            PuanlamaIStatistikleri.Sakla();
            Card.Instance.GoreveUyumluCtasYoket();
            Card.Instance.PtasIleUyumluCtaslariYoket();
            Istaka.Instance.PtaslariYoket(); 
        }
        
    } 
}
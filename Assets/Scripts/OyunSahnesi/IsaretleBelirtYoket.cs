using System;
using System.Collections; 
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
            // boş bir değişim yaratalım. skorlsitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
            MultiPlayerVeriYoneticisi.Instance?.SkorVeHamleGuncelleServerRpc();
        } 
    }

    public void LimitleriKontrolEt(){
        // eğer multi player ise
        if (!MainMenu.isSoloGame){
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
                if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
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
        } 
        
        // eğer solo ise
        if(MainMenu.isSoloGame){ 
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
                if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                }
            }

            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
                if (GameManager.Instance.oyununBitimineKalanZaman <= 0){
                    GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.LimitDoldu;
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
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster();
            }
            Card.Instance.TaslariAltinVeElmasaDonustur();  
            PuanlamaIStatistikleri.Sakla();
            Card.Instance.GoreveUyumluCtasYoket();
            Card.Instance.PtasIleUyumluCtaslariYoket();
            Istaka.Instance.PtaslariYoket();
            LimitleriKontrolEt();
        }
    } 
}
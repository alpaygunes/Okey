using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour{
    public readonly float PuanlamaIcinGeriSayimSuresi = 100f;
    public readonly int _colonCount = 6;
    public readonly int BaslangicTasSayisi = 100;
    public readonly int CepSayisi = 6;
    public readonly RangeInt RenkAraligi = new RangeInt(0, 6);
    public readonly RangeInt MeyveIDAraligi = new RangeInt(0, 6);
    public List<GameObject> spawnHolesList = new List<GameObject>();
    public string seed;
    public static GameManager Instance{ get; private set; }
    public int oyununBitimineKalanZaman = 0;
    public OynanmaDurumu OyunDurumu;
    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public int YeniTasEklendiSayisi = 0;
    public bool OyunSahnesiKapaniyor{ get; set; } = false;

    public enum OynanmaDurumu{
        LimitDoldu,
        DevamEdiyor,
        PuanlamaYapiliyor,
    }

    void Awake(){
        if (MainMenu.isSoloGame){
            OyunKurallari.Instance.InitializeSettings();
        }

        OyunDurumu = OynanmaDurumu.DevamEdiyor;
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start(){
        seed = "A";
        // solo
        if (MainMenu.isSoloGame){
            seed = MainMenu.GetRandomSeed();
        }
        else if (LobbyManager.Instance){
            // multy
            seed = LobbyManager.Instance.gameSeed;
        }


        CreateSpawnHoles();
        TasManeger.Instance.TaslariOlustur();
        Card.Instance.KutulariHazirla();


        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            // solo ise
            if (MainMenu.isSoloGame){
                GorevYoneticisi.GorevHazirla();
                GorevYoneticisi.Instance.SiradakiGoreviSahnedeGoster();
            }
            // Multi ise GorevYoneticisi OnNetworkSpawn() olunca tetiklenir.
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            // puanlama sırasında gereken kotrol yapılıyor.
        }

        OyunSahnesiUI.Instance.KalanTasSayisi.text = TasManeger.Instance.TasList.Count.ToString();
    }

    private void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / _colonCount;
        for (int i = 0; i < _colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            SpawnHole.GetComponent<SpawnHole>().colID = i;
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth, colonWidth);
            spawnHolesList.Add(sh);
        }
    }

    private IEnumerator OyununBitimiIcinGeriSayRoutine(){
        oyununBitimineKalanZaman = 120;
        if (OyunKurallari.Instance){
            oyununBitimineKalanZaman = OyunKurallari.Instance.ZamanLimiti;
        }

        while (oyununBitimineKalanZaman > 0){
            OyunSahnesiUI.Instance.GeriSayim.text = oyununBitimineKalanZaman.ToString();
            yield return new WaitForSeconds(1f);
            oyununBitimineKalanZaman--;
        }

        OyunSahnesiUI.Instance.GeriSayim.text = "0";
        if (OyununBitimiIcinGeriSayRoutineCoroutin != null){
            StopCoroutine(OyununBitimiIcinGeriSayRoutineCoroutin);
            OyununBitimiIcinGeriSayRoutineCoroutin = null;
        }

        Puanlama.Instance.LimitleriKontrolEt();
    }

    public void DugmeGosterilsinmi(){
        if (Istaka.Instance.FarkliMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveFarkliRenkPerleri.Count > 0){
            OyunSahnesiUI.Instance.puanlamaYap.style.display = DisplayStyle.Flex;
            if (Istaka.Instance.DoluCepSayisi() == CepSayisi){
                Puanlama.Instance.Puanla();
                Card.Instance.Sallanma();
            }
        }
        else if (Istaka.Instance.DoluCepSayisi() == CepSayisi){
            Istaka.Instance.EgerPerYoksaTumTaslarinGostergesiniAc();
            Istaka.Instance.IstakayiVeCartiTemizle();
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap)
                Istaka.Instance.CeptekiYildizleriGizle();
        }
    }


    void Update(){
        if (OyunDurumu == OynanmaDurumu.LimitDoldu) return;
        // tıklama algılama
#if UNITY_ANDROID || UNITY_IOS
                            if (Input.touchCount > 0)
                            {
                                Touch touch = Input.GetTouch(0);
                                if (touch.phase == TouchPhase.Began)
                                {
                                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                                    RaycastVeIslem(worldPoint);
                                }
                            }
#else
        if (Input.GetMouseButtonDown(0)){
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastVeIslem(worldPoint);
        }
#endif
        // tıklana bilir nesne varsa oyun durumunu değiştirelim
        bool TiklanamazTasVar = false; 
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasIstance = TasManeger.Instance.TasInstances[cTas];
            if (cTasIstance.TiklanaBilir == false){
                TiklanamazTasVar = true;
                break;
            }
        }

        if (!TiklanamazTasVar){
            foreach (var pTas in perdekiTaslar){
                var pTasIstance = TasManeger.Instance.TasInstances[pTas];
                if (pTasIstance.TiklanaBilir == false){
                    TiklanamazTasVar = true;
                    break;
                }
            }
        }

        if (TiklanamazTasVar){
            OyunDurumu = OynanmaDurumu.PuanlamaYapiliyor;
        }
        else{
            OyunDurumu = OynanmaDurumu.DevamEdiyor;
        }
    }

    void RaycastVeIslem(Vector2 worldPoint){
        if (OyunDurumu != OynanmaDurumu.DevamEdiyor) return;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider != null){
            if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")){
                var tasInstance = TasManeger.Instance.TasInstances[hit.collider.gameObject];
                if (!tasInstance.TiklanaBilir) return;
                var yerlestimi = tasInstance.BosCebeYerles();
                Card.Instance.Sallanma();
                if (yerlestimi){ 
                    if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
                        GorevYoneticisi.Instance.CepGoreveUyduysaYildiziYak(tasInstance);
                    } 
                    Istaka.Instance.PerleriBul();
                    Istaka.Instance.PerdekiTaslariBelirginYap();
                    DugmeGosterilsinmi();
                }

                PerIcinTasTavsiye.Instance.Basla();
            }
        }
    }
}
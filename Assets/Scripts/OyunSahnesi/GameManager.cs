using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour{
    public readonly float PuanlamaIcinGeriSayimSuresi = 100f;  
    public readonly int _colonCount = 6;
    public readonly int TasCount = 100;
    public readonly int CepSayisi = 6;
    public readonly RangeInt RenkAraligi = new RangeInt(0, 6);
    public readonly RangeInt MeyveIDAraligi = new RangeInt(0, 6);  
    public List<GameObject> spawnHolesList = new List<GameObject>(); 
    public string seed;
    //public bool PerKontrolDugmesiOlsun ;
    //public bool OtomatikPerkontrolu ;
    public static GameManager Instance{ get; private set; }
    public int oyununBitimineKalanZaman=0;  
    public OynanmaDurumu OyunDurumu;
    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public enum OynanmaDurumu{
        bitti,
        oynaniyor,
        puanlamaYapiliyor,
    }
    
    void Awake(){
        Debug.Log("GameManager yeniden canlandı");
        if (MainMenu.isSoloGame){
            OyunKurallari.Instance.InitializeSettings();  
        }
        
        OyunDurumu = OynanmaDurumu.oynaniyor; 
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start(){
        if (LobbyManager.Instance){
            seed = LobbyManager.Instance.gameSeed;
        } else{ 
            seed = "1234";
        }

        CreateSpawnHoles();
        TasManeger.Instance.TaslariHazirla();
        Card.Instance.KutulariHazirla();
         
 
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){ 
            // solo ise
            if (MainMenu.isSoloGame){
                GorevYoneticisi.GorevHazirla();
                GorevYoneticisi.Instance.SiradakiGoreviSahnedeGoster();
            }
            // Multi ise GorevYoneticisi OnNetworkSpawn() olunca tetiklenir.
        }else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            // puanlama sırasında gereken kotrol yapılıyor.
        } 
    }
    
    private void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / _colonCount;
        for (int i = 0; i < _colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            SpawnHole.GetComponent<SpawnHole>().colID =  i;
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
        if (OyununBitimiIcinGeriSayRoutineCoroutin!=null){
            StopCoroutine(OyununBitimiIcinGeriSayRoutineCoroutin);
            OyununBitimiIcinGeriSayRoutineCoroutin = null;
        } 
        Puanlama.Instance.LimitleriKontrolEt();
    }

    public void PuanlamaDugmesiniGoster(){
        if (Istaka.Instance.FarkliMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveAyniRenkPerleri.Count > 0
            || Istaka.Instance.AyniMeyveFarkliRenkPerleri.Count > 0 ){ 
            OyunSahnesiUI.Instance.puanlamaYap.style.display = DisplayStyle.Flex; 
            if (Istaka.Instance.DoluCepSayisi() == CepSayisi) 
                Puanlama.Instance.Puanla();
            
        }
        else{
            Istaka.Instance.IstakayiVeCartiTemizle();
        }
    }
    
    void Update(){
        if (OyunDurumu == OynanmaDurumu.bitti) return;
        
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
                void RaycastVeIslem(Vector2 worldPoint){
                    RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                    if (hit.collider != null){
                        if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")){
                            var yerlestimi = TasManeger.Instance.TasInstances[hit.collider.gameObject].BosCebeYerles();
                            if (yerlestimi){  
                                Card.Instance.Sallanma();
                                GorevYoneticisi.Instance.TasGoreveUygunsaYildiziYak(TasManeger.Instance.TasInstances[hit.collider.gameObject]); 
                                Istaka.Instance.PerleriBul();
                                Istaka.Instance.PerdekiTaslariBelirginYap();
                                PuanlamaDugmesiniGoster(); 
                            }
                            PerIcinTasTavsiye.Instance.Basla();
                        }
                    }
                }
    }
    
}
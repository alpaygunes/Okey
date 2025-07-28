using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class Tas : MonoBehaviour {
    public int MeyveID;
    public Color Renk;
    private Rigidbody2D _rigidbody;
    public SpriteRenderer zeminSpriteRenderer;
    public SpriteRenderer MeyveResmiSpriteRenderer;
    private Vector3 skorTxtPosition;
    private Object _collider;
    public Cep cepInstance = null;
    public Tweener tweener = null;
    public Vector3 orginalScale;
    public GameObject zemin;
    public Dictionary<int, Tas> BonusOlarakEslesenTaslar = new Dictionary<int, Tas>();
    public TextMeshPro TextMeyveID;
    public int colID;
    private readonly int sallanmakIcinBeklemeSuresi = 2;
    private Coroutine sallanmaCoroutine;

    public int gorevleUyumBayragi = 0; //0 = yok,
                                       //1 = gorevle,
                                       //2 = gorevle ve yok .
                                       //Bu alan Cepteki tas için. Cart takiler icin degil

    public GameObject gorevUyumGostergesi1;
    public GameObject gorevUyumGostergesi2;
    public GameObject pereUyumluGostergesi; // cepteki daslar per halindeyse belirtir.
    public GameObject meyveResmi;
    public bool bonusBayragi; // Bu alan carttaki taslar için . ceptaki tas için degil
    public GameObject ptasIleUyumluGostergesi; // cardtaki taslar için 
    public GameObject persizIstakaTaslariGostergesi;
    public List<GameObject> ayniKolondakiAltinveElmasTaslar = new List<GameObject>();
    public bool tiklanaBilir  = true;
    public Dictionary<int, GameObject> PerAilesi  = null;
    public bool sallanmaDurumu = false;
    private bool cebeYerles = false;
    private Cep hedefCep;
    public bool kilitli = false;
    public Kutu kutuInstance = null;

    private void Awake() {
        bonusBayragi = false;
        TextMeyveID = transform.Find("TextMeyveID").GetComponent<TextMeshPro>();
        zemin = transform.Find("Zemin").gameObject;
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        MeyveResmiSpriteRenderer = transform.Find("MeyveResmi").GetComponent<SpriteRenderer>();
        gorevUyumGostergesi1 = transform.Find("GorevUyumGostergesi1").gameObject;
        gorevUyumGostergesi2 = transform.Find("GorevUyumGostergesi2").gameObject;
        pereUyumluGostergesi = transform.Find("PereUyumluGostergesi").gameObject;
        meyveResmi = transform.Find("MeyveResmi").gameObject;
        ptasIleUyumluGostergesi = transform.Find("PtasIleUyumluGostergesi").gameObject;
        persizIstakaTaslariGostergesi = transform.Find("PersizIstakaTaslariGostergesi").gameObject;
        skorTxtPosition = new Vector3(0, 0, 0);
    }

    private void Start() {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        var acikRenk = Color.Lerp(Renk, Color.white, 1f);
        zeminSpriteRenderer.color = acikRenk;
        Sprite sprite = Resources.Load<Sprite>("Images/Meyveler/" + MeyveID);
        MeyveResmiSpriteRenderer.sprite = sprite;
        var koyuRenk = Color.Lerp(Renk, Color.black, 0.0f);
        MeyveResmiSpriteRenderer.color = koyuRenk;
        MeyveResmiSpriteRenderer.transform.localScale *= 1.25f;
        TextMeyveID.text = MeyveID.ToString();
        orginalScale = MeyveResmiSpriteRenderer.transform.localScale;
        gorevUyumGostergesi1.SetActive(false);
        gorevUyumGostergesi2.SetActive(false);
        pereUyumluGostergesi.SetActive(false);
        ptasIleUyumluGostergesi.SetActive(false);
        persizIstakaTaslariGostergesi.SetActive(false);
    }

    private void OnDestroy() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
        foreach (var collider in colliders) {
            if (collider.transform.CompareTag("SPAWN_HOLDER")) {
                SpawnHole spawnHole = collider.GetComponent<SpawnHole>();
                if (spawnHole != null) {
                    spawnHole.musait = true;
                }
            }
        }

        if (cepInstance) {
            cepInstance.Dolu = false;
        }

        try {
            // Sıradaki taslar
            var SiradakiTaslar = TasManeger.Instance.TasList.Count;
            // Cardtaki TAslar
            var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
            // Perdeki Taslar
            var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");

            int ToplamTasSayisi = SiradakiTaslar + carddakiTaslar.Length + perdekiTaslar.Length;

            //taş sayısı başlangıc sayısının yarısının altına indiyse yeni taşlar eklensin.
            if (ToplamTasSayisi < GameManager.Instance.BaslangicTasSayisi * 0.5f
                && GameManager.Instance.OyunDurumu == GameManager.OyunDurumlari.DevamEdiyor) {
                TasManeger.Instance.TaslariOlustur();
            }

            TasManeger.Instance.TasInstances.Remove(gameObject);
            OyunSahnesiUI.Instance.KalanTasSayisi.text = (ToplamTasSayisi - 1).ToString();
            PuanlamaIStatistikleri.ToplamTasSayisi = ToplamTasSayisi;
        }
        catch (Exception e) {
            Debug.Log($" Tas.cs OnDestroy içinde HATA : \n {e.Message}");
        }
    }

    public bool BosCebeYerles() {
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++) {
            hedefCep = Istaka.Instance.CepList[i];
            if (hedefCep.Dolu == false) {
                hedefCep.Dolu = true;
                hedefCep.TasInstance = this;
                cepInstance = hedefCep;
                sallanmaDurumu = false;
                cebeYerles = true;
                StartCoroutine(RigidbodyVeCollideriSilGecikmeli());
                return true;
            }
        }

        return false;
    }

    IEnumerator RigidbodyVeCollideriSilGecikmeli() {
        yield return new WaitForFixedUpdate(); // 1 fizik frame bekle
        tag = "CEPTEKI_TAS";
        Destroy(_rigidbody);
        PerIcinUygunTaslariBelirt.Bul();
    }

    private void FixedUpdate() {
        if (cebeYerles) {
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            Destroy(_collider);
            Vector2 cardSize = Card.Instance.Size;
            float colonWidth = cardSize.x / GameManager.Instance.cepSayisi;
            if (colonWidth > cardSize.x / 6) {
                colonWidth = cardSize.x / 6;
            }

            transform.localScale = new Vector3(colonWidth * 1.1f, colonWidth);
            var hedefCepPosition = new Vector3(
                hedefCep.transform.position.x,
                hedefCep.transform.position.y * .9f);
            _rigidbody.MovePosition(hedefCepPosition);
            cebeYerles = false;
        }
    }

    public IEnumerator BekleYokol(float gecikme) {
        
        if (kutuInstance && kutuInstance.KilitSayisi > 0) {
            kutuInstance.KilitSayisi--;
            tiklanaBilir = true;
            if(kutuInstance.KilitSayisi <= 0){ 
                kutuInstance.transform.Find("KilitBelirteci").gameObject.SetActive(false); 
                kilitli = false;
            } 
            kutuInstance.Kilitlen();
            yield break;
        }

        if (kutuInstance) {
            gecikme = 0;
        }

        yield return new WaitForSeconds(gecikme);
        if (this == null) yield break;
        if (cepInstance != null) cepInstance.TasInstance = null;
        if (gameObject != null)
            Destroy(gameObject);
    }

    private void Update() {
        if (sallanmaDurumu && !tweener.IsActive() && tweener == null) {
            if (sallanmaCoroutine == null) {
                sallanmaCoroutine = StartCoroutine(BekleSallan());
            }
        }

        if (!sallanmaDurumu && tweener.IsActive()) {
            tweener.Complete();
            tweener.Kill();
            tweener = null;
            MeyveResmiSpriteRenderer.transform.localScale = orginalScale;
            if (sallanmaCoroutine is not null) {
                StopCoroutine(sallanmaCoroutine);
                sallanmaCoroutine = null;
            }
        }
    }

    private IEnumerator BekleSallan() {
        if (kilitli) yield break;
        yield return new WaitForSeconds(sallanmakIcinBeklemeSuresi);
        if (sallanmaDurumu && !tweener.IsActive() && tweener == null) {
            tweener = MeyveResmiSpriteRenderer.transform.DOScale(.8f, .5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
        }
    }

    public void AltinVeElmasGoster() {
        if (gorevleUyumBayragi == 0) return;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar) {
            var cTasscript = TasManeger.Instance.TasInstances[cTas]; 
            if (cTasscript.colID == cepInstance.colID) {
                if (gorevleUyumBayragi == 1) {
                    cTasscript.gorevUyumGostergesi1.gameObject.SetActive(true);
                    cTasscript.meyveResmi.gameObject.SetActive(false);
                    tiklanaBilir = false;
                }
                else if (gorevleUyumBayragi == 2) {
                    cTasscript.gorevUyumGostergesi2.gameObject.SetActive(true);
                    cTasscript.meyveResmi.gameObject.SetActive(false);
                    tiklanaBilir = false;
                }
                
                if (cTasscript.kutuInstance && gorevleUyumBayragi >0 ) {
                    cTasscript.kilitli = false;
                    cTasscript.kutuInstance.KilitSayisi = 0; 
                    cTasscript.kutuInstance.Kilitlen(); 
                }

                ayniKolondakiAltinveElmasTaslar.Add(cTas);
            }
        }
    }
}
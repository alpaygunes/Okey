using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Tas : MonoBehaviour{
    public int MeyveID;
    public Color Renk; 
    private Rigidbody2D _rigidbody; 
    public SpriteRenderer zeminSpriteRenderer;
    public SpriteRenderer MeyveResmiSpriteRenderer;
    private Vector3 skorTxtPosition;
    public Camera uiCamera;
    private Object _collider;
 
    private AudioSource _audioSource_down;
    private AudioSource _audioSource_up;
    private AudioSource _audioSource_patla;
    public Cep cepInstance = null;
    public Tweener tweener = null;
    public Vector3 orginalScale;
    public GameObject zemin;
    public Dictionary<int, Tas> BonusOlarakEslesenTaslar = new Dictionary<int, Tas>();
    public bool NetworkDatayaEklendi = false;
    public TextMeshPro TextMeyveID;
    public int colID;

    public int
        GorevleUyumBayragi = 0; //0 = yok, 1 = gorevle, 2 = gorevle ve yok . Bu alan Cepteki tas için. Cart takiler icin degil

    public GameObject GorevUyumGostergesi1;
    public GameObject GorevUyumGostergesi2;
    public GameObject PereUyumluGostergesi; // cepteki daslar per halindeyse belirtir.
    public GameObject MeyveResmi;
    public bool BonusBayragi; // Bu alan carttaki taslar için . ceptaki tas için degil
    public GameObject PtasIleUyumluGostergesi; // cardtaki taslar için 
    public GameObject PersizIstakaTaslariGostergesi;
    public List<GameObject> AyniKolondakiAltinveElmasTaslar = new List<GameObject>();
    public bool TiklanaBilir{ get; set; } = true;
    public Dictionary<int, GameObject> PerAilesi{ get; set; } = null; 
    public bool sallanmaDurumu = false;

    private void Awake(){
        BonusBayragi = false;
        TextMeyveID = transform.Find("TextMeyveID").GetComponent<TextMeshPro>();
        zemin = transform.Find("Zemin").gameObject;
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        MeyveResmiSpriteRenderer = transform.Find("MeyveResmi").GetComponent<SpriteRenderer>();
        GorevUyumGostergesi1 = transform.Find("GorevUyumGostergesi1").gameObject;
        GorevUyumGostergesi2 = transform.Find("GorevUyumGostergesi2").gameObject;
        PereUyumluGostergesi = transform.Find("PereUyumluGostergesi").gameObject;
        MeyveResmi = transform.Find("MeyveResmi").gameObject;
        PtasIleUyumluGostergesi = transform.Find("PtasIleUyumluGostergesi").gameObject;
        PersizIstakaTaslariGostergesi = transform.Find("PersizIstakaTaslariGostergesi").gameObject;
        uiCamera = Camera.main;
        skorTxtPosition = new Vector3(0, 0, 0);
    }

    private void Start(){
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        var acikRenk = Color.Lerp(Renk, Color.white, 1f);
        zeminSpriteRenderer.color = acikRenk;
        Sprite sprite = Resources.Load<Sprite>("Images/Meyveler/" + MeyveID);
        MeyveResmiSpriteRenderer.sprite = sprite;

        var koyuRenk = Color.Lerp(Renk, Color.black, 0.0f);
        MeyveResmiSpriteRenderer.color = koyuRenk;
        MeyveResmiSpriteRenderer.transform.localScale *= 1.25f;
 
        _audioSource_down = gameObject.AddComponent<AudioSource>();
        _audioSource_down.playOnAwake = false;
        _audioSource_down.clip = Resources.Load<AudioClip>("Sounds/tas_down");

        _audioSource_up = gameObject.AddComponent<AudioSource>();
        _audioSource_up.playOnAwake = false;
        _audioSource_up.clip = Resources.Load<AudioClip>("Sounds/tas_up");

        _audioSource_patla = gameObject.AddComponent<AudioSource>();
        _audioSource_patla.playOnAwake = false;
        _audioSource_patla.clip = Resources.Load<AudioClip>("Sounds/tas_patla");

        TextMeyveID.text = MeyveID.ToString();
        orginalScale = MeyveResmiSpriteRenderer.transform.localScale;

        GorevUyumGostergesi1.SetActive(false);
        GorevUyumGostergesi2.SetActive(false);
        PereUyumluGostergesi.SetActive(false);
        PtasIleUyumluGostergesi.SetActive(false);
        PersizIstakaTaslariGostergesi.SetActive(false);
    }

    private void OnDestroy(){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
        foreach (var collider in colliders){
            if (collider.transform.CompareTag("SPAWN_HOLDER")){
                SpawnHole spawnHole = collider.GetComponent<SpawnHole>();
                if (spawnHole != null){
                    spawnHole.musait = true;
                }
            }
        }

        if (cepInstance){
            cepInstance.Dolu = false;  
        }

        try{
            // Sıradaki taslar
            var SiradakiTaslar = TasManeger.Instance.TasList.Count;
            // Cardtaki TAslar
            var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
            // Perdeki Taslar
            var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");

            int ToplamTasSayisi = SiradakiTaslar + carddakiTaslar.Length + perdekiTaslar.Length;

            //taş sayısı başlangıc sayısının yarısının altına indiyse yeni taşlar eklensin.
            if (ToplamTasSayisi < GameManager.Instance.BaslangicTasSayisi * 0.5f
                && GameManager.Instance.OyunDurumu == GameManager.OynanmaDurumu.DevamEdiyor){
                TasManeger.Instance.TaslariOlustur();
            }

            TasManeger.Instance.TasInstances.Remove(gameObject);
            OyunSahnesiUI.Instance.KalanTasSayisi.text = (ToplamTasSayisi - 1).ToString();
        }
        catch (Exception e){
            Debug.Log($" Tas.cs OnDestroy içinde HATA : \n {e.Message}"); 
        } 
    }

    public bool BosCebeYerles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance.CepSayisi;
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++){
            var hedefCep = Istaka.Instance.CepList[i];
            if (hedefCep.Dolu == false){
                transform.position += new Vector3(0, 0, -1);
                var hedef_cep_position = new Vector3(
                    hedefCep.transform.position.x,
                    hedefCep.transform.position.y * .9f,
                    hedefCep.transform.position.z);
                transform.localScale = new Vector3(colonWidth * 1.1f, colonWidth);
                transform.position = hedef_cep_position;
                hedefCep.Dolu = true;
                hedefCep.TasInstance = this;
                cepInstance = hedefCep;
                tag = "CEPTEKI_TAS";
                _audioSource_down.Play();
                Destroy(_rigidbody);
                Destroy(_collider);
                sallanmaDurumu = false;
                return true;
            }
        }

        return false;
    }

    public IEnumerator BekleYokol(float gecikme){
        yield return new WaitForSeconds(gecikme);
        if (cepInstance != null) cepInstance.TasInstance = null;
        Destroy(gameObject);
    }
 
    private void Update(){  
        if (sallanmaDurumu && !tweener.IsActive() && tweener == null){
            tweener = MeyveResmiSpriteRenderer.transform.DOScale(.8f, .5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
            
        }
        else if (!sallanmaDurumu && tweener != null && tweener.IsActive()){
            tweener.Complete();
            tweener.Kill();
            tweener = null;
            MeyveResmiSpriteRenderer.transform.localScale = orginalScale;
            
        }
    }

    public void AltinVeElmasGoster(){
        if (GorevleUyumBayragi == 0) return;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasscript = TasManeger.Instance.TasInstances[cTas];
            if (cTasscript.colID == cepInstance.colID){
                if (GorevleUyumBayragi == 1){
                    cTasscript.GorevUyumGostergesi1.gameObject.SetActive(true);
                    cTasscript.MeyveResmi.gameObject.SetActive(false);
                    TiklanaBilir = false;
                }
                else if (GorevleUyumBayragi == 2){
                    cTasscript.GorevUyumGostergesi2.gameObject.SetActive(true);
                    cTasscript.MeyveResmi.gameObject.SetActive(false);
                    TiklanaBilir = false;
                } 
                AyniKolondakiAltinveElmasTaslar.Add(cTas);
            }
        }
    }
}
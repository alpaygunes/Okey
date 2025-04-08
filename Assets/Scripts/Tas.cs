using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Tas : MonoBehaviour{
    public int rakam;
    public Color renk;
    private float _animasyonSuresi = 1f;
    public GameObject SagindakiKomsuTas;
    private Rigidbody2D _rigidbody;
    public bool birCardPerineDahil = false;
    public SpriteRenderer ZeminSpriteRenderer;
    private Vector3 _skorTxtPosition;
    public Camera uiCamera;
    private Object _collider;
    private GameObject destroyEffectPrefab;
    private AudioSource _audioSource_down;
    private AudioSource _audioSource_up;
    private AudioSource _audioSource_patla;
    public Cep cepInstance = null;
    public Tweener tweener = null;
    public Vector3 zeminlocalScale;
    public GameObject zemin;
    public Dictionary<int,Tas> BonusOlarakEslesenTaslar = new Dictionary<int,Tas>(); 

    private void Awake(){
        zemin = GameObject.Find("Zemin");
        gameObject.SetActive(false);
        ZeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        uiCamera = Camera.main;
        _skorTxtPosition = uiCamera.ScreenToWorldPoint(
            new Vector3(GameObject.Find("Skor").transform.position.x,
                GameObject.Find("Skor").transform.position.y, 30f));
    }

    private void Start(){
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        ZeminSpriteRenderer.color = renk;
        destroyEffectPrefab = Resources.Load<GameObject>("Prefabs/CFXR Magic Poof");

        _audioSource_down = gameObject.AddComponent<AudioSource>();
        _audioSource_down.playOnAwake = false;
        _audioSource_down.clip = Resources.Load<AudioClip>("Sounds/tas_down");

        _audioSource_up = gameObject.AddComponent<AudioSource>();
        _audioSource_up.playOnAwake = false;
        _audioSource_up.clip = Resources.Load<AudioClip>("Sounds/tas_up");

        _audioSource_patla = gameObject.AddComponent<AudioSource>();
        _audioSource_patla.playOnAwake = false;
        _audioSource_patla.clip = Resources.Load<AudioClip>("Sounds/tas_patla");

        zeminlocalScale = zemin.transform.localScale;  
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

        var kalanTas = Int32.Parse(Puanlama.Instance.KalanTasOraniTMP.text);
        kalanTas--;
        Puanlama.Instance.KalanTasOraniTMP.text = kalanTas.ToString();
    }

    public void BosCebeYerles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance.CepSayisi;
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++){
            var cepScript = Istaka.Instance.CepList[i];
            if (cepScript.Dolu == false){
                transform.DOMove(cepScript.transform.position, _animasyonSuresi * .5f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete((() => _audioSource_up.Play()));
                gameObject.transform.position += new Vector3(0, 0, -1);
                Destroy(_rigidbody);
                Destroy(_collider);
                gameObject.transform.localScale = new Vector3(colonWidth, colonWidth) * 0.9f;
                cepScript.Dolu = true;
                cepScript.TasInstance = this;
                cepInstance = cepScript;
                gameObject.tag = "CEPTEKI_TAS"; 
                zemin.transform.localScale = zeminlocalScale;
                _audioSource_down.Play();
                Sallanma(); 
                PerIcinTasTavsiye.Instance.Sallanma();
                TasManeger.Instance.PerleriKontrolEt();
                break;
            }
        }
    }
 
    public IEnumerator TaslarinRakaminiPuanaEkle(float gecikme){
        yield return new WaitForSeconds(gecikme);
        float sure = 1;
        Puanlama.Instance.PerlerdenKazanilanPuan += rakam;  
        _audioSource_patla.Play(); 
        Vector3 ilkScale = transform.localScale;
        transform.DOMove(_skorTxtPosition, sure);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(transform.DOScale(transform.localScale * 2, sure * .5f))
            .Append(transform.DOScale(ilkScale * .25f, sure * .5f))
            .OnComplete(() => {  
                transform.DOKill();
                Destroy(gameObject);
                enabled = false;
                TasManeger.Instance.TasInstances.Remove(gameObject);
            });
        foreach (var bonusTaslari in BonusOlarakEslesenTaslar){
            if ( bonusTaslari.Value ){
                bonusTaslari.Value.RakamiPuanaEkle(); 
            }
        }
        Puanlama.Instance.SkorBoardiGuncelle(); 
    }
    
    public void RakamiPuanaEkle(){ 
        Puanlama.Instance.PerlerdenKazanilanPuan += rakam;   
        Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity); 
        Destroy(gameObject); 
        this.enabled = false;
        TasManeger.Instance.TasInstances.Remove(gameObject); 
    }

    public IEnumerator CezaliRakamiCikar(float gecikme){ 
        yield return new WaitForSeconds(gecikme);
        if (gameObject){
            Destroy(gameObject); 
        }
        this.enabled = false;
        TasManeger.Instance.TasInstances.Remove(gameObject);
        Puanlama.Instance.PerlerdenKazanilanPuan -= rakam;
        Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
    }
    
    void OnTriggerStay2D(Collider2D other){
        if (other.gameObject.CompareTag("CARDTAKI_TAS")){
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
            if (edgeCollider != null && edgeCollider.IsTouching(other)){
                if (other is BoxCollider2D){
                    SagindakiKomsuTas = other.gameObject;
                }
            }
        }
    }

    private void Update(){
        if (gameObject.CompareTag("CARDTAKI_TAS")){
            Card.Instance.TaslarHareketli = (_rigidbody.linearVelocity.magnitude > 0.01f);
        }
        else{
            Card.Instance.TaslarHareketli = false;
            this.enabled = false;
        }
    }

    public async Task SiraliAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        Card.Instance.SiraliAyniRenkliGrup.Add(gameObject);
        if (SagindakiKomsuTas){
            var sagdakininRengi = TasManeger.Instance.TasInstances[SagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasInstances[SagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam + 1){
                await TasManeger.Instance.TasInstances[SagindakiKomsuTas].SiraliAyniRenkGrubunaDahilOl();
            }
        }
    }

    public async Task SiraliFarkliRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        bool gruptaAyniRenkYok = true;
        foreach (var gruptakiTas in Card.Instance.SiraliFarkliRenkliGrup){
            if (TasManeger.Instance.TasInstances[gruptakiTas].renk == renk){
                gruptaAyniRenkYok = false;
                break;
            }
        }

        if (gruptaAyniRenkYok){
            Card.Instance.SiraliFarkliRenkliGrup.Add(gameObject);
            if (SagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[SagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[SagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam + 1){
                    await TasManeger.Instance.TasInstances[SagindakiKomsuTas].SiraliFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }

    public async Task AyniRakamAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        Card.Instance.AyniRakamAyniRenkliGrup.Add(gameObject);
        if (SagindakiKomsuTas){
            var sagdakininRengi = TasManeger.Instance.TasInstances[SagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasInstances[SagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam){
                await TasManeger.Instance.TasInstances[SagindakiKomsuTas].AyniRakamAyniRenkGrubunaDahilOl();
            }
        }
    }

    public async Task AyniRakamFarkliRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        bool gruptaAyniRenkYok = true;
        //grupta zaten aynı renk varsa per olmaz . Çık
        foreach (var gruptakiTas in Card.Instance.AyniRakamFarkliRenkli){
            if (TasManeger.Instance.TasInstances[gruptakiTas].renk == renk){
                gruptaAyniRenkYok = false;
                break;
            }
        }

        if (gruptaAyniRenkYok){
            Card.Instance.AyniRakamFarkliRenkli.Add(gameObject);
            if (SagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[SagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[SagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam){
                    await TasManeger.Instance.TasInstances[SagindakiKomsuTas].AyniRakamFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }

    public void Sallan(){
        if (CompareTag("CARDTAKI_TAS")){
            tweener =  zemin.transform.DOScale(1.2f, .3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
        }
    }

    public void Sallanma(){
        zemin.transform.localScale = zeminlocalScale;
        if (tweener != null && tweener.IsActive()){
            tweener.Complete();
            tweener.Kill();
            tweener = null;
        }
    }
}
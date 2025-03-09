using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

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
    public IstakaCebi cepInstance = null;
    


    private void Awake(){
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
        transform.Find("Zemin").GetComponent<SpriteRenderer>().color = renk;
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
    }

    
    private void OnDestroy(){ 
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
        foreach (var collider in colliders) {
            if (collider.transform.CompareTag("SPAWN_HOLDER")) {
                SpawnHole spawnHole = collider.GetComponent<SpawnHole>();
                if (spawnHole != null) {
                    spawnHole.musait = true;
                }
            }
        }
    }


    public void BosCebeYerles(){ 
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / Istaka.Instance.CepSayisi;
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++) {
            var cepScript = Istaka.Instance.CepList[i];
            //var cepScript = cep.GetComponent<IstakaCebi>();
            if (cepScript.Dolu == false) {
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
                Counter.Instance.KontrolIcinGeriSaymayaBasla();
                _audioSource_down.Play();  
                break;
            }
        }
    }

    public void PuaniVerMerkezeKay(float gecikme){
        StartCoroutine(CeptekiTasinRakaminiPuanaEkle(gecikme));
    }

    IEnumerator CeptekiTasinRakaminiPuanaEkle(float gecikme){
        yield return new WaitForSeconds(gecikme);
        Vector3 ilkScale = transform.localScale;
        transform.DOMove(_skorTxtPosition, _animasyonSuresi);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(transform.DOScale(transform.localScale * 2, _animasyonSuresi * .5f))
            .Append(transform.DOScale(ilkScale * .25f, _animasyonSuresi * .5f));
        StartCoroutine(KillSelf());
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan += rakam;
    }

    public IEnumerator RakamiPuanaEkle(float gecikme){
        gameObject.tag = "CARD_PERINDEKI_TAS";
        Invoke("Patla", Random.value*2);
        yield return new WaitForSeconds(gecikme); 
        DestroySelf();
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan += rakam;
    }
    
    public IEnumerator CezaliRakamiCikar(float gecikme){
        gameObject.tag = "CARD_PERINDEKI_TAS";  
        Invoke("Patla", Random.value*2);
        yield return new WaitForSeconds(gecikme); 
        DestroySelf();
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan -= rakam;
    }

    void Patla(){
        Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        _audioSource_patla.Play();
    }

    IEnumerator KillSelf(){
        yield return new WaitForSeconds(_animasyonSuresi);
        transform.DOKill();
        Destroy(gameObject);
        this.enabled = false;
        TasManeger.Instance.TasIstances.Remove(gameObject);
    }

    public void DestroySelf(){
        Destroy(gameObject);
        this.enabled = false;
        TasManeger.Instance.TasIstances.Remove(gameObject); 
    }

    void OnTriggerStay2D(Collider2D other){
        if (other.gameObject.CompareTag("CARDTAKI_TAS")) {
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
            if (edgeCollider != null && edgeCollider.IsTouching(other)) {
                if (other is BoxCollider2D) {
                    SagindakiKomsuTas = other.gameObject;
                }
            }
        }
    }

    private void Update(){
        if (gameObject.CompareTag("CARDTAKI_TAS")) {
            Card.Instance.TaslarHareketli = (_rigidbody.linearVelocity.magnitude > 0.01f);
            //TasManeger.Instance.TasSecileBilir = (_rigidbody.linearVelocity.magnitude < 0.5f);;
        }
        else {
            Card.Instance.TaslarHareketli = false;
            this.enabled = false;
        }
    }

    public async Task SiraliAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        CardKontrolcu.Instance.SiraliAyniRenkliGrup.Add(gameObject);
        if (SagindakiKomsuTas) {
            var sagdakininRengi = TasManeger.Instance.TasIstances[SagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasIstances[SagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam + 1) {
                await TasManeger.Instance.TasIstances[SagindakiKomsuTas].SiraliAyniRenkGrubunaDahilOl();
            }
        }
    }

    public async Task SiraliFarkliRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        bool gruptaAyniRenkYok = true;
        //grupta zaten aynı renk varsa per olmaz . Çık
        foreach (var gruptakiTas in CardKontrolcu.Instance.SiraliFarkliRenkliGrup) {
            if (TasManeger.Instance.TasIstances[gruptakiTas].renk == renk) {
                gruptaAyniRenkYok = false;
                break;
            }
        }

        if (gruptaAyniRenkYok) {
            CardKontrolcu.Instance.SiraliFarkliRenkliGrup.Add(gameObject);
            if (SagindakiKomsuTas) {
                var sagdakininRengi = TasManeger.Instance.TasIstances[SagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasIstances[SagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam + 1) {
                    await TasManeger.Instance.TasIstances[SagindakiKomsuTas].SiraliFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }

    public async Task AyniRakamAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        CardKontrolcu.Instance.AyniRakamAyniRenkliGrup.Add(gameObject);
        if (SagindakiKomsuTas) {
            var sagdakininRengi = TasManeger.Instance.TasIstances[SagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasIstances[SagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam) {
                await TasManeger.Instance.TasIstances[SagindakiKomsuTas].AyniRakamAyniRenkGrubunaDahilOl();
            }
        }
    }

    public async Task AyniRakamFarkliRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        bool gruptaAyniRenkYok = true;
        //grupta zaten aynı renk varsa per olmaz . Çık
        foreach (var gruptakiTas in CardKontrolcu.Instance.AyniRakamFarkliRenkli) {
            if (TasManeger.Instance.TasIstances[gruptakiTas].renk == renk) {
                gruptaAyniRenkYok = false;
                break;
            }
        }

        if (gruptaAyniRenkYok) {
            CardKontrolcu.Instance.AyniRakamFarkliRenkli.Add(gameObject);
            if (SagindakiKomsuTas) {
                var sagdakininRengi = TasManeger.Instance.TasIstances[SagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasIstances[SagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam) {
                    await TasManeger.Instance.TasIstances[SagindakiKomsuTas].AyniRakamFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Tas : MonoBehaviour{
    public int MeyveID;
    public Color renk;
    private float animasyonSuresi = 1f;
    public GameObject sagindakiKomsuTas;
    private Rigidbody2D _rigidbody;
    public bool birCardPerineDahil = false;
    public SpriteRenderer zeminSpriteRenderer;
    public SpriteRenderer MeyveResmiSpriteRenderer;
    private Vector3 skorTxtPosition;
    public Camera uiCamera;
    private Object _collider;
    private GameObject destroyEffectPrefab;
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
    public int GorevleUyum = 0; //0 = yok, 1 = gorevle, 2 = gorevle ve yok
    public GameObject GorevUyumGostergesi1;
    public GameObject GorevUyumGostergesi2;
    public GameObject PereUyumluGostergesi;

    private void Awake(){;
        TextMeyveID = transform.Find("TextMeyveID").GetComponent<TextMeshPro>();
        zemin = transform.Find("Zemin").gameObject;
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        MeyveResmiSpriteRenderer = transform.Find("MeyveResmi").GetComponent<SpriteRenderer>();
        GorevUyumGostergesi1 = transform.Find("GorevUyumGostergesi1").gameObject;
        GorevUyumGostergesi2 = transform.Find("GorevUyumGostergesi2").gameObject;
        PereUyumluGostergesi = transform.Find("PereUyumluGostergesi").gameObject;
        uiCamera = Camera.main;
        skorTxtPosition = new Vector3(0, 0, 0);
    }

    private void Start(){ 
        
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        var acikRenk = Color.Lerp(renk, Color.white, 1f);
        zeminSpriteRenderer.color = acikRenk;
        Sprite sprite = Resources.Load<Sprite>("Images/Meyveler/" + MeyveID); 
        MeyveResmiSpriteRenderer.sprite = sprite;

        var koyuRenk = Color.Lerp(renk, Color.black, 0.0f);
        MeyveResmiSpriteRenderer.color = koyuRenk;
        MeyveResmiSpriteRenderer.transform.localScale *= 1.25f;
         
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
        
        TextMeyveID.text = MeyveID.ToString(); 
        orginalScale = MeyveResmiSpriteRenderer.transform.localScale;
        
        GorevUyumGostergesi1.SetActive(false);
        GorevUyumGostergesi2.SetActive(false);
        PereUyumluGostergesi.SetActive(false);
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

        var kalanTas = Int32.Parse(OyunSahnesiUI.Instance.KalanTasSayisi.text);
        kalanTas--; 
        OyunSahnesiUI.Instance.KalanTasSayisi.text = kalanTas.ToString();
    }

    public bool BosCebeYerles(){
        Card.Instance.Sallanma();
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance.CepSayisi;
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++){
            var cepScript = Istaka.Instance.CepList[i];
            if (cepScript.Dolu == false){
                gameObject.transform.position += new Vector3(0, 0, -1);
                var hedef_cep_position = new Vector3(
                    cepScript.transform.position.x, 
                    cepScript.transform.position.y*.9f,
                    cepScript.transform.position.z);
                transform.DOMove(hedef_cep_position, animasyonSuresi * .2f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete((() => {
                        transform.localScale = new Vector3(colonWidth*1.1f, colonWidth);
                        _audioSource_up.Play();
                    })); 
                Destroy(_rigidbody);
                Destroy(_collider); 
                cepScript.Dolu = true;
                cepScript.TasInstance = this;
                cepInstance = cepScript; 
                tag = "CEPTEKI_TAS"; 
                _audioSource_down.Play();
                return true; 
            }
        }
        return false;
    }

    public IEnumerator TaslarinRakaminiPuanaEkle(float gecikme){
        yield return new WaitForSeconds(gecikme);
        float sure = 1;
        _audioSource_patla.Play();
        Vector3 ilkScale = transform.localScale;
        transform.DOMove(skorTxtPosition, sure);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(transform.DOScale(transform.localScale * 2, sure * .5f))
            .Append(transform.DOScale(ilkScale * .25f, sure * .5f))
            .OnComplete(() => {
                transform.DOKill();
                Destroy(gameObject);
                enabled = false;
                TasManeger.Instance.TasInstances.Remove(gameObject);
                if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) cepInstance.YildiziYak(0); 
                foreach (var bonusTaslari in BonusOlarakEslesenTaslar){
                    if (bonusTaslari.Value){
                        bonusTaslari.Value.RakamiPuanaEkle();
                    }
                }
            });
    }

    public void RakamiPuanaEkle(){
        Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        enabled = false;
        TasManeger.Instance.TasInstances.Remove(gameObject);
    }

    public IEnumerator CezaliRakamiCikar(float gecikme){
        yield return new WaitForSeconds(gecikme);
        if (gameObject){
            Destroy(gameObject);
        }

        enabled = false;
        TasManeger.Instance.TasInstances.Remove(gameObject);
        Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
    }

    void OnTriggerStay2D(Collider2D other){
        if (other.gameObject.CompareTag("CARDTAKI_TAS")){
            EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
            if (edgeCollider != null && edgeCollider.IsTouching(other)){
                if (other is BoxCollider2D){
                    sagindakiKomsuTas = other.gameObject;
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
            enabled = false;
        }
    }

    // public async Task SiraliAyniRenkGrubunaDahilOl(){
    //     if (birCardPerineDahil) return;
    //     Card.Instance.SiraliAyniRenkliGrup.Add(gameObject);
    //     if (sagindakiKomsuTas){
    //         var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
    //         var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].MeyveID;
    //         if (sagdakininRengi == renk && sagdakininRakami == MeyveID + 1){
    //             await TasManeger.Instance.TasInstances[sagindakiKomsuTas].SiraliAyniRenkGrubunaDahilOl();
    //         }
    //     }
    // }

    /*
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
            if (sagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].MeyveID;
                if (sagdakininRengi != renk && sagdakininRakami == MeyveID + 1){
                    await TasManeger.Instance.TasInstances[sagindakiKomsuTas].SiraliFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }*/

    /*
     public async Task AyniRakamAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        Card.Instance.AyniMeyveAyniRenkliGrup.Add(gameObject);
        if (sagindakiKomsuTas){
            var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].MeyveID;
            if (sagdakininRengi == renk && sagdakininRakami == MeyveID){
                await TasManeger.Instance.TasInstances[sagindakiKomsuTas].AyniRakamAyniRenkGrubunaDahilOl();
            }
        }
    }*/

    /*
    public async Task AyniRakamFarkliRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        bool gruptaAyniRenkYok = true;
        //grupta zaten aynı renk varsa per olmaz . Çık
        foreach (var gruptakiTas in Card.Instance.AyniMeyveFarkliRenkli){
            if (TasManeger.Instance.TasInstances[gruptakiTas].renk == renk){
                gruptaAyniRenkYok = false;
                break;
            }
        }

        if (gruptaAyniRenkYok){
            Card.Instance.AyniMeyveFarkliRenkli.Add(gameObject);
            if (sagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].MeyveID;
                if (sagdakininRengi != renk && sagdakininRakami == MeyveID){
                    await TasManeger.Instance.TasInstances[sagindakiKomsuTas].AyniRakamFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }
    */

    public void Sallan(){
        if (CompareTag("CARDTAKI_TAS")){
            tweener = MeyveResmiSpriteRenderer.transform.DOScale(.8f, .5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
        }
    }

    public void Sallanma(){ 
        if (tweener != null && tweener.IsActive()){
            tweener.Complete();
            tweener.Kill();
            tweener = null;
        }
        MeyveResmiSpriteRenderer.transform.localScale = orginalScale;
    }
 
    public void GoreveUygunsaKolonuIsaretle(){
        if (GorevleUyum == 0) return;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasscript = TasManeger.Instance.TasInstances[cTas];
            if (cTasscript.colID == cepInstance.colID){
                if (GorevleUyum == 1){
                    cTasscript.GorevUyumGostergesi1.gameObject.SetActive(true);
                }else if (GorevleUyum == 2){
                    cTasscript.GorevUyumGostergesi2.gameObject.SetActive(true);
                }
            }
        }
    }
}

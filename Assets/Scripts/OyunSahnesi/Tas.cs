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
    public int rakam;
    public Color renk;
    private float animasyonSuresi = 1f;
    public GameObject sagindakiKomsuTas;
    private Rigidbody2D _rigidbody;
    public bool birCardPerineDahil = false;
    public SpriteRenderer zeminSpriteRenderer;
    private Vector3 skorTxtPosition;
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
    public Dictionary<int, Tas> BonusOlarakEslesenTaslar = new Dictionary<int, Tas>();
    public bool NetworkDatayaEklendi = false;
    public TextMeshPro TextRakam;

    private void Awake(){
        zemin = transform.Find("Zemin").gameObject;
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        uiCamera = Camera.main;
        skorTxtPosition = new Vector3(0, 0, 0);
    }

    private void Start(){
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        zeminSpriteRenderer.color = renk;
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

        zeminlocalScale = zemin.transform.localScale*.25f;
        TextRakam.text = rakam.ToString();
        CreateBg();
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
        //Puanlama.Instance.KalanTasOraniTMP.text = kalanTas.ToString();
        OyunSahnesiUI.Instance.KalanTasSayisi.text = kalanTas.ToString();
    }

    public void BosCebeYerles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance.CepSayisi;
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++){
            var cepScript = Istaka.Instance.CepList[i];
            if (cepScript.Dolu == false){
                gameObject.transform.position += new Vector3(0, 0, -1);
                var hedef_cep_position = new Vector3(
                    cepScript.transform.position.x, 
                    cepScript.transform.position.y*.965f,
                    cepScript.transform.position.z);
                transform.DOMove(hedef_cep_position, animasyonSuresi * .5f)
                    .SetEase(Ease.OutExpo)
                    .OnComplete((() => {
                        transform.localScale = new Vector3(colonWidth, colonWidth) * 0.865f;
                        _audioSource_up.Play();
                    })); 
                Destroy(_rigidbody);
                Destroy(_collider); 
                cepScript.Dolu = true;
                cepScript.TasInstance = this;
                cepInstance = cepScript;
                PerIcinTasTavsiye.Instance.Sallanma();
                tag = "CEPTEKI_TAS";
                zemin.transform.localScale = zeminlocalScale;
                _audioSource_down.Play();
                TasManeger.Instance.PerleriKontrolEt();
                break;
            }
        }
    }

    public IEnumerator TaslarinRakaminiPuanaEkle(float gecikme){
        yield return new WaitForSeconds(gecikme);
        float sure = 1;
        //Puanlama.Instance.PerlerdenKazanilanPuan += rakam;  
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
            });
        foreach (var bonusTaslari in BonusOlarakEslesenTaslar){
            if (bonusTaslari.Value){
                bonusTaslari.Value.RakamiPuanaEkle();
            }
        }
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
            this.enabled = false;
        }
    }

    public async Task SiraliAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        Card.Instance.SiraliAyniRenkliGrup.Add(gameObject);
        if (sagindakiKomsuTas){
            var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam + 1){
                await TasManeger.Instance.TasInstances[sagindakiKomsuTas].SiraliAyniRenkGrubunaDahilOl();
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
            if (sagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam + 1){
                    await TasManeger.Instance.TasInstances[sagindakiKomsuTas].SiraliFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }

    public async Task AyniRakamAyniRenkGrubunaDahilOl(){
        if (birCardPerineDahil) return;
        Card.Instance.AyniRakamAyniRenkliGrup.Add(gameObject);
        if (sagindakiKomsuTas){
            var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
            var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].rakam;
            if (sagdakininRengi == renk && sagdakininRakami == rakam){
                await TasManeger.Instance.TasInstances[sagindakiKomsuTas].AyniRakamAyniRenkGrubunaDahilOl();
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
            if (sagindakiKomsuTas){
                var sagdakininRengi = TasManeger.Instance.TasInstances[sagindakiKomsuTas].renk;
                var sagdakininRakami = TasManeger.Instance.TasInstances[sagindakiKomsuTas].rakam;
                if (sagdakininRengi != renk && sagdakininRakami == rakam){
                    await TasManeger.Instance.TasInstances[sagindakiKomsuTas].AyniRakamFarkliRenkGrubunaDahilOl();
                }
            }
        }
    }

    public void Sallan(){
        if (CompareTag("CARDTAKI_TAS")){
            tweener = zemin.transform.DOScale(.275f, .35f)
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

    // kendi zemini hazirla
    public void CreateBg(){
        int width = 96*4;
        int height = 96*4;
        float radius = 30f; // yuvarlak köşe yarıçapı
        float shadowThickness = 0f; // gölge kalınlığı
        float shadowAlpha = 0.5f; // gölgenin maksimum alfa değeri (0 - 1)

        // Renkleri ayarlayın
        Color baseColor = renk; // varsayılan zemin rengi
        Color shadowColor = new Color(1, 1, 1, shadowAlpha);

        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                int index = x + y * width;

                // Köşe yuvarlaklığı kontrolü
                bool isRoundedCorner =
                    (x < radius && y < radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) > radius) ||
                    (x > width - radius && y < radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, radius)) > radius) ||
                    (x < radius && y > height - radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius)) > radius) ||
                    (x > width - radius && y > height - radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, height - radius)) > radius);

                if (isRoundedCorner){
                    pixels[index] = new Color(0, 0, 0, 0); // şeffaf köşe
                    continue;
                }

                // Alt ve sağ kenar için gölge hesaplama
                float shadowBlend = 0;

                if (y < shadowThickness)
                    shadowBlend = Mathf.Max(shadowBlend, 1 - (y / shadowThickness));

                if (x > width - shadowThickness)
                    shadowBlend = Mathf.Max(shadowBlend, (x - (width - shadowThickness)) / shadowThickness);

                if (shadowBlend > 0)
                    pixels[index] = Color.Lerp(baseColor, shadowColor, shadowBlend);
                else
                    pixels[index] = baseColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        zeminSpriteRenderer.sprite = sprite;
        zeminSpriteRenderer.transform.localScale *= .25f;
    }
}
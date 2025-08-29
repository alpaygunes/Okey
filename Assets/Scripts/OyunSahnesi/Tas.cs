using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class Tas : MonoBehaviour
{
    private Dictionary<int, Sprite[]> RenkliMeyvelerDic = new Dictionary<int, Sprite[]>();
    public int MeyveID;
    public int RenkID;
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
    public int gorevleUyumBayragi = 0;  
    public GameObject gorevUyumGostergesi1;
    public GameObject gorevUyumGostergesi2;
    public GameObject pereUyumluGostergesi; // cepteki daslar per halindeyse belirtir.
    public GameObject meyveResmi;
    public bool bonusBayragi; // Bu alan carttaki taslar için . ceptaki tas için degil
    public GameObject ptasIleUyumluGostergesi; // cardtaki taslar için 
    public GameObject persizIstakaTaslariGostergesi;
    public List<GameObject> ayniKolondakiAltinveElmasTaslar = new List<GameObject>();
    public bool tiklanaBilir = true;
    public Dictionary<int, GameObject> PerAilesi = null;
    public bool sallanmaDurumu = false;
    private bool cebeYerles = false;
    private Cep hedefCep;
    public bool kilitli = false;
    public Kutu kutuInstance = null; 
    private bool YokOlmayiBekliyor = false;

    private void Awake()
    {
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

        RenkliMeyvelerDic.Add(0, Resources.LoadAll<Sprite>("Images/Meyveler/0_kirmizi"));
        RenkliMeyvelerDic.Add(1, Resources.LoadAll<Sprite>("Images/Meyveler/1_turuncu"));
        RenkliMeyvelerDic.Add(2, Resources.LoadAll<Sprite>("Images/Meyveler/2_mavi"));
        RenkliMeyvelerDic.Add(3, Resources.LoadAll<Sprite>("Images/Meyveler/3_yesil"));
        RenkliMeyvelerDic.Add(4, Resources.LoadAll<Sprite>("Images/Meyveler/4_mor"));
        RenkliMeyvelerDic.Add(5, Resources.LoadAll<Sprite>("Images/Meyveler/5_pembe"));
        RenkliMeyvelerDic.Add(6, Resources.LoadAll<Sprite>("Images/Meyveler/6_kahve"));
        RenkliMeyvelerDic.Add(7, Resources.LoadAll<Sprite>("Images/Meyveler/7_mavimsi"));
        RenkliMeyvelerDic.Add(8, Resources.LoadAll<Sprite>("Images/Meyveler/8_turkuazimsi"));
        RenkliMeyvelerDic.Add(9, Resources.LoadAll<Sprite>("Images/Meyveler/9_yesilimsi"));
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        var Meyveler = RenkliMeyvelerDic[RenkID];
        Sprite sprite = Meyveler[MeyveID];
        MeyveResmiSpriteRenderer.sprite = sprite;
        MeyveResmiSpriteRenderer.transform.localScale *= 1.25f;
        TextMeyveID.text = MeyveID.ToString();
        orginalScale = MeyveResmiSpriteRenderer.transform.localScale;
        gorevUyumGostergesi1.SetActive(false);
        gorevUyumGostergesi2.SetActive(false);
        pereUyumluGostergesi.SetActive(false);
        ptasIleUyumluGostergesi.SetActive(false);
        persizIstakaTaslariGostergesi.SetActive(false);
    }

    private void OnDestroy()
    {
        try
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2);
            foreach (var collider in colliders)
            {
                if (collider.transform.CompareTag("SPAWN_HOLDER"))
                {
                    SpawnHole spawnHole = collider.GetComponent<SpawnHole>();
                    if (spawnHole != null)
                    {
                        spawnHole.musait = true;
                    }
                }
            }

            if (cepInstance)
                cepInstance.Dolu = false;

            // Sıradaki taslar
            var siradakiTaslar = TasManeger.Instance.TasList.Count;
            // Cardtaki TAslar
            var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
            // Perdeki Taslar
            var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
            int toplamTasSayisi = siradakiTaslar + carddakiTaslar.Length + perdekiTaslar.Length;
            TasManeger.Instance.TasInstances.Remove(gameObject);
            OyunSahnesiUI.Instance.KalanTasSayisi.text = (toplamTasSayisi - 1).ToString();
            PuanlamaIStatistikleri.ToplamTasSayisi = toplamTasSayisi;
        }
        catch (Exception e)
        {
            Debug.Log($" Tas.cs OnDestroy içinde HATA : \n {e.Message}");
        }
    }

    public bool BosCebeYerles()
    {
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++)
        {
            hedefCep = Istaka.Instance.CepList[i];
            if (hedefCep.Dolu == false)
            {
                hedefCep.Dolu = true;
                hedefCep.TasInstance = this;
                cepInstance = hedefCep;
                sallanmaDurumu = false;
                cebeYerles = true; 
                tag = "CEPTEKI_TAS";
                StartCoroutine(RigidbodyVeCollideriSilGecikmeli());
                return true;
            }
        }

        return false;
    }

    IEnumerator RigidbodyVeCollideriSilGecikmeli()
    {
        yield return new WaitForFixedUpdate(); // 1 fizik frame bekle 
        Destroy(_rigidbody);
        PerIcinUygunTaslariBelirt.Bul();
    }

    private void FixedUpdate()
    {
        if (cebeYerles)
        {
            _rigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            Destroy(_collider);
            Vector2 cardSize = Card.Instance.Size;
            float colonWidth = cardSize.x / GameManager.Instance.cepSayisi;
            if (colonWidth > cardSize.x / 6)
            {
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

    public IEnumerator BekleYokol(float gecikme)
    { 
        if (kutuInstance && kutuInstance.KilitSayisi > 0 && !YokOlmayiBekliyor)
        { 
            YokOlmayiBekliyor = true;
            kutuInstance.KilitSayisi--;
            if (kutuInstance.KilitSayisi <= 0)
            {
                kutuInstance.transform.Find("KilitBelirteci").gameObject.SetActive(false);
                kilitli = false;
            } 
            tiklanaBilir = true;
            kutuInstance.Kilitlen();
            yield break;
        }

        if (kutuInstance)
        {
            gecikme = 0;
        }

        yield return new WaitForSeconds(gecikme);
        if (this == null) yield break;
        if (cepInstance != null) cepInstance.TasInstance = null;
        if (gameObject != null)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (sallanmaDurumu && !tweener.IsActive() && tweener == null)
        {
            if (sallanmaCoroutine == null)
            {
                sallanmaCoroutine = StartCoroutine(BekleSallan());
            }
        }

        if (!sallanmaDurumu && tweener.IsActive())
        {
            tweener.Complete();
            tweener.Kill();
            tweener = null;
            MeyveResmiSpriteRenderer.transform.localScale = orginalScale;
            if (sallanmaCoroutine is not null)
            {
                StopCoroutine(sallanmaCoroutine);
                sallanmaCoroutine = null;
            }
        }
    }

    private IEnumerator BekleSallan()
    {
        if (kilitli) yield break;
        yield return new WaitForSeconds(sallanmakIcinBeklemeSuresi);
        if (sallanmaDurumu && !tweener.IsActive() && tweener == null)
        {
            tweener = MeyveResmiSpriteRenderer.transform.DOScale(.8f, .5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
        }
    }

    public void AltinVeElmasGoster()
    {
        if (gorevleUyumBayragi == 0) return;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar)
        {
            var cTasscript = TasManeger.Instance.TasInstances[cTas];
            if (cTasscript.colID == cepInstance.colID)
            { 
                if (gorevleUyumBayragi == 1)
                {
                    cTasscript.gorevUyumGostergesi1.gameObject.SetActive(true);
                    cTasscript.meyveResmi.gameObject.SetActive(false);
                    tiklanaBilir = false;
                }
                else if (gorevleUyumBayragi == 2)
                {
                    cTasscript.gorevUyumGostergesi2.gameObject.SetActive(true);
                    cTasscript.meyveResmi.gameObject.SetActive(false);
                    tiklanaBilir = false;
                }

                if (cTasscript.kilitli)
                {
                    cTasscript.meyveResmi.gameObject.SetActive(true);
                    cTasscript.gorevUyumGostergesi1.gameObject.SetActive(false);
                    cTasscript.gorevUyumGostergesi2.gameObject.SetActive(false);
                } 

                ayniKolondakiAltinveElmasTaslar.Add(cTas);
            }
        }
    }
}
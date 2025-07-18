using NUnit.Framework.Constraints;
using UnityEngine;

public class Kutu : MonoBehaviour {
    public Vector2Int colRowPosition = Vector2Int.zero; 
    private const float MinSpeedEpsilon = 0.0000001f;

    public bool isStoper = false;
    private bool isStatik = true;
    public bool odul_kutusu = false;

    // Sensör olarak kullanılacak child collider
    private CircleCollider2D _sensor;
    public int KilitSayisi = 0;
    private bool kilitlendi = false;

    void Start() {
        // 1) Sensörü bul
        if (odul_kutusu)
            _sensor = transform.Find("OdulBelirteci")?.GetComponent<CircleCollider2D>();
        if (isStoper)
            _sensor = transform.Find("IsStaticBelirteci")?.GetComponent<CircleCollider2D>();
        if (KilitSayisi>0)
            _sensor = transform.Find("KilitBelirteci")?.GetComponent<CircleCollider2D>();

        if (_sensor != null) {
            // 2) Sensörü tetikleyici yap
            _sensor.isTrigger = true;

            // 3) Olayların bu script’e ulaşması için kök objede Rigidbody2D olsun
            if (GetComponent<Rigidbody2D>() == null) {
                var rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = true;
            }
        }

        if (KilitSayisi>0) {
            transform.Find("KilitBelirteci").gameObject.SetActive(true);
            Kilitlen();
        }
    }

    public void Kilitlen() {
        var kilit1 = transform.Find("KilitBelirteci/1"); 
        var kilit2 = transform.Find("KilitBelirteci/2");
        kilit1?.gameObject.SetActive(KilitSayisi == 1);
        kilit2?.gameObject.SetActive(KilitSayisi == 2);
    }

    // ===       Fizik Olayları     =======
    private void OnTriggerEnter2D(Collider2D other) {
        // Sadece Rigidbody2D taşıyan nesneler
        if (other.attachedRigidbody == null) return;
        if (isStoper) isStatik = false;
    }


    // Update is called once per frame
    void Update() {
        if (isStoper && !isStatik) {
            GameObject temasEden = null;
            if (_sensor) {
                temasEden = StatikKutuyaTemasEdenMeyve();
                if (temasEden) {
                    var rb = temasEden.GetComponent<Rigidbody2D>();
                    if (rb && rb.linearVelocity.sqrMagnitude < MinSpeedEpsilon) {
                        rb.bodyType = RigidbodyType2D.Static;
                        isStatik = true;
                    }
                }
            } 
        }
        
        if (KilitSayisi>0 && !kilitlendi) {
            GameObject temasEden = null;
            if (_sensor) {
                temasEden = KilitliKutuyaTemasEdenMeyve();
                if (temasEden) {
                    var rb = temasEden.GetComponent<Rigidbody2D>();
                    if (rb && rb.linearVelocity.sqrMagnitude < MinSpeedEpsilon) {
                        rb.bodyType = RigidbodyType2D.Static;
                        TasManeger.Instance.TasInstances[temasEden].kilitli = true;
                        TasManeger.Instance.TasInstances[temasEden].kutuInstance = this; 
                        kilitlendi = true;
                    }
                }
            } 
        }
    }

    public GameObject OdulKutusunaTemasEdenMeyve() {
        // 1) Collider referansı
        var belirteci = transform.Find("OdulBelirteci");
        if (belirteci == null) return null;

        var col = belirteci.GetComponent<CircleCollider2D>();
        if (col == null) return null;

        // 2) Gerekirse Kinematic Rigidbody ekle
        if (col.attachedRigidbody == null) {
            var rb = belirteci.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3) Filtre – tetikleyicileri de dahil et
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            layerMask = Physics2D.DefaultRaycastLayers,
            useLayerMask = true
        };

        // 4) Sonuçları tutacak dizi
        Collider2D[] hits = new Collider2D[1];

        // 5) Temas kontrolü
        if (col.Overlap(filter, hits) > 0 && hits[0] != null)
            return hits[0].gameObject;

        return null;
    }
    
    public GameObject StatikKutuyaTemasEdenMeyve() {
        // 1) Collider referansı
        var belirteci = transform.Find("IsStaticBelirteci");
        if (belirteci == null) return null;

        var col = belirteci.GetComponent<CircleCollider2D>();
        if (col == null) return null;

        // 2) Gerekirse Kinematic Rigidbody ekle
        if (col.attachedRigidbody == null) {
            var rb = belirteci.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3) Filtre – tetikleyicileri de dahil et
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            layerMask = Physics2D.DefaultRaycastLayers,
            useLayerMask = true
        };

        // 4) Sonuçları tutacak dizi
        Collider2D[] hits = new Collider2D[1];

        // 5) Temas kontrolü
        if (col.Overlap(filter, hits) > 0 && hits[0] != null)
            return hits[0].gameObject;

        return null;
    }
    
    public GameObject KilitliKutuyaTemasEdenMeyve() {
        // 1) Collider referansı
        var belirteci = transform.Find("KilitBelirteci");
        if (belirteci == null) return null;

        var col = belirteci.GetComponent<CircleCollider2D>();
        if (col == null) return null;

        // 2) Gerekirse Kinematic Rigidbody ekle
        if (col.attachedRigidbody == null) {
            var rb = belirteci.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3) Filtre – tetikleyicileri de dahil et
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            layerMask = Physics2D.DefaultRaycastLayers,
            useLayerMask = true
        };

        // 4) Sonuçları tutacak dizi
        Collider2D[] hits = new Collider2D[1];

        // 5) Temas kontrolü
        if (col.Overlap(filter, hits) > 0 && hits[0] != null)
            return hits[0].gameObject;

        return null;
    }
}
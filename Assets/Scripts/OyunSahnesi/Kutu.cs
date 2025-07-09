using UnityEngine;

public class Kutu : MonoBehaviour
{
    private const float MinSpeedEpsilon = 0.0000001f;

    public bool tasiStatikYap;

    // Sensör olarak kullanılacak child collider
    private CircleCollider2D _sensor;

    void Awake()
    {
        // 1) Sensörü bul
        _sensor = transform.Find("OdulBelirteci")?.GetComponent<CircleCollider2D>();
        if (_sensor == null)
        {
            Debug.LogError("OdulBelirteci veya CircleCollider2D bulunamadı!");
            return;
        }

        // 2) Sensörü tetikleyici yap
        _sensor.isTrigger = true;

        // 3) Olayların bu script’e ulaşması için kök objede Rigidbody2D olsun
        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    // === Fizik Olayları =====================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece Rigidbody2D taşıyan nesneler
        if (other.attachedRigidbody == null) return;
        tasiStatikYap = true;
    }
 
    

    // Update is called once per frame
    void Update() {   
        if (tasiStatikYap) {
            var kutuylaOrtusenTas = GetKutuyaTemasEden();
            if (kutuylaOrtusenTas) {
                var rb = kutuylaOrtusenTas.GetComponent<Rigidbody2D>();
                if (rb != null && rb.linearVelocity.sqrMagnitude < MinSpeedEpsilon) {
                    rb.bodyType = RigidbodyType2D.Static;
                    tasiStatikYap = false; 
                }
            }
        }
    }

    public GameObject GetKutuyaTemasEden() {
        // 1) Collider referansı
        var belirteci = transform .Find("OdulBelirteci");
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
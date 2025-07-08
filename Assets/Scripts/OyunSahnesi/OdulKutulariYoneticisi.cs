using System.Collections.Generic;
using UnityEngine;

public static class OdulKutulariYoneticisi {
    private static int _kutuSayisi = 2;

    // Ödül atanmış kutular bu listede tutulacak
    private static readonly List<GameObject> _odulluKutular = new();

    public static void OdulKutulariniBelirle() {
        _odulluKutular.Clear();

        // Sahnedeki tüm "KUTU" objelerini çek
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;

        // Kutu sayısı sahnedeki toplamdan büyükse hepsini al
        if (_kutuSayisi >= tumKutular.Count) {
            _odulluKutular.AddRange(tumKutular);
            return;
        }

        // Rastgele _kutuSayisi adet kutu seç
        for (int i = 0; i < _kutuSayisi; i++) {
            int rndIndex = Random.Range(0, tumKutular.Count);
            _odulluKutular.Add(tumKutular[rndIndex]);
            tumKutular[rndIndex].transform.Find("OdulBelirteci").gameObject.SetActive(true);
            tumKutular.RemoveAt(rndIndex); // Aynı kutunun tekrar seçilmesini engelle
        }
    }

    public static void EslesmeVarmi() {
        foreach (var per in PerKontrolBirimi.Instance.Gruplar) {
            foreach (var pTas in per.Value.Taslar) {
                for (var k = 0; k < _odulluKutular.Count; k++) {
                    var kutu = _odulluKutular[k];
                    var kutuylaOrtusenTas = GetKutuyaTemasEden(kutu);
                    if (kutuylaOrtusenTas) {
                        if (TasManeger.Instance.TasInstances[kutuylaOrtusenTas].MeyveID == pTas.MeyveID
                            && TasManeger.Instance.TasInstances[kutuylaOrtusenTas].Renk == pTas.Renk) {
                            //TODO EŞLEŞEN ÖDÜL KUTUSUNDAN KARTTAKİ TAŞLARA ŞİMŞEKLER ÇAKARAK TAŞLARI YOK EDECEK
                            Debug.Log("Eslesme var.");
                            pTas.sallanmaDurumu = true;
                            TasManeger.Instance.TasInstances[kutuylaOrtusenTas].sallanmaDurumu = true;
                        }
                    }
                }
            }
        }
    }

    private static GameObject GetKutuyaTemasEden(GameObject kutu)
    {
        // 1) Collider referansı
        var belirteç = kutu.transform.Find("OdulBelirteci");
        if (belirteç == null) return null;

        var col = belirteç.GetComponent<CircleCollider2D>();
        if (col == null) return null;

        // 2) Gerekirse Kinematic Rigidbody ekle
        if (col.attachedRigidbody == null)
        {
            var rb = belirteç.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3) Filtre – tetikleyicileri de dahil et
        var filter = new ContactFilter2D
        {
            useTriggers = true,
            layerMask   = Physics2D.DefaultRaycastLayers,
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
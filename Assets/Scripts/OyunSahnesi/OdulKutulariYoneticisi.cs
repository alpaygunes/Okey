using System.Collections.Generic;
using UnityEngine;

public static class OdulKutulariYoneticisi {
    private static int _kutuSayisi = 2;

    // Ödül atanmış kutular bu listede tutulacak
    private static readonly List<GameObject> OdulluKutular = new();

    public static void OdulKutulariniBelirle() { 
        OdulluKutular.Clear();
        if (!GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")].OdulKutus) return;

        // Sahnedeki tüm "KUTU" objelerini çek
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;

        // Kutu sayısı sahnedeki toplamdan büyükse hepsini al
        if (_kutuSayisi >= tumKutular.Count) {
            OdulluKutular.AddRange(tumKutular);
            return;
        }

        // Rastgele _kutuSayisi adet kutu seç
        for (int i = 0; i < _kutuSayisi; i++) {
            int rndIndex = Random.Range(0, tumKutular.Count);
            var kutu = tumKutular[rndIndex];
            var belirtec = kutu.transform.Find("OdulBelirteci").gameObject;
            var kutuScript = kutu.GetComponent<Kutu>();
            if (kutuScript.isStoper || kutuScript.KilitSayisi>0) continue; 
            OdulluKutular.Add(kutu);
            belirtec.gameObject.SetActive(true);
            kutu.GetComponent<Kutu>().odul_kutusu = true;
            tumKutular.RemoveAt(rndIndex); // Aynı kutunun tekrar seçilmesini engelle
        }
    }

    public static void EslesmeVarmi() {
        foreach (var per in PerKontrolBirimi.Instance.Gruplar) {
            foreach (var pTas in per.Value.Taslar) {
                for (var k = 0; k < OdulluKutular.Count; k++) {
                    var kutu = OdulluKutular[k];
                    var kutuscript = kutu.GetComponent<Kutu>();
                    var kutuylaOrtusenTas = kutuscript.OdulKutusunaTemasEdenMeyve();
                    if (kutuylaOrtusenTas) {
                        if (TasManeger.Instance.TasInstances[kutuylaOrtusenTas].MeyveID == pTas.MeyveID
                            && TasManeger.Instance.TasInstances[kutuylaOrtusenTas].RenkID == pTas.RenkID) {
                            //TODO EŞLEŞEN ÖDÜL KUTUSUNDAN KARTTAKİ TAŞLARA ŞİMŞEKLER ÇAKARAK TAŞLARI YOK EDECEK
                            Debug.Log("Eslesme var."); 
                        }
                    }
                }
            }
        }
    } 
}
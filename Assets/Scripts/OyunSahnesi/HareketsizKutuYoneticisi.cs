using System.Collections.Generic;
using UnityEngine;

public static class HareketsizKutuYoneticisi
{
    private static int _kutuSayisi = 2;

    public static void HareketsizKutulariBelirle() {
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;
        // Rastgele _kutuSayisi adet kutu seç
        for (int i = 0; i < _kutuSayisi; i++) {
            int rndIndex = Random.Range(0, tumKutular.Count);
            var kutu = tumKutular[rndIndex];
            var belirtec = kutu.transform.Find("OdulBelirteci").gameObject;
            if (belirtec.activeSelf) continue;
            belirtec.SetActive(true);
            belirtec.GetComponent<SpriteRenderer>().color = Color.grey;
            kutu.GetComponent<Kutu>().tasiStatikYap = true;
            tumKutular.RemoveAt(rndIndex);
        }
    }
 
}

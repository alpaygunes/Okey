using System.Collections.Generic;
using UnityEngine;

public static class HareketsizKutuYoneticisi
{
    private static int _kutuSayisi = 2;

    public static void HareketsizKutulariBelirle() { 
        if (!GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")].HareketsizKutu) return;
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;
        // Rastgele _kutuSayisi adet kutu seç
        for (int i = 0; i < _kutuSayisi; i++) {
            int rndIndex = Random.Range(0, tumKutular.Count);
            var kutu = tumKutular[rndIndex];
            var belirtec = kutu.transform.Find("IsStaticBelirteci").gameObject;
            var kutuScript = kutu.GetComponent<Kutu>();
            if (kutuScript.odul_kutusu || kutuScript.KilitSayisi > 0) continue; 
            belirtec.SetActive(true); 
            kutuScript.isStoper = true;
            tumKutular.RemoveAt(rndIndex);
        }
    }
 
}

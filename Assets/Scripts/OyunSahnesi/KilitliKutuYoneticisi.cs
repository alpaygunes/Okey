using System.Collections.Generic;
using UnityEngine;

public static class KilitliKutuYoneticisi {
    public static void KilitliKutulariBelirle()
    {
        var level = GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")];
        if (!level.KilitliKutu) return; 
        var oynananKalipNo = SoloLevelManager.Instance.GetLevelVerisi(PlayerPrefs.GetInt("OynananLevelID")).KalipSirasi;
        var kalip = level.Kalip[oynananKalipNo];
        var sablon = Pattern.getCharMatris(kalip);
        Isaretle(sablon);
    }

    private static void Isaretle(int[,] sablon) {
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;

        int matrisSatirSayisi = sablon.GetLength(0); 
        int matRisSutunSayisi = sablon.GetLength(1); 

        for (int i = 0; i < matrisSatirSayisi; i++) {
            for (int j = 0; j < matRisSutunSayisi; j++) { 
                for (int k = 0; k < tumKutular.Count; k++) { 
                    if (sablon[i, j] == 1) {
                        var kutuScript = tumKutular[k].GetComponent<Kutu>(); 
                        var kutujI = i + Card.Instance.SatirSayisi - matrisSatirSayisi; 
                        if (kutuScript.colRowPosition.x == j 
                            && kutuScript.colRowPosition.y == kutujI) { 
                            kutuScript.KilitSayisi = 4; // 0 olmamalı. 0 kilitsiz demek
                        } 
                    }
                }
            }
        } 
    }
}
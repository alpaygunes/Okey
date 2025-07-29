using System.Collections.Generic;
using UnityEngine;

public static class KilitliKutuYoneticisi {
    public static void KilitliKutulariBelirle() {  
        if (!GameLevels.GetLevel().KilitliKutu) return;
        var OynananKalipNo = PlayerPrefs.GetInt("OynananKalipNo");
        var kalip = GameLevels.GetLevel().Kalip[OynananKalipNo];
        var sablon = Pattern.getCharMatris(kalip);
        Isaretle(sablon);
        //var (_, matrix) = Pattern.getRandom();
        //Isaretle(matrix);
        
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
                            kutuScript.KilitSayisi = 2; // 0 olmamalı. 0 kilitsiz demek
                        } 
                    }
                }
            }
        } 
    }
}
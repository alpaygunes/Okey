using System.Collections.Generic;
using UnityEngine;

public static class KilitliKutuYoneticisi {
    public static void KilitliKutulariBelirle() {
        int[,] sablon = new int[,]
        {
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 }
        };
        Isaretle(sablon);
    }

    private static void Isaretle(int[,] sablon) {
        var tumKutular = new List<GameObject>(GameObject.FindGameObjectsWithTag("KUTU"));
        if (tumKutular.Count == 0) return;

        int matrisSatirSayisi = sablon.GetLength(0); // 3
        int matRisSutunSayisi = sablon.GetLength(1); // 5

        for (int i = 0; i < matrisSatirSayisi; i++) {
            for (int j = 0; j < matRisSutunSayisi; j++) { 
                for (int k = 0; k < tumKutular.Count; k++) { 
                    if (sablon[i, j] == 1) {
                        var kutuScript = tumKutular[k].GetComponent<Kutu>();
                        if (kutuScript.colRowPosition.x == j 
                            && kutuScript.colRowPosition.y == i) { 
                            kutuScript.KilitSayisi = 2; // 0 olmamalı. 0 kilitsiz demek
                        } 
                    }
                }
            }
        } 
    }
}
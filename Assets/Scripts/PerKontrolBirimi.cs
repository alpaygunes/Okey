using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerKontrolBirimi : MonoBehaviour
{
    public static PerKontrolBirimi Instance;
    public int SiradakiCepNum = 0;
    Dictionary<int,List<Tas>> Pers  = new Dictionary<int,List<Tas>>();
    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        } 
        Instance = this;
    }

    public void PerleriBul(){
        Pers.Clear();
        var ceps = Istaka.Instance.CepList;
        for (int i = 0; i+2 < ceps.Count; i++){
            var perAdayGrubu = new List<Tas>();
            var cep = ceps[i];
            if (cep.TasInstance == null) continue;
            if (ceps[i].TasInstance is not null) perAdayGrubu.Add(ceps[i].TasInstance); 
            if (ceps[i+1].TasInstance is not null) perAdayGrubu.Add(ceps[i+1].TasInstance); 
            if (ceps[i+2].TasInstance is not null) perAdayGrubu.Add(ceps[i+2].TasInstance);
            if (perAdayGrubu.Count < 3) continue;
            SiradakiCepNum = 2 + i ; // i dahil üç cep
            while (GrupPermi(perAdayGrubu)){ 
                var kontroldenGecmisPerAdayGrubu = new List<Tas>(perAdayGrubu);
                if(!Pers.TryAdd(kontroldenGecmisPerAdayGrubu[0].cepInstance.colID, kontroldenGecmisPerAdayGrubu))
                    Pers[kontroldenGecmisPerAdayGrubu[0].cepInstance.colID] = kontroldenGecmisPerAdayGrubu;
                SiradakiCepNum++;
                if (SiradakiCepNum > ceps.Count-1) break;
                if (ceps[SiradakiCepNum].TasInstance == null) break; 
                perAdayGrubu.Add(ceps[SiradakiCepNum].TasInstance);
            }
        }

        foreach (var per in Pers){
            Debug.Log($" perAdayGrubu   {per.Value.Count} Cep ID {per.Value.First().cepInstance.colID}");
        }
    }
    
    private bool GrupPermi(List<Tas> pAdayGrubu){ 
        var cloneperAdayGrubu = new List<Tas>(pAdayGrubu);
        bool RA = true; // renkler ayni
        bool RF = true; // renkler hepsi bir birinden farkli
        bool MA = true; // meyveler ayni
        bool MF = true; // meyveler hepsi bir birinden farklı
        // RA kontrolu
        for (int i = 0; i < pAdayGrubu.Count; i++){
            var t = pAdayGrubu[i];
            for (int j = 0; j < cloneperAdayGrubu.Count; j++){
                if (j == cloneperAdayGrubu.Count-1) break;
                var st = cloneperAdayGrubu[j];
                if (i == j) continue;
                if (t.Renk != st.Renk){
                    RA = false;
                    break;
                }
            }
        }
        
        // RF kontrolu
        for (int r = 0; r < pAdayGrubu.Count; r++){
            var t = pAdayGrubu[r];
            for (int rr = 0; rr < cloneperAdayGrubu.Count; rr++){
                if (rr == cloneperAdayGrubu.Count-1) break;
                var st = cloneperAdayGrubu[rr];
                if (r == rr) continue;
                if (t.Renk == st.Renk){
                    RF = false;
                    break;
                }
            }
        }
        
        // MA kontrolu
        for (int ii = 0; ii < pAdayGrubu.Count; ii++){  
            var t = pAdayGrubu[ii];
            for (int jj = 0; jj < cloneperAdayGrubu.Count; jj++){
                if (jj == cloneperAdayGrubu.Count-1) break;
                var st = cloneperAdayGrubu[jj]; 
                if (ii == jj) continue;
                if (t.MeyveID != st.MeyveID){
                    MA = false;
                    break;
                }
            }
        }
        
        // MF kontrolu
        for (int iii = 0; iii < pAdayGrubu.Count; iii++){  
            var t = pAdayGrubu[iii];
            for (int jjj = 0; jjj < cloneperAdayGrubu.Count; jjj++){
                if (jjj == cloneperAdayGrubu.Count-1) break;
                var st = cloneperAdayGrubu[jjj];
                if (iii == jjj) continue;
                if (t.MeyveID == st.MeyveID){
                    MF = false;
                    break;
                } 
            }  
        }
        
        //Debug.Log($"RA {RA}, MA {MA},MF {MF},RF {RF},");
        if (RA && MA ) return true;
        if (RA && MF ) return true;
        if (RF && MA ) return true;
        
        // per değilse eklenen son taşı çıkar
        
        return false; 
    }
}

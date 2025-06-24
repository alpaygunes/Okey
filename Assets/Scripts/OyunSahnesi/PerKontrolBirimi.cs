using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grup{
    public List<Tas> Taslar{ get; set; }
    public string GrupTuru{ get; set; }
}


public class PerKontrolBirimi : MonoBehaviour{
    public static PerKontrolBirimi Instance;
    public int siradakiCepNum = 0;
    public Dictionary<int, Grup> Gruplar;
    private readonly List<Color> grupRenkleri = new List<Color>() { Color.red, Color.yellow, Color.blue };
    private List<Cep> ceps;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Gruplar = new Dictionary<int, Grup>();
    }

    public void ParseEt(List<Cep> CepList){
        ceps = CepList;
        GruplariBul();
        KesisenGruplardanKucuguSil();
    }

    public void PerdekiTaslariBelirt(){
        // önceki belirteçleri kaldır. per düzeni değişmiştir belki
        for (int i = 0; i < Istaka.Instance.CepList.Count; i++){
            var cep = Istaka.Instance.CepList[i];
            if (!cep.TasInstance) continue;
            Istaka.Instance.CepList[i].TasInstance.PereUyumluGostergesi.SetActive(false);
            Istaka.Instance.CepList[i].TasInstance.PereUyumluGostergesi.GetComponent<SpriteRenderer>().color =
                Color.white;
        }

        try{
            var i = 0;
            foreach (var grup in Gruplar){
                foreach (var tas in grup.Value.Taslar){
                    tas.PereUyumluGostergesi.SetActive(true);
                    tas.PereUyumluGostergesi.GetComponent<SpriteRenderer>().color = grupRenkleri[i];
                } 
                i++;
            }
        }
        catch (Exception e){
            Console.WriteLine($"PerdekiTasiBelirt {e.Message}");
        }
    }

    public void GruplariBul(){
        try{
            Gruplar = new Dictionary<int, Grup>();
            for (int i = 0; i + 2 < ceps.Count; i++){
                var perAdayTasGrubu = new List<Tas>();
                var cep = ceps[i];
                if (cep.TasInstance == null) continue;
                if (ceps[i].TasInstance is not null) perAdayTasGrubu.Add(ceps[i].TasInstance);
                if (ceps[i + 1].TasInstance is not null) perAdayTasGrubu.Add(ceps[i + 1].TasInstance);
                if (ceps[i + 2].TasInstance is not null) perAdayTasGrubu.Add(ceps[i + 2].TasInstance);
                if (perAdayTasGrubu.Count < 3) continue;
                siradakiCepNum = 2 + i; // i dahil üç cep 
                while (GrupPermi(perAdayTasGrubu) is { } grupTuru){
                    var kontroldenGecmisPerAdayGrubu = new List<Tas>(perAdayTasGrubu);
                    var yeniGrup = new Grup();
                    yeniGrup.Taslar = kontroldenGecmisPerAdayGrubu;
                    yeniGrup.GrupTuru = grupTuru;
                    int colID = kontroldenGecmisPerAdayGrubu.First().cepInstance.colID;

                    if (!Gruplar.TryAdd(colID, yeniGrup)){
                        Gruplar[colID] = yeniGrup;
                    }

                    siradakiCepNum++;
                    if (siradakiCepNum > ceps.Count - 1) break;
                    if (!ceps[siradakiCepNum].TasInstance) break;
                    perAdayTasGrubu.Add(ceps[siradakiCepNum].TasInstance);
                }
            }
        }
        catch (Exception e){
            Debug.Log(e.Message);
        }
    }

    private string GrupPermi(List<Tas> pAdayGrubu){
        var cloneperAdayGrubu = new List<Tas>(pAdayGrubu);
        bool RA = true; // renkler ayni
        bool RF = true; // renkler hepsi bir birinden farkli
        bool MA = true; // meyveler ayni
        bool MF = true; // meyveler hepsi bir birinden farklı
        string grupTuru = null;
        // RA kontrolu
        for (int i = 0; i < pAdayGrubu.Count; i++){
            var t = pAdayGrubu[i];
            for (int j = 0; j < cloneperAdayGrubu.Count; j++){
                if (j == cloneperAdayGrubu.Count - 1) break;
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
                if (rr == cloneperAdayGrubu.Count - 1) break;
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
                if (jj == cloneperAdayGrubu.Count - 1) break;
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
                if (jjj == cloneperAdayGrubu.Count - 1) break;
                var st = cloneperAdayGrubu[jjj];
                if (iii == jjj) continue;
                if (t.MeyveID == st.MeyveID){
                    MF = false;
                    break;
                }
            }
        }

        //Debug.Log($"RA {RA}, MA {MA},MF {MF},RF {RF},");
        if (RA && MA){
            grupTuru = "rama";
        }
        else if (RA && MF){
            grupTuru = "ramf";
        }
        else if (RF && MA){
            grupTuru = "rfma";
        }

        return grupTuru;
    }
    
    public void KesisenGruplardanKucuguSil(){
        int silinecekGrupKey = -1;
        // kesişen grup var mı ?
        foreach (var grup0 in Gruplar){
            var key0 = grup0.Key;
            var last0 = key0 + grup0.Value.Taslar.Count;
            foreach (var grup1 in Gruplar){
                var key1 = grup1.Key;
                if (key0 < key1 && key1 < last0){
                    silinecekGrupKey = (grup0.Value.Taslar.Count >= grup1.Value.Taslar.Count) ? key1 : key0;
                    break;
                }
            } 
            if (silinecekGrupKey >= 0) break;
        }

        if (silinecekGrupKey >= 0){
            var cloneGruplar = new Dictionary<int, Grup>(Gruplar);
            foreach (var grup in cloneGruplar){
                if (grup.Key == silinecekGrupKey){
                    Gruplar.Remove(silinecekGrupKey);
                }
            } 
            KesisenGruplardanKucuguSil();
        }
    }
}
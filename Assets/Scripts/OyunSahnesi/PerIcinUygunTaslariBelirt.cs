using System;
using System.Collections.Generic; 
using UnityEngine; 

public static class PerIcinUygunTaslariBelirt{
    public static Dictionary<int, Grup> MevcutGruplar{ get; set; }
    public static Dictionary<int, Grup> SanalGruplar{ get; set; }

    public static void Bul(){
        MevcutGruplar = PerKontrolBirimi.Instance.Gruplar;
        var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        List<Cep> copyCeplist = new List<Cep>();
        var cepPrefab = Resources.Load<GameObject>("Prefabs/IstakaCebi");

        foreach (var cep in Istaka.Instance.CepList) {
            GameObject copyCepGO = UnityEngine.Object.Instantiate(cepPrefab); 
            copyCepGO.SetActive(false);                        // Kameraya çizilmez 
            Cep copyCep          = copyCepGO.GetComponent<Cep>();
            copyCep.colID        = cep.colID; 
            if (cep.TasInstance is not Tas) cep.TasInstance = null;
            copyCep.TasInstance  = cep.TasInstance;
            copyCeplist.Add(copyCep);
        }

        foreach (var cTas in carddakiTaslar){
            TasManeger.Instance.TasInstances[cTas].sallanmaDurumu = false;
        }

        //ilk boş ceblere sanal olarak cardtaki taslari yerleştirelim ve mevcutper değiştimi kontrole delim 
        foreach (var copyCep in copyCeplist){
            try{
                if (copyCep.TasInstance) continue;
                foreach (var cTas in carddakiTaslar){
                    copyCep.TasInstance = TasManeger.Instance.TasInstances[cTas];
                    PerKontrolBirimi.Instance.ParseEt(copyCeplist);
                    SanalGruplar = PerKontrolBirimi.Instance.Gruplar;
                    // burada SanalGruplar ile MevcutGruplar karşılaştıracak. farklı oalnlar sallanacak
                    var tumMevcutGruplardakiTaslar = new List<Tas>();
                    foreach (var mGrup in MevcutGruplar){
                        tumMevcutGruplardakiTaslar.AddRange(mGrup.Value.Taslar); 
                    }

                    var tumSanalGruplardakiTaslar = new List<Tas>();
                    foreach (var sGrup in SanalGruplar){
                        tumSanalGruplardakiTaslar.AddRange(sGrup.Value.Taslar); 
                    }

                    foreach (var sTas in tumSanalGruplardakiTaslar){
                        bool tasZatenMevcut = false;
                        foreach (var mTas in tumMevcutGruplardakiTaslar){
                            if (sTas == mTas){
                                tasZatenMevcut = true;
                                break;
                            }
                        }

                        if (tasZatenMevcut == false){
                            if (sTas.CompareTag("CARDTAKI_TAS")){
                                sTas.sallanmaDurumu = true;
                            }
                        }
                    } 
                    //----
                    copyCep.TasInstance = null;
                }
            }
            catch (Exception e){
                Console.WriteLine(e); 
            }
        } 
        
        
        // ➋  – KULLANIM BİTİNCE HEPSİNİ TEMİZLE
        foreach (var copyCep in copyCeplist){
            UnityEngine.Object.Destroy(copyCep.gameObject);
        }

    }  
    
}
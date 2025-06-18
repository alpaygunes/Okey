using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerIcinTasTavsiye : MonoBehaviour{
    
    public static PerIcinTasTavsiye Instance;
    private  Cep _istakadakiIlkBosCep;
    private  GameObject[] carddakiTaslar;
    private  List<List<Cep>> ucluCepler = new List<List<Cep>>();
    private  Coroutine _countdownCoroutine;
    private  float PerTavsiyesiIcinGeriSayimSuresi{ get; set; } = 2.5f;

    private static Dictionary<int, Dictionary<string, object>> AranacakKriterler = new Dictionary<int, Dictionary<string, object>>();
    
    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this; 
    }
    
    public  void Basla(){
        if (_countdownCoroutine != null){
            StopCoroutine(_countdownCoroutine);
        }
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private  IEnumerator CountdownRoutine(){
        float _timeLeft = PerTavsiyesiIcinGeriSayimSuresi;
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--;
        }
        //if (GameManager.Instance.OyunDurumu == GameManager.OynanmaDurumu.DevamEdiyor)
            TasBul();
    }
    
    private  void TasBul(){ 
        _istakadakiIlkBosCep = IstakaKontrol();
        PerAdaylariniBelirle(_istakadakiIlkBosCep);
        PerAdayiKurallariniBelirle();
        KurallarauygunIlkTasiBul();
    }
    
    private  Cep IstakaKontrol(){
        carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS"); 
        for (int i = 0; i < Istaka.Instance.CepList.Count; i++){
            if (Istaka.Instance.CepList[i].TasInstance == null){
                return Istaka.Instance.CepList[i];
            }
        }
        return null;
    }

    private void PerAdaylariniBelirle(Cep cep){ 
        if (cep == null) return;
        if (cep.colID == 0 || cep.colID == Istaka.Instance.CepList.Count) return;
        List<Cep> ucluk = new List<Cep>();
        ucluCepler.Clear();
        if (Istaka.Instance.CepList.Count > cep.colID && cep.colID>1){
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 2]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluCepler.Add(new List<Cep>(ucluk));
            foreach (var cp in ucluk){
                cp.transform.Find("ForDebug").GetComponent<SpriteRenderer>().color = Color.green;
            }
            ucluk.Clear();
        }

        if (Istaka.Instance.CepList.Count > cep.colID-1 
            && Istaka.Instance.CepList.Count > cep.colID+1){
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 1]);
            ucluCepler.Add(new List<Cep>(ucluk));
            foreach (var cp in ucluk){
                cp.transform.Find("ForDebug").GetComponent<SpriteRenderer>().color = Color.green;
            }
            ucluk.Clear();
        }

        if (Istaka.Instance.CepList.Count > cep.colID+2){
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 2]);
            ucluCepler.Add(new List<Cep>(ucluk));
            foreach (var cp in ucluk){
                cp.transform.Find("ForDebug").GetComponent<SpriteRenderer>().color = Color.green;
            }
            ucluk.Clear();
        }
        
        if (cep.colID  == Istaka.Instance.CepList.Count-1){
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 2]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 1]);
            ucluCepler.Add(new List<Cep>(ucluk));
            foreach (var cp in ucluk){
                cp.transform.Find("ForDebug").GetComponent<SpriteRenderer>().color = Color.green;
            }
            ucluk.Clear();
        }
        
        cep.transform.Find("ForDebug").GetComponent<SpriteRenderer>().color = Color.red;
    }
    
    private  void PerAdayiKurallariniBelirle(){
        AranacakKriterler.Clear();
        Dictionary<string, object> kriter = new Dictionary<string, object>();
        foreach (var ucluk in ucluCepler){
            kriter.Clear();
            int arananMeyveID = -1;
            int yasakliMeyveID0 = -1;
            int yasakliMeyveID1 = -1;
            Color arananRenk = default;
            Color yasakliRenk0 = default;
            Color yasakliRenk1 = default;

            // 0 1 1
            if (ucluk[0].TasInstance == null){
                if (ucluk[1].TasInstance && ucluk[2].TasInstance){ 
                    // Aynı meyve 
                    if (ucluk[1].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID){ 
                        arananMeyveID = ucluk[1].TasInstance.MeyveID;
                        kriter.Add("arananMeyveID", arananMeyveID);
                        //aynı renk
                        if (ucluk[1].TasInstance.Renk == ucluk[2].TasInstance.Renk){
                            arananRenk = ucluk[1].TasInstance.Renk;  
                            kriter.Add("arananRenk", arananRenk); 
                        } else { // farklı renk 
                            yasakliRenk0 = ucluk[1].TasInstance.Renk;
                            yasakliRenk1 = ucluk[2].TasInstance.Renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        } 
                    }
                    
                    // farklı meyve
                    if (ucluk[1].TasInstance.MeyveID != ucluk[2].TasInstance.MeyveID){
                        if (ucluk[1].TasInstance.Renk == ucluk[2].TasInstance.Renk){
                            yasakliMeyveID0 = ucluk[1].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[2].TasInstance.MeyveID ;
                            arananRenk = ucluk[1].TasInstance.Renk;
                            kriter.Add("arananRenk", arananRenk); 
                            kriter.Add("yasakliMeyveID0", yasakliMeyveID0); 
                            kriter.Add("yasakliMeyveID1", yasakliMeyveID1); 
                        } 
                    } 
                    kriter.Add("hedefCep", ucluk[0]); 
                    AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter)); 
                }
            }

            //1 0 1
            if (ucluk[1].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[2].TasInstance){ 
                    arananMeyveID = ucluk[0].TasInstance.MeyveID;
                    kriter.Add("arananMeyveID", arananMeyveID);
                    // Aynı meyve 
                    if (ucluk[0].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID){  
                        //aynı renk
                        if (ucluk[0].TasInstance.Renk == ucluk[2].TasInstance.Renk){
                            arananRenk = ucluk[0].TasInstance.Renk; 
                            kriter.Add("arananRenk", arananRenk);  
                        } else { // farklı renk
                            yasakliRenk0 = ucluk[0].TasInstance.Renk;
                            yasakliRenk1 = ucluk[2].TasInstance.Renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }
                    }
                    
                    // farklı meyve
                    if (ucluk[0].TasInstance.MeyveID != ucluk[2].TasInstance.MeyveID){
                        if (ucluk[0].TasInstance.Renk == ucluk[2].TasInstance.Renk){
                            yasakliMeyveID0 = ucluk[0].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[2].TasInstance.MeyveID ;
                            arananRenk = ucluk[0].TasInstance.Renk;
                            kriter.Add("arananRenk", arananRenk); 
                            kriter.Add("yasakliMeyveID0", yasakliMeyveID0); 
                            kriter.Add("yasakliMeyveID1", yasakliMeyveID1); 
                        } 
                    } 
                    kriter.Add("hedefCep", ucluk[1]); 
                    AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter)); 
                } 
            }

            //1 1 0
            if (ucluk[2].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[1].TasInstance){ 
                    // Aynı meyve 
                    if (ucluk[0].TasInstance.MeyveID == ucluk[1].TasInstance.MeyveID){  
                        arananMeyveID = ucluk[0].TasInstance.MeyveID;
                        kriter.Add("arananMeyveID", arananMeyveID); 
                        //aynı renk
                        if (ucluk[0].TasInstance.Renk == ucluk[1].TasInstance.Renk){
                            arananRenk = ucluk[0].TasInstance.Renk; 
                            kriter.Add("arananRenk", arananRenk);  
                        } else { // farklı renk  
                            yasakliRenk0 = ucluk[0].TasInstance.Renk;
                            yasakliRenk1 = ucluk[1].TasInstance.Renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }
                    }
                    
                    // farklı meyve
                    if (ucluk[0].TasInstance.MeyveID != ucluk[1].TasInstance.MeyveID){
                        if (ucluk[0].TasInstance.Renk == ucluk[1].TasInstance.Renk){
                            yasakliMeyveID0 = ucluk[0].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[1].TasInstance.MeyveID ;
                            arananRenk = ucluk[0].TasInstance.Renk;
                            kriter.Add("arananRenk", arananRenk);
                            kriter.Add("yasakliMeyveID0", yasakliMeyveID0); 
                            kriter.Add("yasakliMeyveID1", yasakliMeyveID1); 
                        } 
                    } 
                    kriter.Add("hedefCep", ucluk[2]); 
                    AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter)); 
                } 
            }
            
        } // foreach
    }
    
    private  void KurallarauygunIlkTasiBul(){
        List<Tas> tavsiyeEdilecekTaslar = new List<Tas>();
        tavsiyeEdilecekTaslar.Clear();
        foreach (var kriter in AranacakKriterler){
            int arananMeyveID = -1;
            int yasakliMeyveID0 = -1;
            int yasakliMeyveID1 = -1;
            Color arananRenk = default;
            Color yasakliRenk0 = default;
            Color yasakliRenk1 = default;
            Cep hedefCep = null;
 
            foreach (var tas in carddakiTaslar){
                bool MeyveIDTamam = false;
                bool uygunRenkTamam = false;
                bool yasakliRenklerTamam = false;
                bool yasakliMeyveTamam = false;
                var  tavsiyeEdilecekTas = TasManeger.Instance.TasInstances[tas];
                bool PerdeTasMevcut = false; 

                if (kriter.Value.ContainsKey("arananMeyveID")){
                    arananMeyveID = (int)kriter.Value["arananMeyveID"];
                    MeyveIDTamam = tavsiyeEdilecekTas.MeyveID == arananMeyveID;
                }
                else if (kriter.Value.ContainsKey("yasakliMeyveID0") && kriter.Value.ContainsKey("yasakliMeyveID1")){
                    yasakliMeyveID0 =  (int)kriter.Value["yasakliMeyveID0"];
                    yasakliMeyveID1 =  (int)kriter.Value["yasakliMeyveID1"];
                    yasakliMeyveTamam = (tavsiyeEdilecekTas.MeyveID != yasakliMeyveID0)
                                        && (tavsiyeEdilecekTas.MeyveID != yasakliMeyveID1);
                }

                if (kriter.Value.ContainsKey("arananRenk")){
                    arananRenk = (Color)kriter.Value["arananRenk"];
                    uygunRenkTamam = tavsiyeEdilecekTas.Renk == arananRenk;
                }
                else if (kriter.Value.ContainsKey("yasakliRenk0") && kriter.Value.ContainsKey("yasakliRenk1")){
                    yasakliRenk0 = (Color)kriter.Value["yasakliRenk0"];
                    yasakliRenk1 = (Color)kriter.Value["yasakliRenk1"];
                    yasakliRenklerTamam = (tavsiyeEdilecekTas.Renk != yasakliRenk0)
                                          && (tavsiyeEdilecekTas.Renk != yasakliRenk1);
                }
                
                // Hedef cepten önce yada sonra per var mı ? Cepteki PerAilesine bakalım.
                if (kriter.Value.ContainsKey("hedefCep")){
                    hedefCep = (Cep)kriter.Value["hedefCep"];
                    int oncekiCepID;
                    int sonrakiCepID;
                    Tas oncekiCeptekiTas = null;
                    Tas sonrakiCeptekiTas= null;
                    if (hedefCep.colID > 0){
                        oncekiCepID = hedefCep.colID - 1;
                        oncekiCeptekiTas = Istaka.Instance.CepList[oncekiCepID].TasInstance;
                    }
                    if (hedefCep.colID < Istaka.Instance.CepList.Count-1){
                        sonrakiCepID = hedefCep.colID + 1;
                        sonrakiCeptekiTas = Istaka.Instance.CepList[sonrakiCepID].TasInstance;
                    }
                    
                    // öncekinde ailevarmı
                    if (oncekiCeptekiTas?.PerAilesi is not null){
                        var aile = oncekiCeptekiTas.PerAilesi;
                        PerdeTasMevcut = AiledeTavsiyeEdicelecekTasVarmi(aile,tavsiyeEdilecekTas);
                    }
                    // sonrakinde ailevarmı
                    if (sonrakiCeptekiTas?.PerAilesi is not null){
                        var aile = sonrakiCeptekiTas.PerAilesi;
                        PerdeTasMevcut = AiledeTavsiyeEdicelecekTasVarmi(aile,tavsiyeEdilecekTas);
                    }
                }

                if (!PerdeTasMevcut){ 
                    if ((MeyveIDTamam || yasakliMeyveTamam) && uygunRenkTamam){
                        tavsiyeEdilecekTaslar.Add(tavsiyeEdilecekTas);
                    } else if ((MeyveIDTamam || yasakliMeyveTamam)  && yasakliRenklerTamam){
                        tavsiyeEdilecekTaslar.Add(tavsiyeEdilecekTas);
                    }
                }
            } // foreach
        } // foreeach

        foreach (var tavsiyeTas in tavsiyeEdilecekTaslar){
            tavsiyeTas.sallanmaDurumu=true; 
        } 
    }

    private bool AiledeTavsiyeEdicelecekTasVarmi(Dictionary<int, GameObject> Per, Tas tavsiyeEdilecekTas){
        try{
            bool mevcut = false; 
            //perin iki tasina bakarak netür bir per olduğuna karar verelim
            var pTasIns0 = TasManeger.Instance.TasInstances[Per[0]];
            var pTasIns1 = TasManeger.Instance.TasInstances[Per[1]];
            // renk aynimi 
            bool renkAyni = false;
            bool meyveAyni = false;
            renkAyni = (pTasIns0.Renk == pTasIns1.Renk);
            meyveAyni = (pTasIns0.MeyveID == pTasIns1.MeyveID);
        
            foreach (var pTas in Per){  
                var pTasIsntance = TasManeger.Instance.TasInstances[pTas.Value];
            
                // R ayni M farkli per ise
                if ((renkAyni && !meyveAyni) || (!renkAyni && meyveAyni)){
                    if (tavsiyeEdilecekTas.MeyveID == pTasIsntance.MeyveID){ 
                        mevcut = true;
                        break;
                    }
                }   
            }
            return mevcut;
        }
        catch (Exception e){
            Console.WriteLine($"AiledeTavsiyeEdicelecekTasVarmi içinde hata : {e.Message}");
            return false;
        } 
    }
}
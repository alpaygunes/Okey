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
    private  float GeriSayimSuresi{ get; set; } = 2.5f;

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
        float _timeLeft = GeriSayimSuresi;
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--;
        }
        if (GameManager.Instance.OyunDurumu == GameManager.OynanmaDurumu.oynaniyor)
            TasBul();
    }
    
    private  void TasBul(){ 
        _istakadakiIlkBosCep = IstakaKontrol();
        uclukleriBelirle(_istakadakiIlkBosCep);
        uclugeUygunKurallariBelirle();
        kurallarauygunIlkTasiBul(); 
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

    private  void kurallarauygunIlkTasiBul(){
        List<Tas> aranantaslar = new List<Tas>();
        aranantaslar.Clear();
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

                if (kriter.Value.ContainsKey("hedefCep")){
                    hedefCep = (Cep)kriter.Value["hedefCep"];
                }

                if (kriter.Value.ContainsKey("arananMeyveID")){
                    arananMeyveID = (int)kriter.Value["arananMeyveID"];
                    MeyveIDTamam = TasManeger.Instance.TasInstances[tas].MeyveID == arananMeyveID;
                }
                else if (kriter.Value.ContainsKey("yasakliMeyveID0") && kriter.Value.ContainsKey("yasakliMeyveID1")){
                    yasakliMeyveID0 =  (int)kriter.Value["yasakliMeyveID0"];
                    yasakliMeyveID1 =  (int)kriter.Value["yasakliMeyveID1"];
                    yasakliMeyveTamam = (TasManeger.Instance.TasInstances[tas].MeyveID != yasakliMeyveID0)
                                        && (TasManeger.Instance.TasInstances[tas].MeyveID != yasakliMeyveID1);
                }

                if (kriter.Value.ContainsKey("arananRenk")){
                    arananRenk = (Color)kriter.Value["arananRenk"];
                    uygunRenkTamam = TasManeger.Instance.TasInstances[tas].renk == arananRenk;
                }
                else if (kriter.Value.ContainsKey("yasakliRenk0") && kriter.Value.ContainsKey("yasakliRenk1")){
                    yasakliRenk0 = (Color)kriter.Value["yasakliRenk0"];
                    yasakliRenk1 = (Color)kriter.Value["yasakliRenk1"];
                    yasakliRenklerTamam = (TasManeger.Instance.TasInstances[tas].renk != yasakliRenk0)
                                          && (TasManeger.Instance.TasInstances[tas].renk != yasakliRenk1);
                }

                if ((MeyveIDTamam || yasakliMeyveTamam) && uygunRenkTamam){
                    aranantaslar.Add(TasManeger.Instance.TasInstances[tas]);
                } else if ((MeyveIDTamam || yasakliMeyveTamam)  && yasakliRenklerTamam){
                    aranantaslar.Add(TasManeger.Instance.TasInstances[tas]);
                }
            } // foreach
        } // foreeach

        foreach (var bulunanTas in aranantaslar){
            bulunanTas.Sallan();
        }
    }

    private  void uclugeUygunKurallariBelirle(){
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
                        if (ucluk[1].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[1].TasInstance.renk;  
                            kriter.Add("arananRenk", arananRenk); 
                        } else { // farklı renk 
                            yasakliRenk0 = ucluk[1].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        } 
                    }
                    
                    // farklı meyve
                    if (ucluk[1].TasInstance.MeyveID != ucluk[2].TasInstance.MeyveID){
                        if (ucluk[1].TasInstance.renk == ucluk[2].TasInstance.renk){
                            yasakliMeyveID0 = ucluk[1].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[2].TasInstance.MeyveID ;
                            arananRenk = ucluk[1].TasInstance.renk;
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
                        if (ucluk[0].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk; 
                            kriter.Add("arananRenk", arananRenk);  
                        } else { // farklı renk
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }
                    }
                    
                    // farklı meyve
                    if (ucluk[0].TasInstance.MeyveID != ucluk[2].TasInstance.MeyveID){
                        if (ucluk[0].TasInstance.renk == ucluk[2].TasInstance.renk){
                            yasakliMeyveID0 = ucluk[1].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[2].TasInstance.MeyveID ;
                            arananRenk = ucluk[1].TasInstance.renk;
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
                        if (ucluk[0].TasInstance.renk == ucluk[1].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk; 
                            kriter.Add("arananRenk", arananRenk);  
                        } else { // farklı renk  
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[1].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0); 
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }
                    }
                    
                    // farklı meyve
                    if (ucluk[0].TasInstance.MeyveID != ucluk[1].TasInstance.MeyveID){
                        if (ucluk[0].TasInstance.renk == ucluk[1].TasInstance.renk){
                            yasakliMeyveID0 = ucluk[0].TasInstance.MeyveID ;
                            yasakliMeyveID1 = ucluk[1].TasInstance.MeyveID ;
                            arananRenk = ucluk[0].TasInstance.renk;
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

    void uclukleriBelirle(Cep cep){
        //if (Istaka.Instance.CepList.Count==GameManager.Instance.CepSayisi) return;
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
}
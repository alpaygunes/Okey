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

    public void Sallanma(){
        carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar){
            TasManeger.Instance.TasInstances[tas].Sallanma();
        }
    }

    private  IEnumerator CountdownRoutine(){
        float _timeLeft = GeriSayimSuresi;
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--;
        }
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
            int arananMeyveID = 0;
            Color arananRenk = default;
            Color yasakliRenk0 = default;
            Color yasakliRenk1 = default;
            Cep hedefCep = null;

 
            foreach (var tas in carddakiTaslar){
                bool rakamTamam = false;
                bool uygunRenkTamam = false;
                bool tasakliRenklerTamam = false;

                if (kriter.Value.ContainsKey("hedefCep")){
                    hedefCep = (Cep)kriter.Value["hedefCep"];
                }

                if (kriter.Value.ContainsKey("arananMeyveID")){
                    arananMeyveID = (int)kriter.Value["arananMeyveID"];
                    rakamTamam = TasManeger.Instance.TasInstances[tas].MeyveID == arananMeyveID;
                }

                if (kriter.Value.ContainsKey("arananRenk")){
                    arananRenk = (Color)kriter.Value["arananRenk"];
                    uygunRenkTamam = TasManeger.Instance.TasInstances[tas].renk == arananRenk;
                }
                else{
                    yasakliRenk0 = (Color)kriter.Value["yasakliRenk0"];
                    yasakliRenk1 = (Color)kriter.Value["yasakliRenk1"];
                    tasakliRenklerTamam = (TasManeger.Instance.TasInstances[tas].renk != yasakliRenk0)
                                          && (TasManeger.Instance.TasInstances[tas].renk != yasakliRenk1);
                }

                if (rakamTamam && uygunRenkTamam){
                    aranantaslar.Add(TasManeger.Instance.TasInstances[tas]);
                }
                else if (rakamTamam && tasakliRenklerTamam){
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
            int arananMeyveID = 0;
            Color arananRenk = default;
            Color yasakliRenk0 = default;
            Color yasakliRenk1 = default;

            // 0 1 1
            if (ucluk[0].TasInstance == null){
                if (ucluk[1].TasInstance && ucluk[2].TasInstance){
                    if (ucluk[1].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID){
                        arananMeyveID = ucluk[1].TasInstance.MeyveID;
                    }
                    else if (ucluk[1].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID - 1){
                        arananMeyveID = ucluk[1].TasInstance.MeyveID - 1;
                    }

                    if (arananMeyveID > 0){
                        if (ucluk[1].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[1].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk); 
                        }
                        else{
                            yasakliRenk0 = ucluk[1].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }

                        kriter.Add("hedefCep", ucluk[0]);
                        kriter.Add("arananMeyveID", arananMeyveID); 
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }

            //1 0 1
            if (ucluk[1].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[2].TasInstance){
                    if (ucluk[0].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID){
                        arananMeyveID = ucluk[0].TasInstance.MeyveID;
                    }
                    else if (ucluk[0].TasInstance.MeyveID == ucluk[2].TasInstance.MeyveID - 2){
                        arananMeyveID = ucluk[2].TasInstance.MeyveID - 1;
                    }

                    if (arananMeyveID > 0){
                        if (ucluk[0].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk);
                             
                        }
                        else{
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }

                        kriter.Add("hedefCep", ucluk[1]);
                        kriter.Add("arananMeyveID", arananMeyveID); 
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }

            //1 1 0
            if (ucluk[2].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[1].TasInstance){
                    if (ucluk[0].TasInstance.MeyveID == ucluk[1].TasInstance.MeyveID){
                        arananMeyveID = ucluk[0].TasInstance.MeyveID;
                    }
                    else if (ucluk[0].TasInstance.MeyveID == ucluk[1].TasInstance.MeyveID - 1){
                        arananMeyveID = ucluk[1].TasInstance.MeyveID + 1;
                    }

                    if (arananMeyveID > 0){
                        if (ucluk[0].TasInstance.renk == ucluk[1].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk); 
                        }
                        else{
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[1].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1); 
                        }

                        kriter.Add("hedefCep", ucluk[2]);
                        kriter.Add("arananMeyveID", arananMeyveID); 
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }
        } // foreach
    }

    void uclukleriBelirle(Cep cep){
        if (cep == null) return;
        List<Cep> ucluk = new List<Cep>();
        ucluCepler.Clear();

        if (cep.colID > 1){
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 2]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }

        if (cep.colID + 1 < Istaka.Instance.CepList.Count && cep.colID > 0){
            ucluk.Add(Istaka.Instance.CepList[cep.colID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 1]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }

        if (cep.colID + 2 < Istaka.Instance.CepList.Count){
            ucluk.Add(Istaka.Instance.CepList[cep.colID]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.colID + 2]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }
        
    }
}
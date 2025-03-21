using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public static class PerKontrol{
    private static Cep _istakadakiIlkBosCep;
    private static GameObject[] carddakiTaslar;
    private static List<List<Cep>> ucluCepler = new List<List<Cep>>();

    public static void IstakaKontrol(){
        carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar){
            TasManeger.Instance.TasInstances[tas].Sallanma();
        }

        for (int i = 0; i < Istaka.Instance.CepList.Count; i++){
            if (Istaka.Instance.CepList[i].TasInstance == null){
                _istakadakiIlkBosCep = Istaka.Instance.CepList[i];
                //_istakadakiIlkBosCep.transform.localScale = new Vector2(0.69f, 0.69f);
                //_istakadakiIlkBosCep.transform.localScale = new Vector2(0.5f, 0.5f);
                //_istakadakiIlkBosCep.transform.GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            }
        }

        // foreach (var c in Istaka.Instance.CepList){
        //     c.transform.localScale = new Vector2(0.5f, 0.5f);
        // }

        uclukleriBelirle(_istakadakiIlkBosCep);
        uclugeUygunKurallariBelirle();
        kurallarauygunIlkTasiBul();
    }

    private static void kurallarauygunIlkTasiBul(){
        List<Tas> aranantaslar = new List<Tas>();
        aranantaslar.Clear();
        foreach (var kriter in AranacakKriterler){
            int arananRakam = 0;
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

                if (kriter.Value.ContainsKey("arananRakam")){
                    arananRakam = (int)kriter.Value["arananRakam"];
                    rakamTamam = TasManeger.Instance.TasInstances[tas].rakam == arananRakam;
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
            bulunanTas.transform.localScale = bulunanTas.localScale;
        }
    }


    private static Dictionary<int, Dictionary<string, object>> AranacakKriterler =
        new Dictionary<int, Dictionary<string, object>>();

    private static void uclugeUygunKurallariBelirle(){
        AranacakKriterler.Clear();
        Dictionary<string, object> kriter = new Dictionary<string, object>();
        foreach (var ucluk in ucluCepler){
            kriter.Clear();
            int arananRakam = 0;
            Color arananRenk = default;
            Color yasakliRenk0 = default;
            Color yasakliRenk1 = default;

            // 0 1 1
            if (ucluk[0].TasInstance == null){
                if (ucluk[1].TasInstance && ucluk[2].TasInstance){
                    if (ucluk[1].TasInstance.rakam == ucluk[2].TasInstance.rakam){
                        arananRakam = ucluk[1].TasInstance.rakam;
                    }
                    else if (ucluk[1].TasInstance.rakam == ucluk[2].TasInstance.rakam - 1){
                        arananRakam = ucluk[1].TasInstance.rakam - 1;
                    }

                    if (arananRakam > 0){
                        if (ucluk[1].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[1].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk);
                            Debug.Log($"arananRenk {arananRenk}");
                        }
                        else{
                            yasakliRenk0 = ucluk[1].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1);
                            Debug.Log($"yasakliRenk0 {yasakliRenk0}");
                            Debug.Log($"yasakliRenk1 {yasakliRenk1}");
                        }

                        kriter.Add("hedefCep", ucluk[0]);
                        kriter.Add("arananRakam", arananRakam);
                        Debug.Log($"hedefCep {ucluk[0].ID}");
                        Debug.Log($"arananRakam {arananRakam}");
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }

            //1 0 1
            if (ucluk[1].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[2].TasInstance){
                    if (ucluk[0].TasInstance.rakam == ucluk[2].TasInstance.rakam){
                        arananRakam = ucluk[0].TasInstance.rakam;
                    }
                    else if (ucluk[0].TasInstance.rakam == ucluk[2].TasInstance.rakam - 2){
                        arananRakam = ucluk[2].TasInstance.rakam - 1;
                    }

                    if (arananRakam > 0){
                        if (ucluk[0].TasInstance.renk == ucluk[2].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk);
                            Debug.Log($"arananRenk {arananRenk}");
                        }
                        else{
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[2].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1);
                            Debug.Log($"yasakliRenk0 {yasakliRenk0}");
                            Debug.Log($"yasakliRenk1 {yasakliRenk1}");
                        }

                        kriter.Add("hedefCep", ucluk[1]);
                        kriter.Add("arananRakam", arananRakam);
                        Debug.Log($"hedefCep {ucluk[1].ID}");
                        Debug.Log($"arananRakam {arananRakam}");
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }

            //1 1 0
            if (ucluk[2].TasInstance == null){
                if (ucluk[0].TasInstance && ucluk[1].TasInstance){
                    if (ucluk[0].TasInstance.rakam == ucluk[1].TasInstance.rakam){
                        arananRakam = ucluk[0].TasInstance.rakam;
                    }
                    else if (ucluk[0].TasInstance.rakam == ucluk[1].TasInstance.rakam - 1){
                        arananRakam = ucluk[1].TasInstance.rakam + 1;
                    }

                    if (arananRakam > 0){
                        if (ucluk[0].TasInstance.renk == ucluk[1].TasInstance.renk){
                            arananRenk = ucluk[0].TasInstance.renk;
                            kriter.Add("arananRenk", arananRenk);
                            Debug.Log($"arananRenk {arananRenk}");
                        }
                        else{
                            yasakliRenk0 = ucluk[0].TasInstance.renk;
                            yasakliRenk1 = ucluk[1].TasInstance.renk;
                            kriter.Add("yasakliRenk0", yasakliRenk0);
                            kriter.Add("yasakliRenk1", yasakliRenk1);
                            Debug.Log($"yasakliRenk0 {yasakliRenk0}");
                            Debug.Log($"yasakliRenk1 {yasakliRenk1}");
                        }

                        kriter.Add("hedefCep", ucluk[2]);
                        kriter.Add("arananRakam", arananRakam);
                        Debug.Log($"hedefCep {ucluk[2].ID}");
                        Debug.Log($"arananRakam {arananRakam}");
                        AranacakKriterler.Add(AranacakKriterler.Count, new Dictionary<string, object>(kriter));
                    }
                }
            }
        } // foreach
    }

    static void uclukleriBelirle(Cep cep){
        List<Cep> ucluk = new List<Cep>();
        ucluCepler.Clear();

        if (cep.ID > 1){
            ucluk.Add(Istaka.Instance.CepList[cep.ID - 2]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }

        if (cep.ID + 1 < Istaka.Instance.CepList.Count){
            ucluk.Add(Istaka.Instance.CepList[cep.ID - 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID + 1]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }

        if (cep.ID + 2 < Istaka.Instance.CepList.Count){
            ucluk.Add(Istaka.Instance.CepList[cep.ID]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID + 1]);
            ucluk.Add(Istaka.Instance.CepList[cep.ID + 2]);
            ucluCepler.Add(new List<Cep>(ucluk));
            ucluk.Clear();
        }


        // foreach (var u in ucluCepler){
        //     u[0].transform.localScale = new Vector2(1f, 1f);
        //     u[1].transform.localScale = new Vector2(1f, 1f);
        //     u[2].transform.localScale = new Vector2(1f, 1f);
        // }
    }
}
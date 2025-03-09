using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IstakaKontrolcu : MonoBehaviour{
    public static IstakaKontrolcu Instance;
    public List<Dictionary<GameObject, GameObject>> SiraliGruplar = new List<Dictionary<GameObject, GameObject>>();

    public List<Dictionary<GameObject, GameObject>>
        BenzerRakamGruplari = new List<Dictionary<GameObject, GameObject>>();

    public List<Dictionary<GameObject, GameObject>> SiraliRakamAyniRenkGruplari =
        new List<Dictionary<GameObject, GameObject>>();

    public List<Dictionary<GameObject, GameObject>> AyniRakamAyniRenkGruplari =
        new List<Dictionary<GameObject, GameObject>>();

    public List<Dictionary<GameObject, GameObject>> AyniRakamHepsiFarkliRenkGruplari =
        new List<Dictionary<GameObject, GameObject>>();

    public List<Dictionary<GameObject, GameObject>> SiraliRakamHepsiFarkliRenkGruplari =
        new List<Dictionary<GameObject, GameObject>>();

    private bool _yeniGrupOlustur;

    private void Awake(){
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public int DoluCepSayisi(){
        int doluCepSayisi = 0;
        foreach (var cep in Istaka.Instance.CepList) {
            var cepScript = cep.Value;
            if (cepScript.Dolu) {
                doluCepSayisi++;
            }
        }

        return doluCepSayisi;
    }

    // public void PersizFullIstakayiBosalt(){
    //     var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
    //     var ceptekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS"); 
    //     if (DoluCepSayisi() == Istaka.Instance.CepSayisi) {
    //         foreach (var ceptekiTas in ceptekiTaslar) {
    //             var AIstance = TasManeger.Instance.TasIstances[ceptekiTas];
    //             foreach (var cardtakiTas in cardtakiTaslar) {
    //                 var BInstance = TasManeger.Instance.TasIstances[cardtakiTas];
    //                 if (BInstance.rakam == AIstance.rakam || BInstance.renk == AIstance.renk) { 
    //                     BInstance.ZeminSpriteRenderer.color = Color.red;
    //                     StartCoroutine(BInstance.CezaliRakamiCikar(1));
    //                 }
    //             }
    //             AIstance.ZeminSpriteRenderer.color = Color.red;
    //             StartCoroutine(AIstance.CezaliRakamiCikar(1));
    //         }
    //
    //         foreach (var Cep in Istaka.Instance.CepList) {
    //             Cep.GetComponent<IstakaCebi>().Dolu = false;
    //         }
    //     }
    // }

    // BURADA KALDIN. 
    //     CEPSAYISINI ARTIRARAK DENE BİRDEN FALZA PERİ TESPİT EDEBİLİYORMU.
    // BİR BİRİNİN DEVAMI OLAN ORTAK TAŞ KULLANARNA PERLERE DİKKAT ETMELİSİN . 
    //     tas.cs içinde yeni şeyler ekledin. kontrolleri yaparken orayı baz alamlısın.
    
    
    public void SiraliGruplariBelirle(){
        SiraliGruplar.Clear();
        Dictionary<GameObject, GameObject> siraliGrup = new Dictionary<GameObject, GameObject>();

        // Istakadaki cepleri tek tek kontrol edelim.
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++) {
            _yeniGrupOlustur = false;
            var CepInstance = Istaka.Instance.CepList[i];
            //Cepte taş var mı ?
            if (CepInstance.Dolu) {
                var tasInstance = CepInstance.TasInstance;
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (siraliGrup.Count == 0) {
                    siraliGrup.Add(CepInstance.gameObject, tasInstance.gameObject);
                }

                // sonraki ile ardışık mı ? 
                if (i < Istaka.Instance.CepList.Count - 1) {
                    var sonrakiCepInst = Istaka.Instance.CepList[i + 1];
                    if (sonrakiCepInst.Dolu) {
                        if (tasInstance.rakam == sonrakiCepInst.TasInstance.rakam - 1) {
                            siraliGrup.Add(sonrakiCepInst.gameObject, sonrakiCepInst.TasInstance.gameObject);
                        }
                        else {
                            _yeniGrupOlustur = true;
                        }
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (siraliGrup.Count > 2) {
                    SiraliGruplar.Add(new Dictionary<GameObject, GameObject>(siraliGrup));
                }

                siraliGrup.Clear();
            }
        } //end for

        Debug.Log($" SiraliGruplar {SiraliGruplar.Count}");
    }


    public void BenzerRakamGruplariniBelirle(){
        BenzerRakamGruplari.Clear();
        Dictionary<GameObject, GameObject> benzerRakamGrubu = new Dictionary<GameObject, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (var i = 0; i < Istaka.Instance.CepList.Count; i++) {
            _yeniGrupOlustur = false;
            var cep = Istaka.Instance.CepList[i];
            //Cepte taş var mı ?
            if (cep.Dolu) {
                var tasInstance = cep.TasInstance;
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (benzerRakamGrubu.Count == 0) {
                    benzerRakamGrubu.Add(cep.gameObject, tasInstance.gameObject);
                }

                if (i < Istaka.Instance.CepList.Count - 1) {
                    var sonrakiCepInst = Istaka.Instance.CepList[i + 1];
                    if (sonrakiCepInst.Dolu) {
                        if (tasInstance.rakam == sonrakiCepInst.TasInstance.rakam) {
                            benzerRakamGrubu.Add(sonrakiCepInst.gameObject, sonrakiCepInst.TasInstance.gameObject);
                        }
                        else {
                            _yeniGrupOlustur = true;
                        }
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (benzerRakamGrubu.Count > 2) {
                    BenzerRakamGruplari.Add(new Dictionary<GameObject, GameObject>(benzerRakamGrubu));
                }

                benzerRakamGrubu.Clear();
            }
        } // for end 

        Debug.Log($" BenzerRakamGruplari {BenzerRakamGruplari.Count}");
    }

    public void SiraliGruplarinIcindekiAyniRenkGruplariniBelirle(){
        SiraliRakamAyniRenkGruplari.Clear();
        Dictionary<GameObject, GameObject> renkGrubu = new Dictionary<GameObject, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++) {
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                var tasInstance = Istaka.Instance.TasInstances[tas];
                if (renkGrubu.Count == 0) {
                    renkGrubu.Add(key, tas);
                }

                // sonraki ile aynımı mı ? 
                try {
                    var sonrakiCep = grup.Keys.ToList()[j + 1];
                    var sonrakiTasInstance = Istaka.Instance.TasInstances[grup[sonrakiCep]];
                    Color sonrakiTasinRenk = sonrakiTasInstance.renk;
                    Color simdikiTasinRenk = tasInstance.renk;

                    if (sonrakiTasinRenk == simdikiTasinRenk) {
                        renkGrubu.Add(sonrakiCep.gameObject, sonrakiTasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (renkGrubu.Count > 2) {
                        SiraliRakamAyniRenkGruplari.Add(new Dictionary<GameObject, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for

        Debug.Log($" SiraliRakamAyniRenkGruplari {SiraliRakamAyniRenkGruplari.Count}");
    }

    public void AyniRakamGruplarinIcindekiAyniRenkGruplariniBelirle(){
        AyniRakamAyniRenkGruplari.Clear();
        Dictionary<GameObject, GameObject> renkGrubu = new Dictionary<GameObject, GameObject>();
        
        for (int i = 0; i < BenzerRakamGruplari.Count; i++) {
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                var tasInstance = Istaka.Instance.TasInstances[tas];
                if (renkGrubu.Count == 0) {
                    renkGrubu.Add(key, tas);
                }

                // sonraki ile aynımı mı ? 
                try {
                    var sonrakiCep = grup.Keys.ToList()[j + 1];
                    var sonrakiTasInstance = Istaka.Instance.TasInstances[grup[sonrakiCep]];
                    Color sonrakiTasinRenk = sonrakiTasInstance.renk;
                    Color simdikiTasinRenk = tasInstance.renk;

                    if (sonrakiTasinRenk == simdikiTasinRenk) {
                        renkGrubu.Add(sonrakiCep.gameObject, sonrakiTasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (renkGrubu.Count > 2) {
                        AyniRakamAyniRenkGruplari.Add(new Dictionary<GameObject, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for
         
        Debug.Log($" AyniRakamAyniRenkGruplari {AyniRakamAyniRenkGruplari.Count}");
    }

    public void AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        AyniRakamHepsiFarkliRenkGruplari.Clear();
        Dictionary<GameObject, GameObject> farkliRenklilerGrubu = new Dictionary<GameObject, GameObject>();
        for (int i = 0; i < BenzerRakamGruplari.Count; i++) {
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0) {
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ? 
                try {
                    var sonrakiCep = grup.Keys.ToList()[j + 1];
                    var sonrakiTasInstance = Istaka.Instance.TasInstances[grup[sonrakiCep]]; 

                    bool ayniRenkVar = false;
                    foreach (var item in farkliRenklilerGrubu) {
                        if (sonrakiTasInstance.gameObject != item.Value) {
                            Color itemRenk = Istaka.Instance.TasInstances[item.Value].renk;
                            if (itemRenk == sonrakiTasInstance.renk) {
                                ayniRenkVar = true;
                                break;
                            }
                        }
                    }

                    if (!ayniRenkVar) {
                        farkliRenklilerGrubu.Add(sonrakiCep, sonrakiTasInstance.gameObject);
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (farkliRenklilerGrubu.Count > 2) {
                        AyniRakamHepsiFarkliRenkGruplari.Add(
                            new Dictionary<GameObject, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 

        Debug.Log($" AyniRakamHepsiFarkliRenkGruplari {AyniRakamHepsiFarkliRenkGruplari.Count}");
    }

    public void SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        SiraliRakamHepsiFarkliRenkGruplari.Clear();
        Dictionary<GameObject, GameObject> farkliRenklilerGrubu = new Dictionary<GameObject, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++) {
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0) {
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ? 
                try {
                    var sonrakiCep = grup.Keys.ToList()[j + 1];
                    var sonrakiTasInstance = Istaka.Instance.TasInstances[grup[sonrakiCep]];  

                    bool ayniRenkVar = false;
                    foreach (var item in farkliRenklilerGrubu) {
                        if (sonrakiTasInstance.gameObject != item.Value) {
                            Color itemRenk = Istaka.Instance.TasInstances[item.Value].renk;
                            if (itemRenk == sonrakiTasInstance.renk) {
                                ayniRenkVar = true;
                                break;
                            }
                        }
                    }

                    if (!ayniRenkVar) {
                        farkliRenklilerGrubu.Add(sonrakiCep, sonrakiTasInstance.gameObject);
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (farkliRenklilerGrubu.Count > 2) {
                        SiraliRakamHepsiFarkliRenkGruplari.Add(
                            new Dictionary<GameObject, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 

        Debug.Log($" SiraliRakamHepsiFarkliRenkGruplari {SiraliRakamHepsiFarkliRenkGruplari.Count}");
    }

    public void GruplariTemizle(){
        // SiraliRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamAyniRenkGruplari.Count; i++) {
            var grup = SiraliRakamAyniRenkGruplari[i];
            foreach (var item in grup) {
                Istaka.Instance.CepVeTas.Remove(item.Key);
                item.Key.GetComponent<IstakaCebi>().Dolu = false;
            }
        }

        // AyniRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamAyniRenkGruplari.Count; i++) {
            var grup = AyniRakamAyniRenkGruplari[i];
            foreach (var item in grup) {
                Istaka.Instance.CepVeTas.Remove(item.Key);
                item.Key.GetComponent<IstakaCebi>().Dolu = false;
            }
        }


        // AyniRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamHepsiFarkliRenkGruplari.Count; i++) {
            var grup = AyniRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup) {
                Istaka.Instance.CepVeTas.Remove(item.Key);
                item.Key.GetComponent<IstakaCebi>().Dolu = false;
            }
        }

        // SiraliRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamHepsiFarkliRenkGruplari.Count; i++) {
            var grup = SiraliRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup) {
                Istaka.Instance.CepVeTas.Remove(item.Key);
                item.Key.GetComponent<IstakaCebi>().Dolu = false;
            }
        }

        SiraliRakamHepsiFarkliRenkGruplari.Clear();
        SiraliRakamAyniRenkGruplari.Clear();
        BenzerRakamGruplari.Clear();
        SiraliGruplar.Clear();
        AyniRakamHepsiFarkliRenkGruplari.Clear();
    }
}
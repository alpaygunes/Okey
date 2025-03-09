using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Istaka : MonoBehaviour{
    public int CepSayisi { get; set; } = 10;
    public List<IstakaCebi> CepList = new List<IstakaCebi>();
    public static Istaka Instance;
    public List<Dictionary<int, GameObject>> SiraliGruplar = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> BenzerRakamGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> SiraliRakamAyniRenkGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniRakamAyniRenkGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniRakamHepsiFarkliRenkGruplari = new List<Dictionary<int, GameObject>>();

    public List<Dictionary<int, GameObject>> SiraliRakamHepsiFarkliRenkGruplari =
        new List<Dictionary<int, GameObject>>();

    private bool _yeniGrupOlustur;

    private void Awake(){
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start(){
        CepleriOlustur();
    }

    public int DoluCepSayisi(){
        int doluCepSayisi = 0;
        foreach (var cepIns in CepList) {
            //var cepScript = cep.GetComponent<IstakaCebi>();
            if (cepIns.Dolu) {
                doluCepSayisi++;
            }
        }

        return doluCepSayisi;
    }

    void CepleriOlustur(){
        float istakaGenisligi = GetComponent<SpriteRenderer>().bounds.size.x;
        float aralikMesafesi = istakaGenisligi / CepSayisi;
        for (int i = 0; i < CepSayisi; i++) {
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - istakaGenisligi * .5f;
            GameObject Cep = Resources.Load<GameObject>("Prefabs/IstakaCebi");
            var cep = Instantiate(Cep, new Vector3(x, transform.position.y, -2), Quaternion.identity);
            cep.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -1);
            cep.transform.localScale *= .7f;
            cep.transform.SetParent(PlatformManager.Instance.transform);
            CepList.Add(cep.GetComponent<IstakaCebi>());
        }
    }

    public void PersizFullIstakayiBosalt(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        var ceptekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
        if (DoluCepSayisi() == CepSayisi) { 
            foreach (var cepInstance in CepList) {
                var AIstance = cepInstance.TasInstance;
                foreach (var cardtakiTas in cardtakiTaslar) {
                    var BInstance = TasManeger.Instance.TasIstances[cardtakiTas];
                    if (BInstance.rakam == AIstance.rakam || BInstance.renk == AIstance.renk) {
                        BInstance.ZeminSpriteRenderer.color = Color.red;
                        StartCoroutine(BInstance.CezaliRakamiCikar(1));
                    }
                }
    
                AIstance.ZeminSpriteRenderer.color = Color.red;
                StartCoroutine(AIstance.CezaliRakamiCikar(1));

                cepInstance.Dolu = false;
                cepInstance.TasInstance = null;
            } 
        }
    }

    public void SiraliGruplariBelirle(){
        SiraliGruplar.Clear();
        Dictionary<int, GameObject> siraliGrup = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < CepList.Count; i++) {
            IstakaCebi cepInstance = CepList[i];
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (cepInstance.TasInstance) {
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (siraliGrup.Count == 0) {
                    siraliGrup.Add(i, cepInstance.TasInstance.gameObject);
                }

                // sonraki ile ardışık mı ?
                try {
                    IstakaCebi sonrakiCepInstance = CepList[i + 1];
                    if (cepInstance.TasInstance.rakam == sonrakiCepInstance.TasInstance.rakam - 1) {
                        siraliGrup.Add(i + 1, sonrakiCepInstance.TasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (siraliGrup.Count > 2) {
                    SiraliGruplar.Add(new Dictionary<int, GameObject>(siraliGrup));
                }

                siraliGrup.Clear();
            }
        } // for end 
 
    }

    public void BenzerRakamGruplariniBelirle(){
        BenzerRakamGruplari.Clear();
        Dictionary<int, GameObject> benzerRakamGrubu = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < CepList.Count; i++) {
            IstakaCebi cepInstance = CepList[i];
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (cepInstance.TasInstance) {
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (benzerRakamGrubu.Count == 0) {
                    benzerRakamGrubu.Add(i, cepInstance.TasInstance.gameObject);
                }

                // sonraki ile ardışık mı ?
                try {
                    IstakaCebi sonrakiCepInstance = CepList[i + 1];
                    if (cepInstance.TasInstance.rakam == sonrakiCepInstance.TasInstance.rakam) {
                        benzerRakamGrubu.Add(i + 1, sonrakiCepInstance.TasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) {
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (benzerRakamGrubu.Count > 2) {
                    BenzerRakamGruplari.Add(new Dictionary<int, GameObject>(benzerRakamGrubu));
                }

                benzerRakamGrubu.Clear();
            }
        } // for end 
    }

    public void SiraliGruplarinIcindekiRenkGruplariniBelirle(){
        SiraliRakamAyniRenkGruplari.Clear();
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++) {
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                if (renkGrubu.Count == 0) {
                    renkGrubu.Add(key, tas);
                }

                //sonraki var mı ?  
                if (j + 1 < grup.Keys.ToList().Count) {
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)) {
                        // rengi aynı mı ?
                        if (TasManeger.Instance.TasIstances[tas].renk ==
                            TasManeger.Instance.TasIstances[sonrakiTasA].renk) {
                            renkGrubu.Add(grup.Keys.ToList()[j + 1], sonrakiTasA);
                        }
                        else {
                            _yeniGrupOlustur = true;
                        }
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (renkGrubu.Count > 2) {
                        SiraliRakamAyniRenkGruplari.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for
    }

    public void AyniRakamGruplarinIcindekiRenkGruplariniBelirle(){
        AyniRakamAyniRenkGruplari.Clear();
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < BenzerRakamGruplari.Count; i++) {
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                if (renkGrubu.Count == 0) {
                    renkGrubu.Add(key, tas);
                }

                //sonraki var mı ?  
                if (j + 1 < grup.Keys.ToList().Count) {
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)) {
                        // rengi aynı mı ?
                        if (TasManeger.Instance.TasIstances[tas].renk ==
                            TasManeger.Instance.TasIstances[sonrakiTasA].renk) {
                            renkGrubu.Add(grup.Keys.ToList()[j + 1], sonrakiTasA);
                        }
                        else {
                            _yeniGrupOlustur = true;
                        }
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (renkGrubu.Count > 2) {
                        AyniRakamAyniRenkGruplari.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for
    }

    public void AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        AyniRakamHepsiFarkliRenkGruplari.Clear();
        Dictionary<int, GameObject> farkliRenklilerGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < BenzerRakamGruplari.Count; i++) {
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                int key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0) {
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ?
                if (j + 1 < grup.Keys.ToList().Count) {
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)) {
                        foreach (var item in farkliRenklilerGrubu) {
                            if (TasManeger.Instance.TasIstances[sonrakiTasA].renk
                                == TasManeger.Instance.TasIstances[item.Value].renk) {
                                //aynı renk zaten var
                                _yeniGrupOlustur = true;
                                break;
                            }
                        }

                        if (!_yeniGrupOlustur) {
                            farkliRenklilerGrubu.Add(sonrakiKey, sonrakiTasA);
                        }
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (farkliRenklilerGrubu.Count > 2) {
                        AyniRakamHepsiFarkliRenkGruplari.Add(new Dictionary<int, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 
    }

    public void SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        SiraliRakamHepsiFarkliRenkGruplari.Clear();
        Dictionary<int, GameObject> farkliRenklilerGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++) {
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++) {
                _yeniGrupOlustur = false;
                int key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0) {
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ?
                if (j + 1 < grup.Keys.ToList().Count) {
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)) {
                        foreach (var item in farkliRenklilerGrubu) {
                            if (TasManeger.Instance.TasIstances[sonrakiTasA].renk
                                == TasManeger.Instance.TasIstances[item.Value].renk) {
                                //aynı renk zaten var
                                _yeniGrupOlustur = true;
                                break;
                            }
                        }

                        if (!_yeniGrupOlustur) {
                            farkliRenklilerGrubu.Add(sonrakiKey, sonrakiTasA);
                        }
                    }
                }
                else {
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur) {
                    if (farkliRenklilerGrubu.Count > 2) {
                        SiraliRakamHepsiFarkliRenkGruplari.Add(new Dictionary<int, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 
    }

    public void GruplariTemizle(){
        // SiraliRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamAyniRenkGruplari.Count; i++) {
            var grup = SiraliRakamAyniRenkGruplari[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
        // AyniRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamAyniRenkGruplari.Count; i++) {
            var grup = AyniRakamAyniRenkGruplari[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
    
        // AyniRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamHepsiFarkliRenkGruplari.Count; i++) {
            var grup = AyniRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
        // SiraliRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamHepsiFarkliRenkGruplari.Count; i++) {
            var grup = SiraliRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup) { 
                var cepins = CepList[item.Key];
                cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
        SiraliRakamHepsiFarkliRenkGruplari.Clear();
        SiraliRakamAyniRenkGruplari.Clear();
        BenzerRakamGruplari.Clear();
        SiraliGruplar.Clear();
        AyniRakamHepsiFarkliRenkGruplari.Clear();
    }
}
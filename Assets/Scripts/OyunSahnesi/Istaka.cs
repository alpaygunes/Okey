using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Istaka : MonoBehaviour{
    
    public List<Cep> CepList = new List<Cep>();
    public static Istaka Instance;
    public List<Dictionary<int, GameObject>> FarkliMeyvePerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyvePerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> FarkliMeyveAyniRenkPerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyveAyniRenkPerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyveFarkliRenkPerleri = new List<Dictionary<int, GameObject>>();
    public GameObject Body;
    private float posYrate = 0.66f;
    
    private bool _yeniGrupOlustur;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this; 
        Body = transform.Find("Body").gameObject;
    }

    private void Start(){
        float y = Card.Instance.Size.y;
        transform.position = new Vector3(0, -y*posYrate, 0);
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

    public void CepleriOlustur(){
        float istakaGenisligi = Body.GetComponent<SpriteRenderer>().bounds.size.x;
        float aralikMesafesi = istakaGenisligi / GameManager.Instance.CepSayisi;
        for (int i = 0; i < GameManager.Instance.CepSayisi; i++) {
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - istakaGenisligi * .5f;
            GameObject Cep = Resources.Load<GameObject>("Prefabs/IstakaCebi");
            var cep = Instantiate(Cep, new Vector3(x, Body.transform.position.y, -2), Quaternion.identity);
            cep.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -5);
            cep.transform.localScale *= .2f;
            cep.GetComponent<Cep>().colID = i;
            cep.transform.SetParent(Body.transform);
            CepList.Add(cep.GetComponent<Cep>());
        }
    }

    public void PersizFullIstakayiBosalt(){
        int cezaliPuan = 0;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS"); 
        if (DoluCepSayisi() == GameManager.Instance.CepSayisi) {
            foreach (var cepInstance in CepList) { 
                if (cepInstance is null) break; 
                var AIstance = cepInstance.TasInstance;
                foreach (var cardtakiTas in cardtakiTaslar) {
                    if (cardtakiTas is null) break; 
                    var BInstance = TasManeger.Instance.TasInstances[cardtakiTas];
                    if (BInstance.MeyveID == AIstance.MeyveID || BInstance.renk == AIstance.renk) {
                        BInstance.zeminSpriteRenderer.color = Color.red;
                        StartCoroutine(BInstance.CezaliRakamiCikar(1));
                        cezaliPuan += BInstance.MeyveID;
                    }
                }
    
                AIstance.zeminSpriteRenderer.color = Color.red; 
                cezaliPuan += AIstance.MeyveID;
                StartCoroutine(AIstance.CezaliRakamiCikar(1));
                cepInstance.TasInstance = null;
                
            } 
            Puanlama.Instance.ToplamPuan -= cezaliPuan;
            //Tamam düğmesini gizle
            if ( GameManager.Instance.PerKontrolDugmesiOlsun){
                OyunSahnesiUI.Instance.puanlamaYap.style.display = DisplayStyle.None;
            } 
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) YildizleriGizle();
        }   
    }

    public void FarkliMeyvePerleriniBelirle(){
        FarkliMeyvePerleri.Clear();
        Dictionary<int, GameObject> farklilarGrupu = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < CepList.Count; i++) {
            Cep cepInstance = CepList[i];
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (cepInstance.TasInstance) {
                // grup yeni grupsa farkliliğina bakamdan gruba ekleyelim
                if (farklilarGrupu.Count == 0) {
                    farklilarGrupu.Add(i, cepInstance.TasInstance.gameObject);
                }
                
                // sonraki ile farklimi mı ?
                try {
                    Cep sonrakiCepInstance = CepList[i + 1];
                    if (cepInstance.TasInstance.MeyveID != sonrakiCepInstance?.TasInstance?.MeyveID 
                        && !farklilarGrupu.ContainsValue(sonrakiCepInstance?.TasInstance?.gameObject)) {
                        farklilarGrupu.Add(i + 1, sonrakiCepInstance.TasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) {
                    Debug.Log(e.Message);
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (farklilarGrupu.Count > 2) {
                    FarkliMeyvePerleri.Add(new Dictionary<int, GameObject>(farklilarGrupu));
                }

                farklilarGrupu.Clear();
            }
        } // for end 
 
    }

    public void FarkliMeyveGruplriIcindeAyniRenkPerleriniBelirle(){
        FarkliMeyveAyniRenkPerleri.Clear();
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < FarkliMeyvePerleri.Count; i++) {
            var grup = FarkliMeyvePerleri[i];
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
                        if (TasManeger.Instance.TasInstances[tas].renk ==
                            TasManeger.Instance.TasInstances[sonrakiTasA].renk) {
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
                        FarkliMeyveAyniRenkPerleri.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for
    }
    
    public void AyniMeyvePerleriniBelirle(){ 
        AyniMeyvePerleri.Clear();
        Dictionary<int, GameObject> benzerRakamGrubu = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < CepList.Count; i++) {
            Cep cepInstance = CepList[i];
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (cepInstance.TasInstance) {
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (benzerRakamGrubu.Count == 0) {
                    benzerRakamGrubu.Add(i, cepInstance.TasInstance.gameObject);
                }

                // sonraki ile ardışık mı ?
                try {
                    Cep sonrakiCepInstance = CepList[i + 1];
                    if (cepInstance.TasInstance.MeyveID == sonrakiCepInstance?.TasInstance?.MeyveID) {
                        benzerRakamGrubu.Add(i + 1, sonrakiCepInstance.TasInstance.gameObject);
                    }
                    else {
                        _yeniGrupOlustur = true;
                    }
                }
                catch (Exception e) { 
                    Debug.Log(e.Message);
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur) {
                if (benzerRakamGrubu.Count > 2) {
                    AyniMeyvePerleri.Add(new Dictionary<int, GameObject>(benzerRakamGrubu));
                }

                benzerRakamGrubu.Clear();
            }
        } // for end 
    }

    public void AyniMeyveGruplarinIcindekiAyniRenkPerleriniBelirle(){
        AyniMeyveAyniRenkPerleri.Clear();
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < AyniMeyvePerleri.Count; i++) {
            var grup = AyniMeyvePerleri[i];
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
                        if (TasManeger.Instance.TasInstances[tas].renk ==
                            TasManeger.Instance.TasInstances[sonrakiTasA].renk) {
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
                        AyniMeyveAyniRenkPerleri.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for
    }

    public void AyniMeyveGruplarinIcindekiFarkliRenkPerleriniBelirle(){
        AyniMeyveFarkliRenkPerleri.Clear();
        Dictionary<int, GameObject> farkliRenklilerGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < AyniMeyvePerleri.Count; i++) {
            var grup = AyniMeyvePerleri[i];
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
                            if (TasManeger.Instance.TasInstances[sonrakiTasA].renk
                                == TasManeger.Instance.TasInstances[item.Value].renk) { 
                                if (item.Key != sonrakiKey-1){
                                    j--;
                                }
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
                        AyniMeyveFarkliRenkPerleri.Add(new Dictionary<int, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 
    }
    
    public void PerleriTemizle(){
        // SiraliRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < FarkliMeyveAyniRenkPerleri.Count; i++) {
            var grup = FarkliMeyveAyniRenkPerleri[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
        // AyniRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < AyniMeyveAyniRenkPerleri.Count; i++) {
            var grup = AyniMeyveAyniRenkPerleri[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
    
        // AyniRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < AyniMeyveFarkliRenkPerleri.Count; i++) {
            var grup = AyniMeyveFarkliRenkPerleri[i];
            foreach (var item in grup) {
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }
    
 
        FarkliMeyveAyniRenkPerleri.Clear();
        AyniMeyvePerleri.Clear();
        FarkliMeyvePerleri.Clear();
        AyniMeyveFarkliRenkPerleri.Clear();
    }

    public void YildizleriGizle(){
        foreach (var cep in CepList){
            cep.YildiziYak(0);
        }
    }
}
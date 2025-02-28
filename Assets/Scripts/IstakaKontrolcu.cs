using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IstakaKontrolcu : MonoBehaviour{
    public static IstakaKontrolcu Instance;
    public List<Dictionary<int, GameObject>> SiraliGruplar = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> BenzerRakamGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> SiraliRakamAyniRenkGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniRakamAyniRenkGruplari = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniRakamHepsiFarkliRenkGruplari = new List<Dictionary<int, GameObject>>();

    public List<Dictionary<int, GameObject>> SiraliRakamHepsiFarkliRenkGruplari =
        new List<Dictionary<int, GameObject>>();
    
    private bool _yeniGrupOlustur;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public void SiraliGruplariBelirle(){
        SiraliGruplar.Clear(); 
        Dictionary<int, GameObject> siraliGrup = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < Istaka.Instance.CepSayisi; i++){
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (Istaka.Instance.Taslar.TryGetValue(i, out var tas)){
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (siraliGrup.Count == 0){
                    siraliGrup.Add(i, tas);
                }

                // sonraki ile ardışık mı ?
                if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTas)){
                    int sonrakiTasinRakami = Istaka.Instance.TasinRakami[i + 1];
                    int simdikiTasinRakami = Istaka.Instance.TasinRakami[i];
                    if (simdikiTasinRakami == sonrakiTasinRakami - 1){
                        // sonrakiyle ardışıksa sonrakinide ekleyelim
                        siraliGrup.Add(i + 1, sonrakiTas);
                    }
                    else{
                        _yeniGrupOlustur = true;
                    }
                }
                else{
                    // sonrraki yoksa yeni grup olustur
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur){
                // önceki üç taşı kontrole delim per mi ? 
                if (siraliGrup.Count > 2){
                    if (Istaka.Instance.Taslar.TryGetValue(i - (siraliGrup.Count + 1), out var ardisikIlkTas)){
                        if (Istaka.Instance.Taslar.TryGetValue(i - (siraliGrup.Count), out var oncekiTasB)){
                            if (Istaka.Instance.Taslar.TryGetValue(i - (siraliGrup.Count - 1), out var oncekiTasC)){
                                if (Istaka.Instance.TasinRakami[i - (siraliGrup.Count + 1)] ==
                                    Istaka.Instance.TasinRakami[i - (siraliGrup.Count)]){
                                    if (Istaka.Instance.TasinRakami[i - (siraliGrup.Count)] ==
                                        Istaka.Instance.TasinRakami[i - (siraliGrup.Count - 1)]){
                                        //SiraliGrup.Remove(SiraliGrup.Keys.First());
                                    }
                                }
                            }
                        }
                    }
                }

                if (siraliGrup.Count > 2){
                    if (siraliGrup.Count > 3){
                        // sonraki iki cep varmı ? varsa aynı rakammı ?
                        bool sonrakiIkitaneVar = false; 
                        if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTasA)){
                            if (Istaka.Instance.Taslar.TryGetValue(i + 2, out var sonrakiTasB)){
                                sonrakiIkitaneVar = true;
                            }
                        }

                        if (sonrakiIkitaneVar){
                            if (Istaka.Instance.TasinRakami[i] == Istaka.Instance.TasinRakami[i + 1]){
                                if (Istaka.Instance.TasinRakami[i + 1] == Istaka.Instance.TasinRakami[i + 2]){
                                    //SiraliGrup.Remove(SiraliGrup.Keys.Last());
                                }
                            }
                        }
                    }

                    SiraliGruplar.Add(new Dictionary<int, GameObject>(siraliGrup));
                }

                siraliGrup.Clear();
            }
        } // for end 

        if (SiraliGruplar.Count > 0){
            print($"SiraliGruplar: {SiraliGruplar.Count}");
        }
    }

    public void BenzerRakamGruplariniBelirle(){
        BenzerRakamGruplari.Clear(); 
        Dictionary<int, GameObject> benzerRakamGrubu = new Dictionary<int, GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < Istaka.Instance.CepSayisi; i++){
            _yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (Istaka.Instance.Taslar.TryGetValue(i, out var tas)){
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (benzerRakamGrubu.Count == 0){
                    benzerRakamGrubu.Add(i, tas);
                }

                // sonraki ile aynımı mı ?
                if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTas)){
                    int sonrakiTasinRakami = Istaka.Instance.TasinRakami[i + 1];
                    int simdikiTasinRakami = Istaka.Instance.TasinRakami[i];
                    if (simdikiTasinRakami == sonrakiTasinRakami){
                        // sonrakiyle aynıysa sonrakinide ekleyelim
                        benzerRakamGrubu.Add(i + 1, sonrakiTas);
                    }
                    else{
                        _yeniGrupOlustur = true;
                    }
                }
                else{
                    // sonrraki yoksa yeni grup olustur
                    _yeniGrupOlustur = true;
                }
            }

            if (_yeniGrupOlustur){
                if (benzerRakamGrubu.Count > 2){
                    BenzerRakamGruplari.Add(new Dictionary<int, GameObject>(benzerRakamGrubu));
                }

                benzerRakamGrubu.Clear();
            }
        } // for end 

        if (BenzerRakamGruplari.Count > 0){
            print($"BenzerRakamGruplari: {BenzerRakamGruplari.Count}");
        }
    }

    public void SiraliGruplarinIcindekiRenkGruplariniBelirle(){
        SiraliRakamAyniRenkGruplari.Clear(); 
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++){
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++){
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                if (renkGrubu.Count == 0){
                    renkGrubu.Add(key, tas);
                }

                //sonraki var mı ?  
                if (j + 1 < grup.Keys.ToList().Count){
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)){
                        // rengi aynı mı ?
                        if (tas.GetComponent<Tas>().Renk ==
                            sonrakiTasA.GetComponent<Tas>().Renk){
                            renkGrubu.Add(grup.Keys.ToList()[j + 1], sonrakiTasA);
                        }
                        else{
                            _yeniGrupOlustur = true;
                        }
                    }
                }
                else{
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur){
                    if (renkGrubu.Count > 2){
                        SiraliRakamAyniRenkGruplari.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for

        if (SiraliRakamAyniRenkGruplari.Count > 0){
            print($"SiraliRakamRenkGruplari: {SiraliRakamAyniRenkGruplari.Count}");
        }
    }

    public void AyniRakamGruplarinIcindekiRenkGruplariniBelirle(){
        AyniRakamAyniRenkGruplari.Clear(); 
        Dictionary<int, GameObject> renkGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < BenzerRakamGruplari.Count; i++){
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++){
                _yeniGrupOlustur = false;
                var key = grup.Keys.ToList()[j];
                var tas = grup[key];
                if (renkGrubu.Count == 0){
                    renkGrubu.Add(key, tas);
                }

                //sonraki var mı ?  
                if (j + 1 < grup.Keys.ToList().Count){
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)){
                        // rengi aynı mı ?
                        if (tas.GetComponent<Tas>().Renk ==
                            sonrakiTasA.GetComponent<Tas>().Renk){
                            renkGrubu.Add(grup.Keys.ToList()[j + 1], sonrakiTasA);
                        }
                        else{
                            _yeniGrupOlustur = true;
                        }
                    }
                }
                else{
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur){
                    if (renkGrubu.Count > 2){
                        AyniRakamAyniRenkGruplari.Add(new Dictionary<int, GameObject>(renkGrubu));
                    }

                    renkGrubu.Clear();
                }
            } // end for 
        } // end for

        if (AyniRakamAyniRenkGruplari.Count > 0){
            print($"AyniRakamRenkGruplari: {AyniRakamAyniRenkGruplari.Count}");
        }
    }

    public void AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        AyniRakamHepsiFarkliRenkGruplari.Clear(); 
        Dictionary<int, GameObject> farkliRenklilerGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < BenzerRakamGruplari.Count; i++){
            var grup = BenzerRakamGruplari[i];
            for (int j = 0; j < grup.Count; j++){
                _yeniGrupOlustur = false;
                int key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0){
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ?
                if (j + 1 < grup.Keys.ToList().Count){
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)){
                        foreach (var item in farkliRenklilerGrubu){
                            if (sonrakiTasA.GetComponent<Tas>().Renk
                                == item.Value.GetComponent<Tas>().Renk){
                                //aynı renk zaten var
                                _yeniGrupOlustur = true;
                                break;
                            }
                        }

                        if (!_yeniGrupOlustur){
                            farkliRenklilerGrubu.Add(sonrakiKey, sonrakiTasA);
                        }
                    }
                }
                else{
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur){
                    if (farkliRenklilerGrubu.Count > 2){
                        AyniRakamHepsiFarkliRenkGruplari.Add(new Dictionary<int, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 

        if (AyniRakamHepsiFarkliRenkGruplari.Count > 0){
            print($"AyniRakamHepsiFarkliRenkGruplari: {AyniRakamHepsiFarkliRenkGruplari.Count}");
        }
    }

    public void SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(){
        SiraliRakamHepsiFarkliRenkGruplari.Clear(); 
        Dictionary<int, GameObject> farkliRenklilerGrubu = new Dictionary<int, GameObject>();
        for (int i = 0; i < SiraliGruplar.Count; i++){
            var grup = SiraliGruplar[i];
            for (int j = 0; j < grup.Count; j++){
                _yeniGrupOlustur = false;
                int key = grup.Keys.ToList()[j];
                var tas = grup[key];
                // grup yeni grupsa  bakamdan gruba ekleyelim
                if (farkliRenklilerGrubu.Count == 0){
                    farkliRenklilerGrubu.Add(key, tas);
                }

                // sonraki var mi ?
                if (j + 1 < grup.Keys.ToList().Count){
                    var sonrakiKey = grup.Keys.ToList()[j + 1];
                    if (grup.TryGetValue(sonrakiKey, out var sonrakiTasA)){
                        foreach (var item in farkliRenklilerGrubu){
                            if (sonrakiTasA.GetComponent<Tas>().Renk
                                == item.Value.GetComponent<Tas>().Renk){
                                //aynı renk zaten var
                                _yeniGrupOlustur = true;
                                break;
                            }
                        }

                        if (!_yeniGrupOlustur){
                            farkliRenklilerGrubu.Add(sonrakiKey, sonrakiTasA);
                        }
                    }
                }
                else{
                    _yeniGrupOlustur = true;
                }

                if (_yeniGrupOlustur){
                    if (farkliRenklilerGrubu.Count > 2){
                        SiraliRakamHepsiFarkliRenkGruplari.Add(new Dictionary<int, GameObject>(farkliRenklilerGrubu));
                    }

                    farkliRenklilerGrubu.Clear();
                }
            } // end for 
        } // end for 

        if (SiraliRakamHepsiFarkliRenkGruplari.Count > 0){
            print($"SiraliRakamHepsiFarkliRenkGruplari: {SiraliRakamHepsiFarkliRenkGruplari.Count}");
        }
    }

    public void GruplariTemizle(){
        // SiraliRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamAyniRenkGruplari.Count; i++){
            var grup = SiraliRakamAyniRenkGruplari[i];
            foreach (var item in grup){
                //Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            }
        }
        
        // AyniRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamAyniRenkGruplari.Count; i++){
            var grup = AyniRakamAyniRenkGruplari[i];
            foreach (var item in grup){
                //Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            }
        }


        // AyniRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < AyniRakamHepsiFarkliRenkGruplari.Count; i++){
            var grup = AyniRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup){
                //Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            }
        }

        // SiraliRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < SiraliRakamHepsiFarkliRenkGruplari.Count; i++){
            var grup = SiraliRakamHepsiFarkliRenkGruplari[i];
            foreach (var item in grup){
                //Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            }
        }

        SiraliRakamHepsiFarkliRenkGruplari.Clear();
        SiraliRakamAyniRenkGruplari.Clear();
        BenzerRakamGruplari.Clear();
        SiraliGruplar.Clear();
        AyniRakamHepsiFarkliRenkGruplari.Clear();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IstakaKontrolcu : MonoBehaviour{
    public static IstakaKontrolcu Instance;
    public List<Dictionary<int,GameObject>> SiraliGruplar = new List<Dictionary<int,GameObject>>();
    public List<Dictionary<int,GameObject>> BenzerRakamGruplari = new List<Dictionary<int,GameObject>>();

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
        bool yeniGrupOlustur = false;
        Dictionary<int,GameObject> SiraliGrup = new Dictionary<int,GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < Istaka.Instance.CepSayisi; i++){
            yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (Istaka.Instance.Taslar.TryGetValue(i, out var tas)){
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (SiraliGrup.Count == 0){
                    SiraliGrup.Add(i,tas);
                }

                // sonraki ile ardışık mı ?
                if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTas)){
                    int sonrakiTasinRakami = Istaka.Instance.TasinRakami[i + 1];
                    int simdikiTasinRakami = Istaka.Instance.TasinRakami[i];
                    if (simdikiTasinRakami == sonrakiTasinRakami - 1){
                        // sonrakiyle ardışıksa sonrakinide ekleyelim
                        SiraliGrup.Add(i+1,sonrakiTas);
                    }
                    else{
                        yeniGrupOlustur = true;
                    }
                }
                else{
                    // sonrraki yoksa yeni grup olustur
                    yeniGrupOlustur = true;
                }
            }

            if (yeniGrupOlustur){
                // önceki üç taşı kontrole delim per mi ? 
                if (SiraliGrup.Count > 2){
                    if (Istaka.Instance.Taslar.TryGetValue(i - (SiraliGrup.Count+1), out var ardisikIlkTas)){
                        if (Istaka.Instance.Taslar.TryGetValue(i - (SiraliGrup.Count), out var oncekiTasB)){
                            if (Istaka.Instance.Taslar.TryGetValue(i - (SiraliGrup.Count - 1), out var oncekiTasC)){
                                if (Istaka.Instance.TasinRakami[i - (SiraliGrup.Count+1)] ==
                                    Istaka.Instance.TasinRakami[i - (SiraliGrup.Count)]){
                                    if (Istaka.Instance.TasinRakami[i - (SiraliGrup.Count)] ==
                                        Istaka.Instance.TasinRakami[i - (SiraliGrup.Count - 1)]){
                                        SiraliGrup.Remove(SiraliGrup.Keys.First());
                                    }
                                }
                            }
                        }
                    }
                }

                if (SiraliGrup.Count > 2){
                    if (SiraliGrup.Count > 3){
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
                                    // sonraki ikitane nin rakamları aynıysa
                                    SiraliGrup.Remove(SiraliGrup.Keys.Last());
                                }
                            }
                        }
                    }

                    SiraliGruplar.Add(new Dictionary<int,GameObject>(SiraliGrup));
                }

                SiraliGrup.Clear();
            }
        } // for end 
 
    }

    public void AyniRakamGruplariniBelirle(){
        BenzerRakamGruplari.Clear();
        bool yeniGrupOlustur = false;
        Dictionary<int,GameObject> BenzerRakamGrubu = new Dictionary<int,GameObject>();
        // Istakadaki cepleri tek tek kontrole delim.
        for (int i = 0; i < Istaka.Instance.CepSayisi; i++){
            yeniGrupOlustur = false;
            //Cepte taş var mı ?
            if (Istaka.Instance.Taslar.TryGetValue(i, out var tas)){
                // grup yeni grupsa ardışıklığına bakamdan gruba ekleyelim
                if (BenzerRakamGrubu.Count == 0){
                    BenzerRakamGrubu.Add(i,tas);
                }

                // sonraki ile aynımı mı ?
                if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTas)){
                    int sonrakiTasinRakami = Istaka.Instance.TasinRakami[i + 1];
                    int simdikiTasinRakami = Istaka.Instance.TasinRakami[i];
                    if (simdikiTasinRakami == sonrakiTasinRakami){
                        // sonrakiyle aynıysa sonrakinide ekleyelim
                        BenzerRakamGrubu.Add(i+1,sonrakiTas);
                    }
                    else{
                        yeniGrupOlustur = true;
                    }
                }
                else{
                    // sonrraki yoksa yeni grup olustur
                    yeniGrupOlustur = true;
                }
            }

            if (yeniGrupOlustur){
                if (BenzerRakamGrubu.Count > 2){
                    // eğer grup üten fazlaysa son elemanı sonraki aynı renk grubunun elemanı olması gereke bilir.
                    // if (BenzerRakamGrubu.Count > 3){
                    //     // sonraki iki cep varmı ? varsa ardışık rakammı ?
                    //     bool sonrakiIkitaneVar = false;
                    //     if (Istaka.Instance.Taslar.TryGetValue(i + 1, out var sonrakiTasA)){
                    //         if (Istaka.Instance.Taslar.TryGetValue(i + 2, out var sonrakiTasB)){
                    //             sonrakiIkitaneVar = true;
                    //         }
                    //     }
                    //
                    //     if (sonrakiIkitaneVar){
                    //         if (Istaka.Instance.TasinRakami[i] == Istaka.Instance.TasinRakami[i + 1] - 1){
                    //             if (Istaka.Instance.TasinRakami[i + 1] == Istaka.Instance.TasinRakami[i + 2] - 1){
                    //                 // sonraki iki tane nin rakamları ardışıksa
                    //                 BenzerRakamGrubu.RemoveAt(BenzerRakamGrubu.Count - 1);
                    //             }
                    //         } 
                    //     }
                    // }

                    BenzerRakamGruplari.Add(new Dictionary<int,GameObject>(BenzerRakamGrubu));
                }

                BenzerRakamGrubu.Clear();
            }
        } // for end 
 
    }
    
    public void GruplariTemizle(){
        // sıralı grupları temizle
        for (int i = 0; i < SiraliGruplar.Count; i++){
            var grup = SiraliGruplar[i];
            foreach (var item in grup){
                Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            } 
        }
        SiraliGruplar.Clear();
        
        // aynı rakamlı grupları temizle
        for (int i = 0; i < BenzerRakamGruplari.Count; i++){
            var grup = BenzerRakamGruplari[i];
            foreach (var item in grup){
                Destroy(item.Value);
                Istaka.Instance.TasinRakami.Remove(item.Key);
                Istaka.Instance.Taslar.Remove(item.Key);
                Istaka.Instance.CepList[item.Key].GetComponent<IstakaCebi>().Dolu = false;
            } 
        }
        BenzerRakamGruplari.Clear();
        
        foreach (var item in Istaka.Instance.TasinRakami){
            Debug.Log($"TasinRakami Key: {item.Key} Value: {item.Value}");
        }
        
        foreach (var item in Istaka.Instance.CepList){
            Debug.Log($" Cebin Doluluğu : {item.GetComponent<IstakaCebi>().Dolu}");
        }
        
        foreach (var item in Istaka.Instance.Taslar){
            Debug.Log($"Taslar Key: {item.Key} Rakam: {item.Value.GetComponent<Kare>().Rakam}");
        }
        
    }
}
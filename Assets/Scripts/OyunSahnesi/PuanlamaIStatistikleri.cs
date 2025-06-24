using UnityEngine;

public static class PuanlamaIStatistikleri{
    public static int Puan = 0;
    public static int HamleSayisi = 0; 
    public static int Zaman = 0;
    public static int CanSayisi = 0;
    public static int YokEdilenTasSayisi = 0;
    public static int GorevSayisi = 0;
    public static int AltinSayisi = 0;
    public static int ElmsSayisi = 0;

    public static void Sakla(){
        AltinVeElmaslariPuanla();
        BonuslariPuanla();
        PerdekiTaslariPuanla();
        OyunSahnesiUI.Instance.SkorTxt.text = Puan.ToString();
        Debug.Log("Puan: " + Puan);
        Debug.Log("AltinSayisi: " + AltinSayisi);
        Debug.Log("ElmsSayisi: " + ElmsSayisi);
    }

    private static void PerdekiTaslariPuanla(){
        for (int i = 0; i < Istaka.Instance.CepList.Count; i++){
            var cep = Istaka.Instance.CepList[i];
            if (!cep.TasInstance) continue;
            if (Istaka.Instance.CepList[i].TasInstance.PereUyumluGostergesi.activeSelf ){
                Puan++;
            }
        }
    }

    private static void BonuslariPuanla(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasscript = TasManeger.Instance.TasInstances[cTas];  
            if (cTasscript.BonusBayragi){
                Puan++;
            } 
        }
    }

    private static void AltinVeElmaslariPuanla(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasscript = TasManeger.Instance.TasInstances[cTas]; 
            // altı ise
            if (cTasscript.GorevleUyumBayragi == 1){
                AltinSayisi++;
            }
            // elmas ise
            else if (cTasscript.GorevleUyumBayragi == 2){
                ElmsSayisi++;
            }
        }
    }
}
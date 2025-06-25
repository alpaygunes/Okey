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
    public static int ToplamTasSayisi{ get; set; } = 0;

    public static void Sifirla(){
        Puan = 0;
        HamleSayisi = 0; 
        Zaman = 0;
        CanSayisi = 0;
        YokEdilenTasSayisi = 0;
        GorevSayisi = 0;
        AltinSayisi = 0;
        ElmsSayisi = 0;
        ToplamTasSayisi = 0;
    }

    public static void Sakla(){
        AltinVeElmaslariPuanla();
        BonuslariPuanla();
        PerdekiTaslariPuanla();
        CanSayisi = GameManager.Instance.CanSayisi;
        
        OyunSahnesiUI.Instance.SkorTxt.text = Puan.ToString();
        OyunSahnesiUI.Instance.AltinSayisi.text = AltinSayisi.ToString();
        OyunSahnesiUI.Instance.ElmasSayisi.text = ElmsSayisi.ToString();
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
            if (cTasscript.GorevUyumGostergesi1.gameObject.activeSelf){
                AltinSayisi++;
            }
            // elmas ise
            else if (cTasscript.GorevUyumGostergesi2.gameObject.activeSelf){
                ElmsSayisi++;
            }
        }
    }
}
using UnityEngine;

public static class LevelManager {
    public static void Init() {
        RenkVeMeyveSeviyesi();
        AyarlariUygula();
        if (PlayerPrefs.GetInt("RenkSeviyesi")>=9) {
            GameManager.Instance.ColonCount = 6;
            GameManager.Instance.CepSayisi = 6;
        } else if (PlayerPrefs.GetInt("RenkSeviyesi")>=5) {
            GameManager.Instance.ColonCount = 5;
            GameManager.Instance.CepSayisi = 5;
        }      
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();
    }

    public static void RenkSayisiniArtir() {
        var renkSeviyesi = PlayerPrefs.GetInt("RenkSeviyesi");
        renkSeviyesi++;
        PlayerPrefs.SetInt("RenkSeviyesi", renkSeviyesi); 
    }
    
    private static void RenkVeMeyveSeviyesi() {
        
        var RenkSayisi = Renkler.RenkSozlugu.Count;
        var MeyveSayisi = Resources.LoadAll<Sprite>("Images/Meyveler").Length;
        var RenkSeviyesi = PlayerPrefs.GetInt("RenkSeviyesi");
        var MeyveSeviyesi = PlayerPrefs.GetInt("MeyveSeviyesi");
        
        if (MeyveSeviyesi >= MeyveSayisi && RenkSayisi>= RenkSeviyesi) {
            return; // zeten son safhaya ulaşmış
        }
        
        // seviye veriler yoksa oluştur             ---------------------
        if (!PlayerPrefs.HasKey("RenkSeviyesi") 
            || RenkSeviyesi==0) RenkSeviyesi = 3;
        if (!PlayerPrefs.HasKey("MeyveSeviyesi") 
            || MeyveSeviyesi==0) MeyveSeviyesi = 3;
        // -------   seviye veriler yoksa oluştur    END ------
        
        if (RenkSeviyesi >= RenkSayisi) {
            RenkSeviyesi = 3;
            MeyveSeviyesi++;
        }
        if (MeyveSeviyesi >= MeyveSayisi) MeyveSeviyesi = 0;
        
        PlayerPrefs.SetInt("RenkSeviyesi", RenkSeviyesi);
        PlayerPrefs.SetInt("MeyveSeviyesi", MeyveSeviyesi); 
    }

    public static void AyarlariUygula() {
        GameManager.Instance.RenkAraligi = new RangeInt (0,PlayerPrefs.GetInt("RenkSeviyesi"));
        GameManager.Instance.MeyveAraligi= new RangeInt (0,PlayerPrefs.GetInt("MeyveSeviyesi"));
        Debug.Log($"RenkSeviyesi: {PlayerPrefs.GetInt("RenkSeviyesi")}  " +
                  $"--  MeyveSeviyesi: {PlayerPrefs.GetInt("MeyveSeviyesi")}");
    }
}
using System.Collections.Generic;
using UnityEngine;

public struct Level {
    public List<string> Kalip { get; private set; }
    public bool KilitliKutu { get; private set; }
    public bool HareketsizKutu { get; private set; }
    public bool OdulKutus { get; private set; }
    public int RenkSayisi { get; private set; }
    public int MeyveSayisi { get; private set; }
    public int LevelGecmePuani { get; set; }

    public Level(List<string> kalip, bool kilitliKutu, bool hareketsizKutu, bool odulKutus, int renkSayisi, int meyveSayisi, int levelGecmePuani)
    {
        Kalip = kalip;
        KilitliKutu = kilitliKutu;
        HareketsizKutu = hareketsizKutu;
        OdulKutus = odulKutus;
        RenkSayisi = renkSayisi;
        MeyveSayisi = meyveSayisi;
        LevelGecmePuani = levelGecmePuani;
    }
}

public static class GameLevels {
    public static Dictionary<int,Level> Levels = new Dictionary<int, Level>();

    static GameLevels() { 
        Levels.Add(0, new Level(null,
            false, false, false, 4, 4,1000));
        Levels.Add(1, new Level(null,
            false, false, false, 4, 4,1000));
        Levels.Add(2, new Level(null, 
            false, false, false, 4, 5,1000));
        Levels.Add(3, new Level(null, 
            false, true, false, 5, 5,1000));
        Levels.Add(4, new Level(null, 
            false, true, true, 6, 5,1000));
        Levels.Add(5, new Level(null, 
            false, true, true, 6, 6,1000)); 
        Levels.Add(6, new Level(new List<string> {  "0", "1", "2" ,"3" , "4" }, 
            true, false, false, 4, 4,1000));  
        Levels.Add(7, new Level(new List<string> {  "5", "6", "7" ,"8" , "9" }, 
            true, false, false, 4, 4,1000));  
        Levels.Add(8, new Level(new List<string> {  "A", "B", "C" ,"D" , "E" }, 
            true, true, false, 4, 4,1000));   
        Levels.Add(9, new Level(new List<string> {  "F", "G", "H" ,"I" , "i" }, 
            true, true, false, 4, 4,1000));   
        Levels.Add(10, new Level(new List<string> {  "J", "K", "L" ,"M" , "N" }, 
            true, true, false, 4, 5,1000));  
        Levels.Add(11, new Level(new List<string> {  "O", "P" ,"R" , "S" }, 
            true, true, false, 4, 5,1000));
    }

    public static Level GetLevel() {
        var levelID = PlayerPrefs.GetInt("GamePlayLevelID");
        return Levels[levelID];
    }

    public static void SetLevel() {
        if (!LevelAtlandimi()) return;
        
        var RecordLevelID = PlayerPrefs.GetInt("RecordLevelID");
        var GamePlayLevelID = PlayerPrefs.GetInt("GamePlayLevelID");
        var OynananMeyveNo = PlayerPrefs.GetInt("OynananMeyveNo");
        var OynananRenkNo = PlayerPrefs.GetInt("OynananRenkNo"); 
        var OynananKalipNo = PlayerPrefs.GetInt("OynananKalipNo");
        var AsilanLimitAdi = "";
        
        OynananMeyveNo++; 
        AsilanLimitAdi = "Meyve";
        PlayerPrefs.SetInt("OynananMeyveNo", OynananMeyveNo); 
        
        // Meyve No Limiti Aştıysa
        if (OynananMeyveNo >= GetLevel().MeyveSayisi) {  
            Debug.Log("OYNANAN MEYVENO limiti aşarsa yeni oyun başlamalı"); 
            OynananMeyveNo = 3;
            PlayerPrefs.SetInt("OynananMeyveNo", OynananMeyveNo); 
            OynananRenkNo++;
            PlayerPrefs.SetInt("OynananRenkNo", OynananRenkNo); 
            AsilanLimitAdi = "Renk";
            
            // Renk No Limiti Aştıysa
            if (OynananRenkNo >= GetLevel().RenkSayisi) { 
                Debug.Log("OYNANAN RENKNO limiti aşarsa yeni oyun başlamalı");
                PlayerPrefs.SetInt("RecordLevelID",RecordLevelID);  
                OynananRenkNo = 3;
                PlayerPrefs.SetInt("OynananRenkNo", OynananRenkNo);
                OynananKalipNo++;
                AsilanLimitAdi = "Kalip";
                PlayerPrefs.SetInt("OynananKalipNo", OynananKalipNo); 
                var kalip_sayisi = GetLevel().Kalip !=null ? GetLevel().Kalip.Count : 0 ;
                if (OynananKalipNo >= kalip_sayisi) {   
                    Debug.Log("OYNANAN KALIP limiti aşarsa yeni oyun başlamalı");
                    OynananKalipNo = 0;
                    PlayerPrefs.SetInt("OynananKalipNo", OynananKalipNo);
                    GamePlayLevelID++;
                    PlayerPrefs.SetInt("GamePlayLevelID",GamePlayLevelID); 

                    if (GamePlayLevelID>RecordLevelID) {
                        RecordLevelID++;
                        PlayerPrefs.SetInt("RecordLevelID",RecordLevelID);
                    }
                } 
            }
        }
        
        // hangi limit aşıldıysa ona göre tepki olsun
        GameManager.Instance.basariPopUplari.Goster(AsilanLimitAdi);  
    }

    private static bool LevelAtlandimi() {
        var BonusMeyveSayisi = PuanlamaIStatistikleri.BonusMeyveSayisi;
        var AltinSayisi = PuanlamaIStatistikleri.AltinSayisi; 
        var ElmasSayisi = PuanlamaIStatistikleri.ElmasSayisi; 
        var ToplananPuan = 0; 
        var LevelGecmePuani = GetLevel().LevelGecmePuani;
        
        ToplananPuan += BonusMeyveSayisi;
        ToplananPuan += AltinSayisi*4;
        ToplananPuan += ElmasSayisi*10;
        Debug.Log($"ToplananPuan : {ToplananPuan}");
        return ToplananPuan >= LevelGecmePuani;
    }
}
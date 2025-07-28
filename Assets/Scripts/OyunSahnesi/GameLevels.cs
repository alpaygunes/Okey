using System.Collections.Generic;
using UnityEngine;

public struct Level {
    public List<string> Kalip { get; private set; }
    public bool KilitliKutu { get; private set; }
    public bool HareketsizKutu { get; private set; }
    public bool OdulKutus { get; private set; }
    public int RenkSayisi { get; private set; }
    public int MeyveSayisi { get; private set; }

    public Level(List<string> kalip, bool kilitliKutu, bool hareketsizKutu, bool odulKutus, int renkSayisi, int meyveSayisi)
    {
        Kalip = kalip;
        KilitliKutu = kilitliKutu;
        HareketsizKutu = hareketsizKutu;
        OdulKutus = odulKutus;
        RenkSayisi = renkSayisi;
        MeyveSayisi = meyveSayisi;
    }
}

public static class GameLevels {
    public static Dictionary<int,Level> Levels = new Dictionary<int, Level>();

    static GameLevels() { 
        Levels.Add(0, new Level(null,
            false, false, false, 4, 4));
        Levels.Add(1, new Level(null,
            false, false, false, 4, 4));
        Levels.Add(2, new Level(null, 
            false, false, false, 4, 5));
        Levels.Add(3, new Level(null, 
            false, true, false, 5, 5));
        Levels.Add(4, new Level(null, 
            false, true, true, 6, 5));
        Levels.Add(5, new Level(null, 
            false, true, true, 6, 6)); 
        Levels.Add(6, new Level(new List<string> {  "0", "1", "2" ,"3" , "4" }, 
            true, false, false, 4, 4));  
        Levels.Add(7, new Level(new List<string> {  "5", "6", "7" ,"8" , "9" }, 
            true, false, false, 4, 4));  
        Levels.Add(8, new Level(new List<string> {  "A", "B", "C" ,"D" , "E" }, 
            true, true, false, 4, 4));   
        Levels.Add(9, new Level(new List<string> {  "F", "G", "H" ,"I" , "i" }, 
            true, true, false, 4, 4));   
        Levels.Add(10, new Level(new List<string> {  "J", "K", "L" ,"M" , "N" }, 
            true, true, false, 4, 5));  
        Levels.Add(11, new Level(new List<string> {  "O", "P" ,"R" , "S" }, 
            true, true, false, 4, 5));
    }

    public static Level getLevel() {
        var levelID = PlayerPrefs.GetInt("GamePlayLevelID");
        return Levels[levelID];
    }
 
}
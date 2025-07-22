using System.Collections.Generic;

public struct Level {
    public List<string> Kalip { get; private set; }
    public bool KilitliKutu { get; private set; }
    public bool HareketsizKutu { get; private set; }
    public bool OdulKutus { get; private set; }
    public int RenkLimiti { get; private set; }
    public int MeyveLimiti { get; private set; }

    public Level(List<string> kalip, bool kilitliKutu, bool hareketsizKutu, bool odulKutus, int renkLimiti, int meyveLimiti)
    {
        Kalip = kalip;
        KilitliKutu = kilitliKutu;
        HareketsizKutu = hareketsizKutu;
        OdulKutus = odulKutus;
        RenkLimiti = renkLimiti;
        MeyveLimiti = meyveLimiti;
    }
}

public static class GameLevels {
    private static Dictionary<int,Level> Levels = new Dictionary<int, Level>();

    static GameLevels() { 
        Levels.Add(1, new Level(null,
            false, false, false, 3, 3));
        Levels.Add(2, new Level(null, 
            false, false, false, 3, 4));
        Levels.Add(3, new Level(null, 
            false, true, false, 4, 4));
        Levels.Add(4, new Level(null, 
            false, true, true, 5, 4));
        Levels.Add(5, new Level(null, 
            false, true, true, 5, 5)); 
        Levels.Add(6, new Level(new List<string> {  "0", "1", "2" ,"3" , "4" }, 
            true, false, false, 3, 3));  
        Levels.Add(7, new Level(new List<string> {  "5", "6", "7" ,"8" , "9" }, 
            true, false, false, 3, 3));  
        Levels.Add(8, new Level(new List<string> {  "A", "B", "C" ,"D" , "E" }, 
            true, true, false, 3, 3));   
    }
    
}
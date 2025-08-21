using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct Level
{
    public List<string> Kalip { get; private set; }
    public bool KilitliKutu { get; private set; }
    public bool HareketsizKutu { get; private set; }
    public bool OdulKutus { get; private set; }
    public int RenkStart { get; private set; }
    public int RenkEnd { get; private set; }
    public int MeyveStart { get; private set; }
    public int MeyveEnd { get; private set; }
    public int LevelGecmePuani { get; set; }
    public int HamleLimiti { get; set; }

    public Level(List<string> kalip, bool kilitliKutu, bool hareketsizKutu, bool odulKutus,int renkStart, int renkEnd,
        int meyveStart ,int meyveEnd, int hamleLimiti, int levelGecmePuani)
    {
        Kalip = kalip;
        KilitliKutu = kilitliKutu;
        HareketsizKutu = hareketsizKutu;
        OdulKutus = odulKutus;
        RenkStart = renkStart;
        RenkEnd = renkEnd;
        MeyveEnd = meyveEnd;
        MeyveStart = meyveStart;
        LevelGecmePuani = levelGecmePuani;
        HamleLimiti = hamleLimiti;
    }
}

public static class GameLevels
{
    public static Dictionary<int, Level> Levels = new Dictionary<int, Level>();

    static GameLevels()
    {
        Levels.Add(0, new Level(null,
            false, false, false, 1,2,1, 2, 10, 800));
        Levels.Add(1, new Level(null,
            false, false, false, 1,2,1, 2, 10, 800));
        Levels.Add(2, new Level(null,
            false, false, false, 2,4,2, 5, 10, 800));
        Levels.Add(3, new Level(null,
            false, true, false, 2,5,2, 5, 10, 800));
        Levels.Add(4, new Level(null,
            false, true, true, 2,6,2, 5, 10, 800));
        Levels.Add(5, new Level(null,
            false, true, true, 2,6,2, 6, 10, 800));
        Levels.Add(6, new Level(new List<string> { "0", "1", "2", "3", "4" },
            true, false, false, 2,4,2, 4, 10, 800));
        Levels.Add(7, new Level(new List<string> { "5", "6", "7", "8", "9" },
            true, false, false, 2,8,2, 8, 10, 800));
        Levels.Add(8, new Level(new List<string> { "A", "B", "C", "D", "E" },
            true, true, false, 2,4,2, 4, 10, 800));
        Levels.Add(9, new Level(new List<string> { "F", "G", "H", "I", "i" },
            true, true, false, 2,4,2, 4, 10, 800));
        Levels.Add(10, new Level(new List<string> { "J", "K", "L", "M", "N" },
            true, true, false, 2,4,2, 5, 10, 800));
        Levels.Add(11, new Level(new List<string> { "O", "P", "R", "S" },
            true, true, false, 2,4,2, 5, 10, 800));
    }
 
}
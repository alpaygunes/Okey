using System.Collections.Generic;
using UnityEngine;

  
public static class Renkler
{
    private static byte alpha = 255;
    public static Dictionary<int, Color32> RenkSozlugu = new Dictionary<int, Color32>();
    
    static Renkler()
    {
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(252, 106, 98, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(140, 191, 124, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 0, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(227, 252, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 0, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 255, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 128, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 0, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 0, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 128, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(175, 50, 50, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 192, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(100, 0, 100, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(192, 192, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(192, 0, 192, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 192, 192, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 0, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 255, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 0, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 128, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 255, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 128, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 128, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 255, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(64, 0, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(0, 0, 64, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(64, 64, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(64, 0, 64, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 64, 0, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(150, 0, 64, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(200, 105, 100, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(64, 0, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 192, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(192, 255, 128, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 255, 192, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(192, 128, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(128, 192, 255, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 128, 192, alpha));
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(192, 192, 255, alpha));
        
    }
     
}
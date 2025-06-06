using System.Collections.Generic;
using UnityEngine;

public static class Renkler
{
    private static byte alpha = 230; // %90 şeffaflık - outline için ideal
    public static Dictionary<int, Color32> RenkSozlugu = new Dictionary<int, Color32>();
    
    static Renkler()
    {
        // İnsan gözünün kolayca ayırt edebildiği 20 farklı yüksek kontrast renk
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(220, 20, 20, alpha));     // Kırmızı - canlı
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(20, 120, 220, alpha));    // Mavi - parlak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(20, 180, 20, alpha));     // Yeşil - canlı 
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(255, 165, 0, alpha));     // Turuncu - sıcak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(128, 0, 128, alpha));     // Mor - derin
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(255, 215, 0, alpha));     // Altın sarısı - parlak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(220, 20, 120, alpha));    // Magenta - çarpıcı
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(0, 150, 150, alpha));     // Cyan - soğuk
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(139, 69, 19, alpha));     // Kahverengi - toprak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(75, 75, 75, alpha));      // Koyu gri - nötr
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(255, 20, 147, alpha));    // Pembe - canlı
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(50, 205, 50, alpha));     // Lime yeşili - parlak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(70, 130, 180, alpha));    // Çelik mavisi - sakin
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(218, 165, 32, alpha));    // Altın - zengin
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(199, 21, 133, alpha));    // Orta pembe - sıcak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(72, 61, 139, alpha));     // Koyu mor - mistik
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(210, 105, 30, alpha));    // Çikolata - toprak
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(46, 139, 87, alpha));     // Deniz yeşili - doğal
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(178, 34, 34, alpha));     // Ateş kırmızısı - güçlü
        RenkSozlugu.Add(RenkSozlugu.Count, new Color32(106, 90, 205, alpha));    // Slate mavi - elegant
    }
}
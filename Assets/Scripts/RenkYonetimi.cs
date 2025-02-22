using System.Collections.Generic;
using UnityEngine;

  
public static class RenklerYonetimi
{
    public static Dictionary<int, Color32> RenkSozlugu = new Dictionary<int, Color32>();
 


    static RenklerYonetimi()
    {
        RenkSozlugu.Add(RenkSozlugu.Count,new Color32(255, 0, 0, 255));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 255, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 0, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 255, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 0, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 255, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 0, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 128, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 0, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 128, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 0, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 128, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 0, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 192, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 0, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 192, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 0, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 192, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 128, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 0, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 255, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 255, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 0, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 128, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 128, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 255, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 128, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 255, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 128, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 255, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(64, 0, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 64, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 0, 64, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(64, 64, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(64, 0, 64, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 64, 64, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 64, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 0, 64, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(64, 255, 0, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 255, 64, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(64, 0, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(0, 64, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 192, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 255, 128, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 255, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 128, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(128, 192, 255, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(255, 128, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 255, 192, 255)));
        RenkSozlugu.Add(RenkSozlugu.Count,(new Color32(192, 192, 255, 255)));
        
    }
     
}
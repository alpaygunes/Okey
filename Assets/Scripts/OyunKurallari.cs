using System;
using UnityEngine;
using UnityEngine.Serialization;

public class OyunKurallari:MonoBehaviour
{
    public static OyunKurallari Instance;
    public OyunTipleri GuncelOyunTipi { get; set; }
    public int HamleLimit { get; private set; }
    public int SkorLimiti { get; private set; }
    public float ZamanLimiti { get; private set; } 
    
    public enum OyunTipleri
    {
        HamleLimitli,
        SkorLimitli,
        ZamanLimitli,
        RakibeGonder,
    }

    private void Awake(){
        
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    public OyunKurallari()
    { 
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        switch (GuncelOyunTipi)
        {
            case OyunTipleri.HamleLimitli:
                HamleLimit = 5;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                break;

            case OyunTipleri.SkorLimitli:
                HamleLimit = 0;
                SkorLimiti = 20;
                ZamanLimiti = 0;
                break;

            case OyunTipleri.ZamanLimitli:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 120f;
                break;

            case OyunTipleri.RakibeGonder:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                break;

            default:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                break;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;


[System.Serializable]
public class LevelVerisi
{
    public int Level;
    public int RenkSirasi;
    public int MeyveSirasi;
    public int KalipSirasi;
}

[System.Serializable]
public class LevelTablosu
{
    public List<LevelVerisi> LevelListesi = new List<LevelVerisi>();
}


public class SoloLevelManager : MonoBehaviour
{
    public LevelTablosu tablo = new LevelTablosu();
    private string dosyaYolu;
    public static SoloLevelManager Instance;
    public string AsilanLimit { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;

        dosyaYolu = Application.persistentDataPath + "/leveldata.json";
        YukleJson(); // oyun başlarken verileri yükle
    }

    // 🔹 Level kaydet
    public void KaydetLevel(int level, int renk, int meyve, int kalip)
    {
        LevelVerisi veri = new LevelVerisi
        {
            Level = level,
            RenkSirasi = renk,
            MeyveSirasi = meyve,
            KalipSirasi = kalip
        };

        // Daha önce aynı level oynanmışsa güncelle
        var mevcut = tablo.LevelListesi.Find(x => x.Level == level);
        if (mevcut != null)
        {
            mevcut.RenkSirasi = renk;
            mevcut.MeyveSirasi = meyve;
            mevcut.KalipSirasi = kalip;
        }
        else
        {
            tablo.LevelListesi.Add(veri);
        }

        KaydetJson(); // her seferinde dosyaya kaydet
        Debug.Log($"Level {level} kaydedildi.");
    }

    // 🔹 Level verisini getir
    public LevelVerisi GetLevelVerisi(int level)
    {
        return tablo.LevelListesi.Find(x => x.Level == level);
    }

    // 🔹 JSON'a kaydet
    private void KaydetJson()
    {
        string json = JsonUtility.ToJson(tablo, true);
        File.WriteAllText(dosyaYolu, json);
    }

    // 🔹 JSON'dan yükle
    private void YukleJson()
    {
        if (File.Exists(dosyaYolu))
        {
            string json = File.ReadAllText(dosyaYolu);
            tablo = JsonUtility.FromJson<LevelTablosu>(json);
            if (tablo == null) tablo = new LevelTablosu();
        }
    }

    public int GetMaxLevel()
    {
        if (tablo.LevelListesi.Count == 0) return 0; // hiç level yoksa 0 dön
        return tablo.LevelListesi.Max(x => x.Level);
    }

    public void Guncelle()
    {
        if (!PuanLimitiDoldumu()) return;

        if (IsaretleBelirtYoket.Instance.HamleSayisi >=
            GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")].HamleLimiti)
        {
            AsilanLimit = "HamleLimiti";
            OyunSahnesiUI.Instance.YeniBasariGoster(AsilanLimit);
            //SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            return;
        }

        AsilanLimit = "";
        sayac();
        OyunSahnesiUI.Instance.YeniBasariGoster(AsilanLimit);
    }

    private void sayac()
    {
        var OynananLevel = GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")];
        var kayitliOyuncuVerisi = GameManager.Instance.kayitliOyuncuVerisi;
        var MeyveSirasi = kayitliOyuncuVerisi.MeyveSirasi;
        var RenkSirasi = kayitliOyuncuVerisi.RenkSirasi;
        var KalipSirasi = kayitliOyuncuVerisi.KalipSirasi;

        if (MeyveSirasi < OynananLevel.MeyveEnd)
        {
            MeyveSirasi++;
            KaydetLevel(kayitliOyuncuVerisi.Level, kayitliOyuncuVerisi.RenkSirasi, MeyveSirasi,
                kayitliOyuncuVerisi.KalipSirasi);
            AsilanLimit = "Meyve";
        }
        else
        {
            if (RenkSirasi < OynananLevel.RenkEnd)
            {
                RenkSirasi++;
                KaydetLevel(kayitliOyuncuVerisi.Level, RenkSirasi, MeyveSirasi, kayitliOyuncuVerisi.KalipSirasi);
                AsilanLimit = "Renk";
            }
            else
            {
                if (OynananLevel.Kalip != null && KalipSirasi < OynananLevel.Kalip.Count - 1)
                {
                    KalipSirasi++;
                    KaydetLevel(kayitliOyuncuVerisi.Level, RenkSirasi, MeyveSirasi, KalipSirasi);
                    AsilanLimit = "Kalip";
                }
                else
                {
                    var NewLevel = kayitliOyuncuVerisi.Level;
                    NewLevel++;
                    AsilanLimit = "PuanLimiti";
                    var limitler = GameLevels.Levels[NewLevel];
                    KaydetLevel(NewLevel, limitler.RenkStart, limitler.MeyveStart, 0);
                    PlayerPrefs.SetInt("OynananLevelID", NewLevel);
                }
            }
        }
    }


    private bool PuanLimitiDoldumu()
    {
        var bonusMeyveSayisi = PuanlamaIStatistikleri.BonusMeyveSayisi;
        var altinSayisi = PuanlamaIStatistikleri.AltinSayisi;
        var elmasSayisi = PuanlamaIStatistikleri.ElmasSayisi;
        var toplananPuan = 0;
        var levelGecmePuani = GameLevels.Levels[PlayerPrefs.GetInt("OynananLevelID")].LevelGecmePuani;

        toplananPuan += bonusMeyveSayisi;
        toplananPuan += altinSayisi * 4;
        toplananPuan += elmasSayisi * 10;
        OyunSahnesiUI.Instance.SkorTxt.text = toplananPuan.ToString();
        if (toplananPuan >= levelGecmePuani)
        {
            PuanlamaIStatistikleri.BonusMeyveSayisi = 0;
            PuanlamaIStatistikleri.AltinSayisi = 0;
            PuanlamaIStatistikleri.ElmasSayisi = 0;
        }

        return toplananPuan >= levelGecmePuani;
    }
}
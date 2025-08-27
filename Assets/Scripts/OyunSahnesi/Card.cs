using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Vector2 Size;
    public List<GameObject> spawnHolesList = new List<GameObject>();
    public int SatirSayisi;

    public static Card Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        Size = GetComponent<SpriteRenderer>().bounds.size;
    }

    public void CreateSpawnHoles()
    {
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance.ColonCount;
        if (colonWidth > cardSize.x / 6)
        {
            colonWidth = cardSize.x / 6;
        }

        var toplamgenislik = GameManager.Instance.ColonCount * colonWidth;
        var fark = cardSize.x - toplamgenislik;
        for (int i = 0; i < GameManager.Instance.ColonCount; i++)
        {
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            holePositionX += fark * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            SpawnHole.GetComponent<SpawnHole>().colID = i;
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth, colonWidth);
            spawnHolesList.Add(sh);
        }
    }

    public void KutulariHazirla()
    {
        Vector2 cardSize = Instance.Size;
        float colonWidth = (cardSize.x / GameManager.Instance.ColonCount);
        if (colonWidth > cardSize.x / 6)
        {
            colonWidth = cardSize.x / 6;
        }

        var toplamgenislik = GameManager.Instance.ColonCount * colonWidth;
        var fark = cardSize.x - toplamgenislik;


        SatirSayisi = (int)(cardSize.y / colonWidth);
        for (var satir = 0; satir < SatirSayisi; satir++)
        {
            for (int sutun = 0; sutun < GameManager.Instance.ColonCount; sutun++)
            {
                GameObject kutu_ = Resources.Load<GameObject>("Prefabs/Kutu");
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                positionX += fark * .5f;
                float positionY = -(cardSize.y * .5f) + ((SatirSayisi - satir) * colonWidth);
                var kutu = Instantiate(kutu_, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = spawnHolesList[0].transform.localScale;
                kutu.transform.Find("OdulBelirteci").gameObject.SetActive(false);
                kutu.transform.Find("IsStaticBelirteci").gameObject.SetActive(false);
                kutu.transform.Find("KilitBelirteci").gameObject.SetActive(false);
                kutu.tag = "KUTU";
                kutu.GetComponent<Kutu>().colRowPosition = new Vector2Int(sutun, satir);
            }
        }

        var kutular = GameObject.FindGameObjectsWithTag("KUTU");
        var sonkutu = kutular[kutular.Length - 1];
        var posY = sonkutu.transform.position.y;
        var sonkutununalti = posY - sonkutu.transform.localScale.y * 0.5f;
        var istaka = GameObject.Find("Middle/Stoper");
        istaka.transform.position =
            new Vector3(istaka.transform.position.x, sonkutununalti, istaka.transform.position.z);
    }

    public void Sallanma()
    {
        var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar)
        {
            TasManeger.Instance.TasInstances[tas].sallanmaDurumu = false;
        }
    }

    public void GoreveUyumluCtasYoket()
    {
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar)
        {
            foreach (var pTas in grup.Value.Taslar)
            {
                foreach (var cTas in pTas.ayniKolondakiAltinveElmasTaslar)
                {
                    var uTasInstance = TasManeger.Instance.TasInstances[cTas];
                    uTasInstance.tiklanaBilir = false;
                    uTasInstance.StartCoroutine(uTasInstance.BekleYokol(beklemeSuresi));
                    beklemeSuresi += 0.1f;
                }
            }
        }
    }

    public void PtasIleUyumluCtaslariYoket()
    {
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar)
        {
            foreach (var pTas in grup.Value.Taslar)
            {
                foreach (var bonusTaslari in pTas.BonusOlarakEslesenTaslar)
                {
                    bonusTaslari.Value.tiklanaBilir = false;
                    bonusTaslari.Value.StartCoroutine(bonusTaslari.Value.BekleYokol(beklemeSuresi));
                    if (!bonusTaslari.Value.kilitli)
                        beklemeSuresi += .1f;
                }
            }
        }
    }

    public void CardtakiBonusTaslariBelirt()
    {
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar)
        {
            var pTaslar = grup.Value.Taslar;
            foreach (var pTasInstance in pTaslar)
            {
                foreach (var cTas in cardtakiTaslar)
                {
                    var cTasInstance = TasManeger.Instance.TasInstances[cTas];
                    if (grup.Value.GrupTuru == "rama")
                    {
                        if (pTaslar.Count == 3)
                        {
                            if (cTasInstance.MeyveID == pTasInstance.MeyveID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count == 4)
                        {
                            if (pTasInstance.RenkID == cTasInstance.RenkID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count >= 5)
                        {
                            pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.bonusBayragi = true;
                            cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                        }
                    }
                    else if (grup.Value.GrupTuru == "ramf")
                    {
                        if (pTaslar.Count == 3)
                        {
                            // bonus yok
                        }
                        else if (pTaslar.Count == 4)
                        {
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID
                                && pTasInstance.RenkID == cTasInstance.RenkID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count == 5)
                        {
                            if (pTasInstance.RenkID == cTasInstance.RenkID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count >= 6)
                        {
                            pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.bonusBayragi = true;
                            cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                        }
                    }
                    else if (grup.Value.GrupTuru == "rfma")
                    {
                        if (pTaslar.Count == 3)
                        {
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID && pTasInstance.RenkID == cTasInstance.RenkID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count == 4)
                        {
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                        else if (pTaslar.Count == 5)
                        {
                            if (pTasInstance.RenkID == cTasInstance.RenkID)
                            {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        } else if (pTaslar.Count >= 6) {
                            if (pTasInstance.RenkID == cTasInstance.RenkID) {
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.bonusBayragi = true;
                                cTasInstance.ptasIleUyumluGostergesi.SetActive(true);
                            }
                        }
                    }

                    // kilitliyse göstergeleri gizle
                    foreach (var eslesenbonustasla in pTasInstance.BonusOlarakEslesenTaslar)
                    {
                        var kilitli = pTasInstance.BonusOlarakEslesenTaslar[eslesenbonustasla.Key].kilitli;
                        var ctasinstance = pTasInstance.BonusOlarakEslesenTaslar[eslesenbonustasla.Key];
                        ctasinstance.ptasIleUyumluGostergesi.SetActive(!kilitli); 
                    }
                }
            }
        }
    }

    public void TaslariAltinVeElmasaDonustur()
    {
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar)
        {
            foreach (var pTas in grup.Value.Taslar)
            {
                pTas.AltinVeElmasGoster();
            }
        }
    }

    public bool TiklanamazTasVar()
    {
        // tıklana bilir nesne varsa oyun durumunu değiştirelim
        bool TiklanamazTasVar = false;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
        foreach (var cTas in cardtakiTaslar)
        {
            var cTasIstance = TasManeger.Instance.TasInstances[cTas];
            if (cTasIstance.tiklanaBilir == false)
            {
                TiklanamazTasVar = true;
                break;
            }
        }

        if (!TiklanamazTasVar)
        {
            foreach (var pTas in perdekiTaslar)
            {
                var pTasIstance = TasManeger.Instance.TasInstances[pTas];
                if (pTasIstance.tiklanaBilir == false)
                {
                    TiklanamazTasVar = true;
                    break;
                }
            }
        }

        return TiklanamazTasVar;
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour{
    public Vector2 Size; 
    public List<GameObject> spawnHolesList = new List<GameObject>();

    public static Card Instance{ get; private set; } 

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;

        Instance = this;
        Size = GetComponent<SpriteRenderer>().bounds.size;
    }

    public void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance._colonCount;
        for (int i = 0; i < GameManager.Instance._colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .35f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            SpawnHole.GetComponent<SpawnHole>().colID = i;
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth, colonWidth);
            spawnHolesList.Add(sh);
        }
    }

    public void KutulariHazirla(){
        Vector2 cardSize = Instance.Size;
        float colonWidth = (cardSize.x / GameManager.Instance._colonCount);
        GameObject kutu_ = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = (cardSize.y / colonWidth);
        for (var satir = 0; satir < satirSayisi; satir++){
            for (int sutun = 0; sutun < GameManager.Instance._colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = -(cardSize.y * .5f) + ((satirSayisi - satir) * colonWidth);
                var kutu = Instantiate(kutu_, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = spawnHolesList[0].transform.localScale;
            }
        }
    }

    public void Sallanma(){
        var carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar){
            TasManeger.Instance.TasInstances[tas].sallanmaDurumu = false;
        }
    }

    public void GoreveUyumluCtasYoket(){
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            foreach (var pTas in grup.Value.Taslar){
                foreach (var cTas in pTas.AyniKolondakiAltinveElmasTaslar){
                    var uTasInstance = TasManeger.Instance.TasInstances[cTas];
                    uTasInstance.TiklanaBilir = false;
                    uTasInstance.StartCoroutine(uTasInstance.BekleYokol(beklemeSuresi));
                    beklemeSuresi += 0.5f;
                } 
                
            }
        }
    }

    public void PtasIleUyumluCtaslariYoket(){
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            foreach (var pTas in grup.Value.Taslar){ 
                foreach (var bonusTaslari in pTas.BonusOlarakEslesenTaslar){
                    bonusTaslari.Value.TiklanaBilir = false;
                    bonusTaslari.Value.StartCoroutine(bonusTaslari.Value.BekleYokol(beklemeSuresi));
                    beklemeSuresi += .5f;
                }
            }
        }
    }
    
    public void CardtakiBonusTaslariBelirt(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            var pTaslar = grup.Value.Taslar;
            foreach (var pTasInstance in pTaslar){
                foreach (var cTas in cardtakiTaslar){
                    var cTasInstance = TasManeger.Instance.TasInstances[cTas];
                    if (grup.Value.GrupTuru == "rama"){
                        if (pTaslar.Count == 3){
                            if (cTasInstance.MeyveID == pTasInstance.MeyveID){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count == 4){
                            if (pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count >= 5){
                            pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.BonusBayragi = true;
                            cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                            //cTasInstance.TiklanaBilir = false;
                        }
                    }
                    else if (grup.Value.GrupTuru == "ramf"){
                        if (pTaslar.Count == 3){
                            // bonus yok
                        }
                        else if (pTaslar.Count == 4){
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID
                                && pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count == 5){
                            if (pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count >= 6){
                            pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                cTasInstance);
                            cTasInstance.BonusBayragi = true;
                            cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                            //cTasInstance.TiklanaBilir = false;
                        }
                    }
                    else if (grup.Value.GrupTuru == "rfma"){
                        if (pTaslar.Count == 3){
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID && pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count == 4){
                            if (pTasInstance.MeyveID == cTasInstance.MeyveID){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count == 5){
                            if (pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                        else if (pTaslar.Count >= 6){
                            if (pTasInstance.Renk == cTasInstance.Renk){
                                pTasInstance.BonusOlarakEslesenTaslar.Add(pTasInstance.BonusOlarakEslesenTaslar.Count,
                                    cTasInstance);
                                cTasInstance.BonusBayragi = true;
                                cTasInstance.PtasIleUyumluGostergesi.SetActive(true);
                                //cTasInstance.TiklanaBilir = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public void TaslariAltinVeElmasaDonustur(){
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            foreach (var pTas in grup.Value.Taslar){
                pTas.AltinVeElmasGoster();
            }
        }
    }

    public bool TiklanamazTasVar(){
        // tıklana bilir nesne varsa oyun durumunu değiştirelim
        bool TiklanamazTasVar = false;
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        var perdekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
        foreach (var cTas in cardtakiTaslar){
            var cTasIstance = TasManeger.Instance.TasInstances[cTas];
            if (cTasIstance.TiklanaBilir == false){
                TiklanamazTasVar = true;
                break;
            }
        }

        if (!TiklanamazTasVar){
            foreach (var pTas in perdekiTaslar){
                var pTasIstance = TasManeger.Instance.TasInstances[pTas];
                if (pTasIstance.TiklanaBilir == false){
                    TiklanamazTasVar = true;
                    break;
                }
            }
        }
        return TiklanamazTasVar;
    }
}
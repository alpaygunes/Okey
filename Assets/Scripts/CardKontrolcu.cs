using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardKontrolcu : MonoBehaviour{
    public static CardKontrolcu Instance { get; private set; }
    private List<List<GameObject>> SiraliAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliAyniRenkliGrup = new List<GameObject>();

    private List<List<GameObject>> SiraliFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliFarkliRenkliGrup = new List<GameObject>();

    private List<List<GameObject>> AyniRakamAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniRakamAyniRenkliGrup = new List<GameObject>();

    private List<List<GameObject>> AyniRakamFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniRakamFarkliRenkli = new List<GameObject>();

    private TextMeshProUGUI textMesh;
    private Camera uiCamera;
    
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
        uiCamera = Camera.main; 
    }

    private void Start(){
        textMesh = GameObject.Find("FlatingText1").GetComponent<TextMeshProUGUI>(); 
    }

    // Karttaki taşları kontrol edip yan yana  perler varmı bakalım
    public async Task KartiKontrolEt() {
        
        SiraliAyniRenkliGruplar.Clear();
        SiraliFarkliRenkliGruplar.Clear();
        AyniRakamAyniRenkliGruplar.Clear();
        AyniRakamFarkliRenkliGruplar.Clear();

        GameObject[] taslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        // önceki işaretlemeleri iptal edelim.
        foreach (var tas in taslar) {
            TasManeger.Instance.TasIstances[tas].birCardPerineDahil = false;
        }

        for (int i = 0; i < taslar.Length; i++) {
            var tas = taslar[i];

            SiraliAyniRenkliGrup.Clear();
            await TasManeger.Instance.TasIstances[tas].SiraliAyniRenkGrubunaDahilOl();
            if (SiraliAyniRenkliGrup.Count > 2) {
                SiraliAyniRenkliGruplar.Add(new List<GameObject>(SiraliAyniRenkliGrup));
                foreach (var item in SiraliAyniRenkliGrup) {
                    TasManeger.Instance.TasIstances[item].birCardPerineDahil = true;
                }
            }

            SiraliFarkliRenkliGrup.Clear();
            await TasManeger.Instance.TasIstances[tas].SiraliFarkliRenkGrubunaDahilOl();
            if (SiraliFarkliRenkliGrup.Count > 2) {
                SiraliFarkliRenkliGruplar.Add(new List<GameObject>(SiraliFarkliRenkliGrup));
                foreach (var item in SiraliFarkliRenkliGrup) {
                    TasManeger.Instance.TasIstances[item].birCardPerineDahil = true;
                }
            }

            AyniRakamAyniRenkliGrup.Clear();
            await TasManeger.Instance.TasIstances[tas].AyniRakamAyniRenkGrubunaDahilOl();
            if (AyniRakamAyniRenkliGrup.Count > 2) {
                AyniRakamAyniRenkliGruplar.Add(new List<GameObject>(AyniRakamAyniRenkliGrup));
                foreach (var item in AyniRakamAyniRenkliGrup) {
                    TasManeger.Instance.TasIstances[item].birCardPerineDahil = true;
                }
            }

            AyniRakamFarkliRenkli.Clear();
            await TasManeger.Instance.TasIstances[tas].AyniRakamFarkliRenkGrubunaDahilOl();
            if (AyniRakamFarkliRenkli.Count > 2) {
                AyniRakamFarkliRenkliGruplar.Add(new List<GameObject>(AyniRakamFarkliRenkli));
                foreach (var item in AyniRakamFarkliRenkli) {
                    TasManeger.Instance.TasIstances[item].birCardPerineDahil = true;
                }
            }
        }

        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan = 0;
        foreach (var grup in SiraliAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZenminRenginiDegistir();
            }
        }

        foreach (var grup in SiraliFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZenminRenginiDegistir();
            }
        }

        foreach (var grup in AyniRakamAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZenminRenginiDegistir();
            }
        }

        foreach (var grup in AyniRakamFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZenminRenginiDegistir();
            }
        }

        StartCoroutine(SkorTMPleriGuncelle());
    }

    IEnumerator SkorTMPleriGuncelle() {
        yield return new WaitForSeconds(1);
        
        //PuanlamaKontrolcu.Instance.toplamPuan += PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan;
        //PuanlamaKontrolcu.Instance.toplamPuanTMP.text = PuanlamaKontrolcu.Instance.toplamPuan.ToString(); 
        var startScore =   PuanlamaKontrolcu.Instance.toplamPuan;
        var currentScore = PuanlamaKontrolcu.Instance.toplamPuan + PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan;
        DOTween.To(() => startScore, x => PuanlamaKontrolcu.Instance.toplamPuanTMP.text = x.ToString(), currentScore, 1f).SetEase(Ease.OutQuad);
        
        PuanlamaKontrolcu.Instance.toplamPuan += PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan;
        //print($"PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan  : {PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan} , Toplam :{PuanlamaKontrolcu.Instance.toplamPuan}");
        
        // floatingText
        Vector3 _targetPosition = uiCamera.WorldToScreenPoint(new Vector3(0,2,0));
        textMesh.text = "+" + PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan.ToString();
        textMesh.transform.position = uiCamera.WorldToScreenPoint(new Vector3(0,0,0));
        textMesh.transform.DOMoveY(_targetPosition.y, 2f).SetEase(Ease.OutQuad);
        var color = textMesh.color;
        color.a = 1f;
        textMesh.color = color;
        textMesh.DOFade(0, 3f);
    }
}
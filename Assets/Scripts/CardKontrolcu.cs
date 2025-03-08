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
    
    private AudioSource _audioSource_puan_sayac;
    

    void Awake(){
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
        uiCamera = Camera.main;
        
        _audioSource_puan_sayac = gameObject.AddComponent<AudioSource>();
        _audioSource_puan_sayac.playOnAwake = false;
        _audioSource_puan_sayac.clip = Resources.Load<AudioClip>("Sounds/puan_sayac");
    }

    private void Start(){
        textMesh = GameObject.Find("FlatingText1").GetComponent<TextMeshProUGUI>();
    }

    // Karttaki taşları kontrol edip yan yana  perler varmı bakalım
    public async Task KarttakiPerleriBul(){
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
                TasManeger.Instance.TasIstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasIstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in SiraliFarkliRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasIstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in AyniRakamAyniRenkliGruplar) {
            foreach (var item in grup) {
                TasManeger.Instance.TasIstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasIstances[item].RakamiPuanaEkle(1));
            }
        }

        foreach (var grup in AyniRakamFarkliRenkliGruplar) {
            foreach (var item in grup) { 
                TasManeger.Instance.TasIstances[item].ZeminSpriteRenderer.color = Color.green;
                StartCoroutine(TasManeger.Instance.TasIstances[item].RakamiPuanaEkle(1));
            }
        }

        StartCoroutine(SkorTMPleriGuncelle());
    }

    IEnumerator SkorTMPleriGuncelle(){
        yield return new WaitForSeconds(1);

        // puan artış animasyonu
        var startScore = PuanlamaKontrolcu.Instance.toplamPuan;
        var currentScore = PuanlamaKontrolcu.Instance.toplamPuan + PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan;
        DOTween.To(() => startScore, x => PuanlamaKontrolcu.Instance.toplamPuanTMP.text = x.ToString(), currentScore,
            1f).SetEase(Ease.OutQuad);
        PuanlamaKontrolcu.Instance.toplamPuan += PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan;
        PuanlamaKontrolcu.Instance.toplamPuanTMP.transform.DOPunchPosition(new Vector3(Screen.width*.01f, Screen.height*.02f, 0f), 1f, 30, 0.5f);

        // sayaç sesi
        _audioSource_puan_sayac.Play();
        
        // floatingText
        Vector3 targetPosition = uiCamera.WorldToScreenPoint(new Vector3(0, 2, 0));
        textMesh.text = "+" + PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan.ToString();
        textMesh.transform.position = uiCamera.WorldToScreenPoint(new Vector3(0, 0, 0));
        textMesh.transform.DOMoveY(targetPosition.y, 2f).SetEase(Ease.OutQuad);
        var color = textMesh.color;
        color.a = 1f;
        textMesh.color = color;
        textMesh.DOFade(0, 3f);
    }
}
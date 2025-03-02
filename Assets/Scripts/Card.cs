using System;
using UnityEngine;

public class Card : MonoBehaviour{
    public Vector2 Size;
    public static Card Instance{ get; private set; }
    public bool TaslarHareketli{ get; set; }

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this;
        Size = GetComponent<SpriteRenderer>().bounds.size; 
    }

 
}

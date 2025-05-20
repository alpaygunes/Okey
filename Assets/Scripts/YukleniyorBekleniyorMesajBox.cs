using System;
using UnityEngine;
using UnityEngine.UIElements;

public class YukleniyorBekleniyorMesajBox : MonoBehaviour
{
    public static YukleniyorBekleniyorMesajBox Instance; 
    
    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); 
            return;
        } 
        Instance = this; 
    }
    
    public VisualElement CreateLoadingElement(string mesaj)
    {
        var container = new VisualElement();
        container.style.alignItems = Align.Center;
        container.style.justifyContent = Justify.Center;
        container.style.width = new Length(100, LengthUnit.Percent);
        container.style.height = new Length(100, LengthUnit.Percent);
        
        var image = new Image();
        image.image = Resources.Load<Texture2D>("images/kum_saati");
        image.style.width = 64;
        image.style.height = 64;
        
        var label = new Label(mesaj);
        label.style.marginTop = 10;
        label.style.fontSize = 20;
        label.style.color = new StyleColor(Color.white);
        
        container.Add(image);
        container.Add(label);
        
        return container;
    }
    
    
}

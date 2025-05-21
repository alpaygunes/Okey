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
        container.style.position = Position.Absolute; 
        container.style.top = new Length(50, LengthUnit.Percent);
        container.style.width = new Length(100, LengthUnit.Percent);
        container.style.translate = new StyleTranslate(new Translate(0, -50, 0));  
        
        var image = new Image();
        image.image = Resources.Load<Texture2D>("images/kum_saati");
        image.style.width = 64;
        image.style.height = 64;   
        image.style.alignSelf = Align.Center; 
        
        var label = new Label(mesaj);
        label.style.fontSize = 12;
        label.style.color = new StyleColor(Color.white); 
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.justifyContent = Justify.Center;
        label.style.alignSelf = Align.Center;
        
        container.Add(image);
        container.Add(label);
        
        return container;
    }
    
    
}

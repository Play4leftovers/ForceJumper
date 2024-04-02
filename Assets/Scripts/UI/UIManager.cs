using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI OrbHealthText;
    public Image OrbSizeIndicator;
    public ForceFieldBehaviour ForceField;
    public Shooting ShootingBehaviour;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ForceField = FindObjectOfType<ForceFieldBehaviour>();
        if (ForceField)
        {
            OrbHealthText.text = ForceField.Health.ToString();
            OrbSizeIndicator.rectTransform.localScale = new Vector3(ShootingBehaviour.OrbScale, ShootingBehaviour.OrbScale,
            ShootingBehaviour.OrbScale) / 4;
        }
        else
        {
            OrbHealthText.text = "X";
        }
    }
}

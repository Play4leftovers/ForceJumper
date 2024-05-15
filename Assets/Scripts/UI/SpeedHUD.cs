using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;

    public void UpdateSelf(float incomingValue)
    {
        textMeshPro.text = incomingValue.ToString("F1");
    }
}

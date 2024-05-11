using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleJumpHUD : MonoBehaviour
{
    [SerializeField] Image doubleJumpImage;
    
    // Start is called before the first frame update
    public void UpdateSelf(bool incomingValue)
    {
        int temp = incomingValue ? 1 : 0;
        doubleJumpImage.color = new Color(doubleJumpImage.color.r, doubleJumpImage.color.g, doubleJumpImage.color.b, temp);
    }
}

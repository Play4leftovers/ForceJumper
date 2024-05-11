using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashHUD : MonoBehaviour
{
    [SerializeField] Image dashImage;

    // Start is called before the first frame update
    public void UpdateSelf(bool incomingValue)
    {
        int temp = incomingValue ? 1 : 0;
        dashImage.color = new Color(dashImage.color.r, dashImage.color.g, dashImage.color.b, temp);
    }
}

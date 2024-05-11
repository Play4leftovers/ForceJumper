using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] PlayerMovementPhysics playerMovementPhysics;

    [SerializeField] private DashHUD _dashHUD;
    [SerializeField] private DoubleJumpHUD _doubleJumpHUD;
    [SerializeField] private SpeedHUD _speedHUD;
    
    private bool _doubleJump;
    private bool _dash;
    private float _speed;

    // Update is called once per frame
    void Update()
    {
        _doubleJumpHUD.UpdateSelf(!playerMovementPhysics.doubleJump);
        _dashHUD.UpdateSelf(playerMovementPhysics.dashReady);
        _speedHUD.UpdateSelf(playerMovementPhysics.currentSpeed);
    }
}
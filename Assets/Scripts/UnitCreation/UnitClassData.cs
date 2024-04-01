using UnityEngine;

[CreateAssetMenu(fileName = "UnitClassData", menuName = "Custom/New Unit Class", order = 1)]
public class UnitClassData : ScriptableObject
{
    public string className;
    public string classDescription;

    public bool promotedClass;

    [Header("Base Stats")]

    public int defaultMovementRange;
    public int defaultHealth;
    public int defaultPower;
    public int defaultIntelligence;
    public int defaultSpeed;
    public int defaultAim;
    public int defaultReflexes;

    [Header("Growth Rates")]

    [Range(0, 1)] public float healthGrowth;
    [Range(0, 1)] public float powerGrowth;
    [Range(0, 1)] public float intelligenceGrowth;
    [Range(0, 1)] public float speedGrowth;
    [Range(0, 1)] public float aimGrowth;
    [Range(0, 1)] public float reflexesGrowth;

    public enum UnitType
    {
        Melee,
        Ranged
    }

    public UnitType UnitMobilityType;
}

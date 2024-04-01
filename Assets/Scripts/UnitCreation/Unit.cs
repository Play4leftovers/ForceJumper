using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Properties")]

    [SerializeField] private string _unitName = "Unit";
    [SerializeField] private string _unitDescription = "A unit.";

    [SerializeField] private UnitClassData defaultEnemyData;
    [SerializeField] private EnemyType unitClass;
    [SerializeField] private UnitTeam team;

    [Header("Level")]
    private int _minLevel = 1;
    private int _maxLevel = 20;

    [SerializeField]
    [Range(1, 20)]
    private int _level;

    [SerializeField]
    [Range(0, 100)]
    private int experience;

    [Header("Stats")]
    public int health = 0;
    public int maxHealth = 0;

    public int power;
    public int intelligence;
    public int speed;
    public int aim;
    public int reflexes;

    public int movementRange = 0;

    [SerializeField]
    private enum UnitTeam
    {
        Team1,
        Team2
    }

    [SerializeField]
    private enum EnemyType
    {
        Default,
        Humanoid,
        Turret,
    }

    void Start()
    {
        LoadUnitData();
    }

    // Handles experience gain.
    public void GainExperience(int experienceGained)
    {
        if (_level != _maxLevel)
        {
            experience += experienceGained;

            if (experience >= 100)
            {
                experience -= 100;
                _level++;
            }
        }
    }

    // Handles taking damage.
    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;

            if (health <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void LoadUnitData()
    {
        UnitClassData selectedData = GetSelectedClassData();

        if (selectedData != null)
        {
            movementRange = selectedData.defaultMovementRange;

            ApplyGrowthForLevels(selectedData);
            ApplyBaseStats(selectedData);
        }
    }
    private UnitClassData GetSelectedClassData()
    {
        return GetUnitClassData(unitClass);
    }
    private void ApplyBaseStats(UnitClassData selectedData)
    {
        health += selectedData.defaultHealth;
        power += selectedData.defaultPower;
        intelligence += selectedData.defaultIntelligence;
        aim += selectedData.defaultAim;
        reflexes += selectedData.defaultReflexes;
        maxHealth = health;
    }

    private void ApplyGrowthForLevels(UnitClassData selectedData)
    {
        for (int i = 1; i <= _level; i++)
        {
            health = CalculateStatWithGrowth(health, selectedData.healthGrowth);
            power = CalculateStatWithGrowth(power, selectedData.powerGrowth);
            intelligence = CalculateStatWithGrowth(intelligence, selectedData.intelligenceGrowth);
            speed = CalculateStatWithGrowth(speed, selectedData.speedGrowth);
            aim = CalculateStatWithGrowth(aim, selectedData.aimGrowth);
            reflexes = CalculateStatWithGrowth(reflexes, selectedData.reflexesGrowth);
        }
    }

    // Gets the corresponding class data to the selected enum
    private UnitClassData GetUnitClassData(EnemyType unitType)
    {
        switch (unitType)
        {
            default: return defaultEnemyData;
        }
    }

    // Rolls a number between 0.1 - 1.0. If the rolled number is less or equal to the growth rate, the unit receives a point in that respective stat. Else, nothing happens.
    private int CalculateStatWithGrowth(int baseStat, float growthRate)
    {
        float randomValue = Random.value;

        if (randomValue <= growthRate)
        {
            return baseStat + 1;
        }

        return baseStat;
    }
}

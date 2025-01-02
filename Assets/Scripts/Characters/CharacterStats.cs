using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Reflection;
using System.Text;

[Serializable]
public class CharacterStats
{
    public float hp;
    public float armor;
    public float attackDamage;
    public float attackInterval;
    public float attackRange;
    public float criticalChance;
    public float criticalMultiplier;
    public float movingSpeed;
    public float jumpForce;
    public float totalJumps;

    // Copy the values from another CharacterStats instance
    public virtual void CopyFrom(CharacterStats source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        // Get all public instance fields of the type
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Copy the value from the source to the current instance
            field.SetValue(this, field.GetValue(source));
        }
    }

    // Reset to the values of another CharacterStats instance
    public void ResetTo(CharacterStats source)
    {
        CopyFrom(source);
    }
    
    public CharacterStats Inverse()
    {
        // Create a new instance of the same type as the current instance
        CharacterStats inverse = (CharacterStats)Activator.CreateInstance(GetType());

        // Get all public instance fields
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Handle numeric fields (float, int, etc.)
            if (field.FieldType == typeof(float))
            {
                float value = (float)field.GetValue(this);
                field.SetValue(inverse, value != 0 ? 1 / value : 0); // Avoid division by zero
            }
            else if (field.FieldType == typeof(int))
            {
                int value = (int)field.GetValue(this);
                field.SetValue(inverse, value != 0 ? 1 / (float)value : 0); // Convert to float for inversion
            }
        }

        return inverse;
    }
    
    // Add stats from two CharacterStats instances using reflection
    public static CharacterStats operator +(CharacterStats stats1, CharacterStats stats2)
    {
        // Find the most common base type between stats1 and stats2
        Type commonBaseType = FindCommonBaseType(stats1.GetType(), stats2.GetType());

        // Create a new instance of the left-hand side type
        CharacterStats result = (CharacterStats)Activator.CreateInstance(stats1.GetType());

        // Get all public instance fields of the common base type
        FieldInfo[] fields = commonBaseType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                float value1 = (float)field.GetValue(stats1);
                float value2 = (float)field.GetValue(stats2);
                field.SetValue(result, value1 + value2);
            }
            else if (field.FieldType == typeof(int))
            {
                int value1 = (int)field.GetValue(stats1);
                int value2 = (int)field.GetValue(stats2);
                field.SetValue(result, value1 + value2);
            }
        }

        return result;
    }
    
    // Subtract stats from two CharacterStats instances using reflection
    public static CharacterStats operator -(CharacterStats stats1, CharacterStats stats2)
    {
        // Find the most common base type between stats1 and stats2
        Type commonBaseType = FindCommonBaseType(stats1.GetType(), stats2.GetType());

        // Create a new instance of the left-hand side type
        CharacterStats result = (CharacterStats)Activator.CreateInstance(stats1.GetType());

        // Get all public instance fields of the common base type
        FieldInfo[] fields = commonBaseType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                float value1 = (float)field.GetValue(stats1);
                float value2 = (float)field.GetValue(stats2);
                field.SetValue(result, value1 - value2);
            }
            else if (field.FieldType == typeof(int))
            {
                int value1 = (int)field.GetValue(stats1);
                int value2 = (int)field.GetValue(stats2);
                field.SetValue(result, value1 - value2);
            }
        }

        return result;
    }

    
    // Negate all stats using reflection
    public static CharacterStats operator -(CharacterStats stats)
    {
        // Create an instance of the same type as the input stats
        CharacterStats result = (CharacterStats)Activator.CreateInstance(stats.GetType());

        // Get all public instance fields of the stats's type
        FieldInfo[] fields = stats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                float value = (float)field.GetValue(stats);
                field.SetValue(result, -value);
            }
            else if (field.FieldType == typeof(int))
            {
                int value = (int)field.GetValue(stats);
                field.SetValue(result, -value);
            }
        }

        return result;
    }

    
    // Multiply stats from two CharacterStats instances using reflection
    public static CharacterStats operator *(CharacterStats stats1, CharacterStats stats2)
    {
        // Find the most common base type between stats1 and stats2
        Type commonBaseType = FindCommonBaseType(stats1.GetType(), stats2.GetType());

        // Create a new instance of the most common base type
        CharacterStats result = (CharacterStats)Activator.CreateInstance(stats1.GetType());
        result = stats1;
        
        // Get all public instance fields of the common base type
        FieldInfo[] fields = commonBaseType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                float value1 = (float)field.GetValue(stats1);
                float value2 = (float)field.GetValue(stats2);
                // If value2 is 0, retain value1; otherwise, multiply
                field.SetValue(result, value2 == 0 ? value1 : value1 * value2);
            }
            else if (field.FieldType == typeof(int))
            {
                int value1 = (int)field.GetValue(stats1);
                int value2 = (int)field.GetValue(stats2);
                // If value2 is 0, retain value1; otherwise, multiply
                field.SetValue(result, value2 == 0 ? value1 : value1 * value2);
            }
        }

        return result;
    }

    // Helper method to find the most common base type
    private static Type FindCommonBaseType(Type type1, Type type2)
    {
        // Start with the first type and traverse its hierarchy
        while (type1 != null)
        {
            // Check if the second type derives from the current type
            if (type1.IsAssignableFrom(type2))
            {
                return type1;
            }

            // Move up the inheritance hierarchy
            type1 = type1.BaseType;
        }

        return typeof(CharacterStats); // Default to CharacterStats if no common base type is found
    }

    
    // Multiply stats or by a scalar using reflection
    public static CharacterStats operator *(CharacterStats stats, float scalar)
    {
        // Create a new instance of the same type as stats
        CharacterStats result = (CharacterStats)Activator.CreateInstance(stats.GetType());

        // Get all public instance fields of the type
        FieldInfo[] fields = stats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                float value = (float)field.GetValue(stats);
                field.SetValue(result, value * scalar);
            }
            else if (field.FieldType == typeof(int))
            {
                int value = (int)field.GetValue(stats);
                field.SetValue(result, (int)(value * scalar)); // Cast back to int after multiplication
            }
        }

        return result;
    }

    public static CharacterStats operator *(CharacterStats stats, int scalar)
    {
        return stats * (float)scalar; // Convert int scalar to float and reuse the float overload
    }

    public static CharacterStats operator *(float scalar, CharacterStats stats)
    {
        return stats * scalar; // Reuse the existing implementation
    }

    public static CharacterStats operator *(int scalar, CharacterStats stats)
    {
        return stats * (float)scalar; // Convert int scalar to float and reuse the float overload
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{GetType().Name}:");

        // Get all public instance fields, including those from derived classes
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(this);
            sb.AppendLine($"{field.Name}: {value}");
        }

        return sb.ToString();
    }
}

[Serializable]
public class PlayerStats : CharacterStats
{
    public float respawnTimeInterval;

    // Copy the values from another VillagerStats instance
    public override void CopyFrom(CharacterStats source)
    {
        base.CopyFrom(source);
        if (source is PlayerStats playerSource)
        {
            respawnTimeInterval = playerSource.respawnTimeInterval;
        }
    }
}

[Serializable]
public class EnemyStats : CharacterStats
{
    public float sensingRange;
    public float soulValue;
    
}

[Serializable]
public class TowerStats : CharacterStats
{
    [Header("Construction Parameter")]
    public int energyCost;
    public Quaternion rotateAngle;//a fixed amount that determines the rotation type of a tower
    [HideInInspector] public Quaternion curAngle =  Quaternion.Euler(0, 0, 0);
}

[Serializable]
public class RangedTowerStats : TowerStats
{
    [Header("Ranged Tower Fields")]
    public int projectilesPerShot = 1;
}
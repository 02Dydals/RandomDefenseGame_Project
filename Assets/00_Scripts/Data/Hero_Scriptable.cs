using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public Debuff debuffType;
    public float[] parameters;

    public void ConfigureParameters()
    {
        switch(debuffType)
        {
            case Debuff.Slow:
                parameters = new float[2];
                break;
            case Debuff.Sturn:
                parameters = new float[1];
                break;
        }
    }
}

[System.Serializable]
public struct HeroData : INetworkSerializable
{
    public string heroName;
    public int heroATK;
    public float heroATK_Speed;
    public float heroRange;        

    // 강제 직렬화
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref heroName);
        serializer.SerializeValue(ref heroATK);
        serializer.SerializeValue(ref heroATK_Speed);
        serializer.SerializeValue(ref heroRange);
    }
}

[CreateAssetMenu(fileName = "Hero_Scriptable", menuName = "Scriptable Objects/Hero_Scriptable")]
public class Hero_Scriptable : ScriptableObject
{
    public string Name;
    public int ATK;
    public float ATK_Speed;    
    public float Range;
    public Rarity rare;
    public RuntimeAnimatorController m_animator;
    public Bullet HitParticle;

    [Header("##Hero Debuff Data##")]
    [Space(20f)]
    public StatusEffect[] effectType;

    public HeroData GetHeroData()
    {
        return new HeroData
        {
            heroName = Name,
            heroATK = ATK,
            heroATK_Speed = ATK_Speed,
            heroRange = Range
        };
    }
}

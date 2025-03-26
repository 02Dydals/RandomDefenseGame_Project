using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossData
{
    public string BossName;
    public Monster BossPrefab;
}

[CreateAssetMenu(fileName = "Boss_Scriptable", menuName = "Scriptable Objects/Boss_Scriptable")]
public class Boss_Scriptable : ScriptableObject
{
    public List<BossData> bossData = new List<BossData>();
}

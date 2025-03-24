using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

// 이벤트를 하기 위해 델리게이트 사용
public delegate void OnMoneyUpEventHandler();
public delegate void OnTimerUpEventHandler();

// 싱글톤으로 작성
public class Game_Mng : NetworkBehaviour
{
    public static Game_Mng instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    
    public float Timer = 60.0f;
    public int Wave = 1;
    public int Money = 50;
    public int SummonCount = 20;
    public int HeroCount = 0;
    public int HeroMaximumCount = 25;

    public event OnMoneyUpEventHandler OnMoneyUp;
    public event OnTimerUpEventHandler OnTimerUp;

    public List<Monster> monsters = new List<Monster>();
    public int MonsterCount;

    private void Update()
    {
        if(IsServer)
        {
            
            if(Timer > 0)
            {
                Timer -= Time.deltaTime;
                Timer = Mathf.Max(Timer, 0);// 음수 방지                
            }
            else
            {
                Wave++;
                Timer = 60;
            }
            NotifyTimerClientRpc(Timer, Wave);
        }
    }

    public void GetMoney(int value, HostType type = HostType.All)
    {        
        if(type == HostType.All)
        {
            NotifyGetMoneyClientRpc(value);
        }
        OnMoneyUp?.Invoke();
    }

    public void AddMonster(Monster monster)
    {
        monsters.Add(monster);
        MonsterCount++;
        UpdateMonsterCountOnClients();
    }

    public void RemoveMonster(Monster monster)
    {
        monsters.Remove(monster);
        MonsterCount--;
        UpdateMonsterCountOnClients();
    }

    private void UpdateMonsterCountOnClients()
    {
        NotifyClientMonsterCountClientRpc(MonsterCount);
    }
    [ClientRpc]
    private void NotifyGetMoneyClientRpc(int value)
    {
        Money += value;
        OnMoneyUp?.Invoke();
    }

    [ClientRpc]
    private void NotifyTimerClientRpc(float timer, int wave)
    {
        Timer = timer;
        wave = Wave;
        OnTimerUp?.Invoke();
    }

    [ClientRpc]
    private void NotifyClientMonsterCountClientRpc(int count)
    {
        MonsterCount = count;
    }
}

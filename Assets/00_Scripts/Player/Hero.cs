using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

public class Hero : Character
{
    Hero_Holder parent_holder;

    private double baseATK;
    public double ATK
    {
        get 
        {
            float upgradeBonus = Game_Mng.instance.Upgrade[UpgradeCount()] != 0 ?
                Game_Mng.instance.Upgrade[UpgradeCount()] * 0.1f : 0;
            return baseATK * (1 + upgradeBonus);
        }
        set
        {
            
        }
    }
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public float attackTime = 0.0f;
    public NetworkObject target;
    public LayerMask enemyLayer;
    public Hero_Scriptable m_Data;    
    bool isMove = false;

    public string HeroName;
    public Rarity HeroRarity;
    public Color[] colors;
    public SpriteRenderer circleRenderer;

    [SerializeField] private GameObject spawnParticle;
    Coroutine ATK_Speed_Coroutine;
    float atkSpeed_Coroutine_Value = 1.0f;
    float originalATKSpeed = 1.0f;
    

    private int UpgradeCount()
    {
        switch(m_Data.rare)
        {
            case Rarity.Common:
            case Rarity.UnCommon:
            case Rarity.Rare:
                return 0;
            case Rarity.Hero:
                return 1;
            case Rarity.Legendary:
                return 2;
        }
        return -1;
    }

    public void Initialize(HeroData obj, Hero_Holder holder, string rarity)
    {
        m_Data = Resources.Load<Hero_Scriptable>("Character_Scriptable/" + rarity + "/" + obj.heroName);
        parent_holder = holder;
        baseATK = obj.heroATK;
        attackSpeed = obj.heroATK_Speed;
        attackRange = obj.heroRange;

        HeroName = obj.heroName;
        HeroRarity = (Rarity)Enum.Parse(typeof(Rarity), rarity);
        circleRenderer.color = colors[(int)HeroRarity];

        Game_Mng.instance.AddHero(this);

        GetInitCharacter(obj.heroName, rarity);
        Instantiate(spawnParticle, parent_holder.transform.position , Quaternion.identity);
    }

    public void OnDestroy()
    {
        Game_Mng.instance.RemoveHero(this);
    }

    public void Position_Change(Hero_Holder holder, List<Vector2>poss, int myIndex)
    {
        isMove = true;
        AnimatorChanage("MOVE", false);

        parent_holder = holder;

        if(IsServer)
            transform.parent = holder.transform;

        int sing = (int)Mathf.Sign(poss[myIndex].x - transform.position.x);
        switch(sing)
        {
            case -1: renderer.flipX = true; break;
            case 1: renderer.flipX = false; break;
        }

        StartCoroutine(Move_Corotine(poss[myIndex]));
    }
    private IEnumerator Move_Corotine(Vector2 endPos)
    {
        float current = 0.0f;
        float percent = 0.0f;
        Vector2 start = transform.position;
        Vector2 end = endPos;
        while (percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / 0.5f;       // 0.5초 동안 진행되도록 설정
            Vector2 LerpPos = Vector2.Lerp(start, end, percent);
            transform.position = LerpPos;
            yield return null;
        }
        isMove = false;
        AnimatorChanage("IDLE", false);
        renderer.flipX = true;
    }

    private void Update()
    {
        if (isMove) return;
            CheckForEnemies();
    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(parent_holder.transform.position, attackRange, enemyLayer);
        attackTime += Time.deltaTime * (attackSpeed * atkSpeed_Coroutine_Value);
        if (enemiesInRange.Length > 0)
        {            
            target = enemiesInRange[0].GetComponent<NetworkObject>();            
            if(attackTime >= attackSpeed)
            {
                attackTime = 0.0f;                
                AnimatorChanage("ATTACK", true);
                animator.speed = attackSpeed * atkSpeed_Coroutine_Value;
                GetBullet();                
            }            
        }
        else
        {
            target = null;
        }
    }

    public void SetATKSpeed(float value, float time)
    {
        if (ATK_Speed_Coroutine != null)
        {
            if (value >= atkSpeed_Coroutine_Value)
            {
                StopCoroutine(ATK_Speed_Coroutine);
            }
        }
        if (atkSpeed_Coroutine_Value <= value)
            atkSpeed_Coroutine_Value = value;

        float origin = originalATKSpeed;
        StartCoroutine(ATKSpeedSet(time, origin));
    }

    IEnumerator ATKSpeedSet(float time, float original)
    {
        yield return new WaitForSeconds(time);
        atkSpeed_Coroutine_Value = original;
    }


    public void GetBullet()
    {
        var go = Instantiate(m_Data.HitParticle, (transform.position + new Vector3(0, 0.1f, 0) ), Quaternion.identity);
        go.Init(target.transform, this);
    }

    public void SetDamage()
    {
        if (target != null)
        {
            AttackMonsterServerRpc(target.NetworkObjectId);
            if (m_Data.effectType != null)
            {
                for (int i = 0; i < m_Data.effectType.Length; i++)
                {
                    List<float> values = new List<float>(m_Data.effectType[i].parameters);
                    switch (m_Data.effectType[i].debuffType)
                    {
                        case Debuff.Slow:

                            if (UnityEngine.Random.value <= values[0])
                            {
                                values.RemoveAt(0);
                                target.GetComponent<Monster>().ApplyDebuffServerRpc((int)Debuff.Slow, values.ToArray());
                            }
                            break;


                        case Debuff.Sturn:
                            if (UnityEngine.Random.value <= values[0])
                            {
                                values.RemoveAt(0);
                                target.GetComponent<Monster>().ApplyDebuffServerRpc((int)Debuff.Sturn, values.ToArray());
                            }
                            break;
                    }
                }
            }                        
        }            
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttackMonsterServerRpc(ulong monsterId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monsterId, out var spawnedObject))
        { 
            Monster monster = spawnedObject.GetComponent<Monster>();
            if(monster != null)
            {
                monster.GetDamage(ATK);//
            }
        }
    }


}

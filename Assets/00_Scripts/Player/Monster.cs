using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using System;


public class Monster : Character
{
    public bool Boss;

    [SerializeField] private float m_Speed = 1f;
    private float originalSpeed;
    [SerializeField] private HitText hitText;
    [SerializeField] private Image m_Fill, m_Fill_Deco;

    Coroutine slowCoroutine;
    Coroutine stunCoroutine;
    [SerializeField] private Color slowColor;
    private float currentSlowAmount;
    private float currentSlowDuration;
    private bool isStun = false;
    [SerializeField] private GameObject StunParticle;

    int target_Value = 0;
    public double HP = 0, MaxHP = 100;
    
    private bool isDead = false;

    List<Vector2> move_list = new List<Vector2>();
    

    public override void Awake()
    {
        HP = CalculateMonsterHP(Game_Mng.instance.Wave);
        MaxHP = HP;
        originalSpeed = m_Speed;
        base.Awake();
    }

    // 지수적 증가 공식 : Monster HP
    double CalculateMonsterHP(int waveLevel)
    {
        double baseHP = 100.0f;

        double powerMultiplier = Mathf.Pow(1.1f, waveLevel);

        if(waveLevel % 10 ==0)
        {
            powerMultiplier += 0.05f * (waveLevel / 10);
        }

        return baseHP * powerMultiplier * (Boss? 10 : 1);
    }

    public void Init(List<Vector2> vectorList)
    {
        move_list = vectorList;
    }

    private void Update()
    {
        m_Fill_Deco.fillAmount = Mathf.Lerp(m_Fill_Deco.fillAmount, m_Fill.fillAmount, Time.deltaTime * 2f);
        if (isDead) return;
        if (isStun) return;

        transform.position = Vector2.MoveTowards(transform.position,
                                                 move_list[target_Value],
                                                 Time.deltaTime * m_Speed);

        if (Vector2.Distance(transform.position, move_list[target_Value]) <= 0.0f)
        {
            target_Value++;

            renderer.flipX = target_Value >= 3 ? true : false;

            if (target_Value >= 4)
            {
                target_Value = 0;
            }
        }
    }

    public void GetDamage(double dmg)
    {
        if (!IsServer) return;
        if (isDead) return;

        GetDamageMonster(dmg);        
        NotifyClientUpdateClientRpc(HP -= dmg, dmg);     
    }
    private void GetDamageMonster(double dmg)
    {
        HP -= dmg;
        m_Fill.fillAmount = (float)HP / (float)MaxHP;

        Instantiate(hitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            Game_Mng.instance.GetMoney(1);
            StartCoroutine(Dead_Coroutine());
            AnimatorChanage("DEAD", true);
        }
    }

    [ClientRpc]
    public void NotifyClientUpdateClientRpc(double hp, double dmg)
    {
        HP = hp;
        m_Fill.fillAmount = (float)HP / (float)MaxHP;

        Instantiate(hitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            StartCoroutine(Dead_Coroutine());
            AnimatorChanage("DEAD", true);
        }
    }

    IEnumerator Dead_Coroutine()
    {
        float Alpha = 1.0f;
        while(renderer.color.a > 0.0f)
        {
            Alpha -= Time.deltaTime;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, Alpha);
            yield return null;
        }

        if(IsServer)
        {
            Game_Mng.instance.RemoveMonster(this, Boss);
            this.gameObject.SetActive(false);
            Destroy(this);         
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    #region 상태 이상
    /*슬로우 프로퍼티
     * float slowChance; 캐릭터가 몬스터에게 슬로우를 줄 확률
     * float slowAmount; 몬스터의 속도를 감소시키는 확률
     * float slowDuration; 슬로우 효과가 유지되는 시간
     * float m_Speed; 몬스터의 속도     
     * 
     * 스턴 프로퍼티
     * sturnDuration : 스턴 효과가 유지되는 시간
     */
    [ClientRpc]
    private void ApplySturnClientRpc(float sturnDuration)
    {
        CoroutineStop(stunCoroutine);
        stunCoroutine = StartCoroutine(EffectCoroutine(sturnDuration,
            () =>
            {
                isStun = true;
                StunParticle.SetActive(true);
            },
            () =>
            {                
                isStun = false;
                StunParticle.SetActive(false);
            }
            ));
    }


    [ClientRpc]
    private void ApplySlowClientRpc(float slowAmount, float duration)
    {
        CoroutineStop(slowCoroutine);
        slowCoroutine = StartCoroutine(EffectCoroutine(duration,
        () =>
        {
            float newSpeed = originalSpeed - (originalSpeed * slowAmount);
            newSpeed = Mathf.Max(newSpeed, 0.1f);
            m_Speed = newSpeed;
            renderer.color = slowColor;
        },
        () =>
        {
            m_Speed = originalSpeed;
            renderer.color = Color.white;
        }
        ));
    }

    private void CoroutineStop(Coroutine coroutine)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator EffectCoroutine(float duration, Action FirstAction, Action SecondAction)
    {
        FirstAction?.Invoke();

        yield return new WaitForSeconds(duration);

        SecondAction?.Invoke();
    }
    

    [ServerRpc(RequireOwnership = false)]
    public void ApplyDebuffServerRpc(int debuffType, float[] values)
    {
        Debuff debuff = (Debuff)debuffType;
        switch(debuff)
        {
            case Debuff.Slow:
                if (values[0] > currentSlowAmount || (values[0] == currentSlowAmount && values[1] > currentSlowDuration))
                {
                    currentSlowAmount = values[0];
                    currentSlowDuration = values[1];

                    ApplySlowClientRpc(values[0], values[1]);
                }                
                break;
            case Debuff.Sturn:
                ApplySturnClientRpc(values[0]);
                break;
        }
    }
    #endregion 상태이상
}

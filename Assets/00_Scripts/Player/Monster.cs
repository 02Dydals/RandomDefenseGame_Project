using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;


public class Monster : Character
{
    public bool Boss;

    [SerializeField] private float m_Speed = 1f;
    private float originalSpeed;
    [SerializeField] private HitText hitText;
    [SerializeField] private Image m_Fill, m_Fill_Deco;

    Coroutine slowCoroutine;
    [SerializeField] private Color slowColor;
    private float currentSlowAmount;
    private float currentSlowDuration;

    int target_Value = 0;
    public double HP = 0, MaxHP = 30;
    
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
        double baseHP = 50.0f;

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

    #region 슬로우
    /*슬로우 프로퍼티
     * float slowChance; 캐릭터가 몬스터에게 슬로우를 줄 확률
     * float slowAmount; 몬스터의 속도를 감소시키는 확률
     * float slowDuration; 슬로우 효과가 유지는 시간
     * float m_Speed; 몬스터의 속도     
     */

    [ServerRpc(RequireOwnership = false)]
    public void ApplySlowServerRpc(float slowAmount, float duration)
    {
        if (slowAmount > currentSlowAmount || (slowAmount == currentSlowAmount && duration > currentSlowDuration))
        {
            currentSlowAmount = slowAmount;
            currentSlowDuration = duration;

            ApplySlowClientRpc(slowAmount, duration);
        }
    }

    [ClientRpc]
    private void ApplySlowClientRpc(float slowAmount, float duration)
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowEffectCoroutine(slowAmount, duration));
    }

    private IEnumerator SlowEffectCoroutine(float slowAmount, float duration)
    {
        float newSpeed = originalSpeed - (originalSpeed * slowAmount);
        newSpeed = Mathf.Max(newSpeed, 0.1f);
        m_Speed = newSpeed;
        renderer.color = slowColor;
        yield return new WaitForSeconds(duration);
        m_Speed = originalSpeed;
        renderer.color = Color.white;
    }



    #endregion 슬로우
}

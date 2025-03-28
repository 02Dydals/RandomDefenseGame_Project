using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public SKILL m_State;
    Hero hero;
    Hero_Scriptable m_Data;
    public bool isReady = false;
    public Image Fill;
    

    private void Start()
    {
        hero = GetComponent<Hero>();
        Initalize();
    }

    private void Initalize()
    {
        if (hero.m_Data.skillData.skill == SKILL.None) return;
        if (hero.m_Data.skillData.skill != SKILL.None)
        {            
            m_Data = hero.m_Data;
            m_State = m_Data.skillData.skill;
            Fill.transform.parent.gameObject.SetActive(true);            
            StartCoroutine(SkillDelay());
        }
        else
        {
            Destroy(this);
        }
    }

    private List<Monster> monsters()
    {
        return Game_Mng.instance.monsters;
    }

    private List<Hero> heros()
    {
        return Game_Mng.instance.heros;
    }

    private bool Distance(Vector2 startPos, Vector2 endPos, float checkDistance)
    {
        if(Vector2.Distance(startPos, endPos) <= checkDistance)
        {
            return true;
        }
        return false;
    }

    private float DistanceStep(int value)
    {
        return (Game_Mng.instance.DistanceMagnitude * value) + 0.1f;
    }

    private double SkillDamage()
    {
        return hero.ATK * (m_Data.skillData.SkillDuration / 100);
    }

    IEnumerator SkillDelay()
    {
        float t = 0.0f;
        float cooltime = hero.m_Data.skillData.Cooltime;
        while(t < cooltime)
        {
            t += Time.deltaTime;
            Fill.fillAmount = t / cooltime;
            yield return null;
        }        
        isReady = true;
    }

    private void Update()
    {
        if(hero.target != null && isReady)
        {
            isReady = false;
            StartCoroutine(SkillDelay());
            GetSkill();
        }
    }

    private void GetSkill()
    {
        switch(m_State)
        {
            case SKILL.Gun: Gun(); break;
            case SKILL.Cleric: Cleric(); break;
        }
    }

    #region 스킬 종류
    private void Gun()
    {
        Vector2 pos = hero.target.transform.position;
        Instantiate(m_Data.skillData.Particle, pos, Quaternion.identity);

        for(int i=0; i< monsters().Count; i++)
        {
            if(Distance(pos, monsters()[i].transform.position, 1.0f))
            {
                var monster = monsters()[i];
                float[] values = { 2.0f };
                monster.GetDamage(SkillDamage());
                monster.ApplyDebuffServerRpc(1, values);
            }
        }
    }

    private void Cleric()
    {
        Vector2 pos = transform.position;
        for(int i=0; i<heros().Count; i++)
        {
            var hero = heros()[i];            
            if(Distance(pos, hero.transform.position, DistanceStep(2)))
            {
                Instantiate(m_Data.skillData.Particle, hero.transform.position, Quaternion.identity);
                hero.SetATKSpeed(m_Data.skillData.SkillDuration / 100, 3.0f);
            }
        }
    }

    #endregion 스킬 종류
}

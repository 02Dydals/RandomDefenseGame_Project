using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;


public class UI_Main : MonoBehaviour
{
    public static UI_Main instance = null;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [Header("##Text##")]
    [SerializeField] private TextMeshProUGUI MonsterCount_T;
    [SerializeField] private TextMeshProUGUI Money_T;
    [SerializeField] private TextMeshProUGUI Summon_T;
    [SerializeField] private TextMeshProUGUI Timer_T;
    [SerializeField] private TextMeshProUGUI Wave_T;    
    [SerializeField] private TextMeshProUGUI HeroCount_T;
    [SerializeField] private TextMeshProUGUI Navigation_T;

    [SerializeField] private Image MonsterCount_Image;
    [SerializeField] private Transform Navigation_Content;
    [SerializeField] private Button SummonButton;
    [SerializeField] private Animator MoneyAnimation;
        
    [Header("##Trail Effect##")] 
    [SerializeField] private GameObject TrailPrefab;
    [SerializeField] private float trailSpeed = 5.0f;
    [UnityEngine.Range(0.0f, 30.0f)]
    [SerializeField] private float yPosMin, yPosMax;
    [SerializeField] private float xPos;

    [Header("##Upgrade##")]
    [SerializeField] private TextMeshProUGUI u_Money_T;
    [SerializeField] private TextMeshProUGUI[] u_Upgrade_T;
    [SerializeField] private TextMeshProUGUI[] u_Upgrade_Asset_T;


    List<GameObject> NavigationTextList = new List<GameObject>();
    private void Start()
    {
        Game_Mng.instance.OnMoneyUp += Money_Anim;
        Game_Mng.instance.OnTimerUp += WavePoint;
        SummonButton.onClick.AddListener(() => ClickSummon());
    }

    public void UpgradeButton(int value)
    {
        if (Game_Mng.instance.Money < 30 + Game_Mng.instance.Upgrade[value])
            return;
        Game_Mng.instance.Money -= (30+ Game_Mng.instance.Upgrade[value]);
        Game_Mng.instance.Upgrade[value]++;
    }

    private void ClickSummon()
    {
        if (Game_Mng.instance.Money < Game_Mng.instance.SummonCount) return;
        if (Game_Mng.instance.HeroCount >= Game_Mng.instance.HeroMaximumCount) return;

        Game_Mng.instance.Money -= Game_Mng.instance.SummonCount;
        Game_Mng.instance.SummonCount += 2;
        Game_Mng.instance.HeroCount++; 

        StartCoroutine(SummonCoroutin());
    }

    private Vector3 GenerateRandomControlPoint(Vector3 start, Vector3 end)
    {        
        Vector3 midPoint = (start + end) / 2f;

        float randomHeight = Random.Range(yPosMin, yPosMax);

        midPoint += randomHeight * Vector3.up;

        midPoint += new Vector3(Random.Range(-xPos, xPos), 0.0f);

        return midPoint;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // 베지어 곡선 공식 (1-t)^2 * p0 +2 * (1-t) * t * p1 + t^2 * p2
        return Mathf.Pow(1 - t, 2) * p0 +2 * (1-t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }

    IEnumerator SummonCoroutin()
    {
        var data = Spawner.instance.Data("Common");

        Vector3 buttonWorldPosition = Camera.main.ScreenToWorldPoint(SummonButton.transform.position);
        GameObject trailInstance = Instantiate(TrailPrefab);
        trailInstance.transform.position = buttonWorldPosition;
        Vector3 endPos = Spawner.instance.HolderPosition(data);

        Vector3 startPoint = buttonWorldPosition;
        Vector3 endPoint = endPos;

        Vector3 controlPoint = GenerateRandomControlPoint(startPoint, endPoint);

        float elapsedTime = 0.0f;

        while(elapsedTime < trailSpeed)
        {
            float t = elapsedTime / trailSpeed;

            Vector3 currvePosition = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);

            trailInstance.transform.position = new Vector3(currvePosition.x , currvePosition.y, 0.0f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        Destroy(trailInstance);
        Spawner.instance.Summon("Common", data);
    }


    private void Update()
    {
        MonsterCount_T.text = Game_Mng.instance.MonsterCount.ToString() + "/ 100";
        MonsterCount_Image.fillAmount = (float)Game_Mng.instance.MonsterCount / 100.0f;
        HeroCount_T.text = UpdateHeroCountText();

        Money_T.text = Game_Mng.instance.Money.ToString();
        Summon_T.text = Game_Mng.instance.SummonCount.ToString();
        u_Money_T.text = Game_Mng.instance.Money.ToString();

        for(int i=0; i< u_Upgrade_T.Length; i++)
        {
            u_Upgrade_T[i].text = "Lv."+(Game_Mng.instance.Upgrade[i] + 1).ToString();
            u_Upgrade_Asset_T[i].text = (30 + Game_Mng.instance.Upgrade[i]).ToString();
        }

        Summon_T.color = Game_Mng.instance.Money >= Game_Mng.instance.SummonCount ? Color.white : Color.red;
    }

    public void GetNavigation(string temp)
    {
        if (NavigationTextList.Count > 7)
        {
            Destroy(NavigationTextList[0]);
            NavigationTextList.RemoveAt(0);
        }
        var go = Instantiate(Navigation_T, Navigation_Content);
        NavigationTextList.Add(go.gameObject);
        go.gameObject.SetActive(true);

        go.transform.SetAsFirstSibling();
        Destroy(go.gameObject, 2.5f);
        
        go.text = temp;
    }
    

    public void WavePoint()
    {
        Timer_T.text = UpdateTimerText();
        Wave_T.text = "WAVE : " +Game_Mng.instance.Wave.ToString();
    }

    string UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(Game_Mng.instance.Timer / 60);
        int seconds = Mathf.FloorToInt(Game_Mng.instance.Timer % 60);

        return $"{minutes:00} : {seconds:00}";
    }

    string UpdateHeroCountText()
    {
        int myCount = Game_Mng.instance.HeroCount;
        string temp = "";

        if(myCount < 10)
        {
            temp = "0" + myCount.ToString();
        }
        else
        {
            temp = myCount.ToString();
        }
        return string.Format("{0} / {1}", temp, Game_Mng.instance.HeroMaximumCount);
    }

    private void Money_Anim()
    {
        MoneyAnimation.SetTrigger("GET");
    }

}

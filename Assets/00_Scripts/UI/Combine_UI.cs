using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Combine_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI DocumentText;
    public Combine_Scriptable[] CombineDataArray;

    public Image MainCharacterImage;
    int characterValue;

    public GameObject SubObject;
    public GameObject PlusObject;
    public Transform HorizontalContent;

    List<GameObject> Gorvage = new List<GameObject>();

    private void Start()
    {
        CombineDataArray = Resources.LoadAll<Combine_Scriptable>("Combine");

        Initialize();
    }

    private void Initialize()
    {
        if(Gorvage.Count > 0)
        {
            for(int i=0; i<Gorvage.Count; i++)
            {
                Destroy(Gorvage[i]);
            }
            Gorvage.Clear();
        }

        var combinedata = CombineDataArray[characterValue];
        MainCharacterImage.sprite = Utils.GetAtlas(combinedata.MainData.Name );             

        for(int i=0; i< combinedata.SubDatas.Count; i++)
        {
            var go = Instantiate(SubObject, HorizontalContent);
            go.transform.Find("SubChrarcter_Icon").GetComponent<Image>().sprite =
                Utils.GetAtlas(combinedata.SubDatas[i].Name);

            go.SetActive(true);

            if( i != combinedata.SubDatas.Count -1)
            {
                var plus = Instantiate(PlusObject, HorizontalContent);
                plus.SetActive(true);
                Gorvage.Add(plus);
            }
            Gorvage.Add(go);
        }
    }

    public void Arrow(int value)
    {
        characterValue += value;
        if(characterValue < 0)
        {
            characterValue = CombineDataArray.Length - 1;
        }
        else if(characterValue > CombineDataArray.Length - 1)
        {
            characterValue = 0;
        }

        Initialize();
    }
}

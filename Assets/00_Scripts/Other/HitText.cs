using UnityEngine;
using TMPro;
using System.Collections;

public class HitText : MonoBehaviour
{
    // 텍스트가 올라가는 속도
    [SerializeField] private float floatSpeed;
    // 텍스트가 올라가는 데 걸리는 시간
    [SerializeField] private float riseDuration = 1.0f;
    // 텍스타가 투명해지는데 걸리는 시간
    [SerializeField] private float fadeDuration = 1.0f;

    public Vector3 offset = new Vector3(0,2,0); // 텍스트가 올라가는 거리

    public TextMeshPro damageText;
    private Color textColor;

    public void Initalize(int dmg)
    {
        damageText.text = dmg.ToString();
        textColor = damageText.color;
        StartCoroutine(MoveAndFade());
    }

    IEnumerator MoveAndFade()
    {
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + offset;

        float elapsedTime = 0;
        // 텍스트가 위로 올라가는 메서드
        while(elapsedTime < riseDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / riseDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0;

        // 텍스트 컬러가 투명해지는 메서드
        while(elapsedTime < fadeDuration)
        {
            textColor.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            damageText.color = textColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }
}

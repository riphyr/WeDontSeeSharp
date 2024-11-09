using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LoadingTextAnimation : MonoBehaviour
{
    public GameObject loadingText;
    private float timer;
    private int dotCount;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.5f)
        {
            timer = 0;
            dotCount = (dotCount + 1) % 4;
            loadingText.GetComponent<TMP_Text>().text = "LOADING" + new string('.', dotCount);
        }
    }
}

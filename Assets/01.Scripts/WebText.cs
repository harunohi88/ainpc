using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebText : MonoBehaviour
{
    public Text MyText;
    
    void Start() {
        StartCoroutine(GetText());
    }
 
    IEnumerator GetText()
    {
        string uri = "https://openapi.naver.com/v1/search/news.json?query=백지헌&display=30";
        UnityWebRequest www = UnityWebRequest.Get(uri);
        www.SetRequestHeader("X-Naver-Client-Id", "59pYaAqkfFsF2zQrske3");
        www.SetRequestHeader("X-Naver-Client-Secret", "4SEkyDcnhc");
        
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            byte[] results = www.downloadHandler.data;
            MyText.text = www.downloadHandler.text;
        }
    }
}

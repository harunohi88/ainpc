using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebImage : MonoBehaviour
{
    public RawImage MyImage;
    
    void Start()
    {
        StartCoroutine(GetTexture());
    }
    
    IEnumerator GetTexture()
    {
        string uri = "https://www.kstarfashion.com/news/photo/202505/232069_175909_427.jpg";
        
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            MyImage.texture = myTexture;
            MyImage.SetNativeSize();
        }
    }
}

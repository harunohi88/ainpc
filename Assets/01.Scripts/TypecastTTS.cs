using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class TypecastTTS : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    private string _apiKey;
    private const string _actorId = "622964d6255364be41659078";
    private const string _ttsUrl = "https://typecast.ai/api/speak";

    private void Start()
    {
        _apiKey = APIKeys.TYPECAST_API_KEY;
        _audioSource = GetComponent<AudioSource>();
    }
    
    public async void PlayTypecastTTS(string text)
    {
        await RequestAndPlayAudio(text);
    }

    private async Task RequestAndPlayAudio(string text)
    {
        JObject payload = new JObject
        {
            { "text", text },
            { "lang", "auto" },
            { "actor_id", _actorId },
            { "xapi_hd", true },
            { "model_version", "latest" }
        };

        UnityWebRequest request = new UnityWebRequest(_ttsUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("TTS 요청 실패: " + request.error);
            return;
        }

        string jsonText = request.downloadHandler.text;
        JObject responseJson = JObject.Parse(jsonText);
        string speakUrl = responseJson["result"]?["speak_url"]?.ToString();

        if (string.IsNullOrEmpty(speakUrl))
        {
            Debug.LogError("speak_url 없음: " + jsonText);
            return;
        }

        // Polling 요청
        JObject responseAudioJson = null;
        const int maxRetries = 20;
        const int delayMs = 1000;
        for (int i = 0; i < maxRetries; i++)
        {
            UnityWebRequest audioRequest = UnityWebRequest.Get(speakUrl);
            audioRequest.downloadHandler = new DownloadHandlerBuffer();
            audioRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            await audioRequest.SendWebRequest();

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("오디오 상태 확인 실패: " + audioRequest.error);
                return;
            }

            string jsonAudioText = audioRequest.downloadHandler.text;
            responseAudioJson = JObject.Parse(jsonAudioText);
            string status = responseAudioJson["result"]?["status"]?.ToString();

            Debug.Log($"TTS 상태: {status}");

            if (status == "done")
            {
                break;
            }

            await Task.Delay(delayMs);
        }

        string audioUrl = responseAudioJson?["result"]?["audio"]?["url"]?.ToString();
        if (string.IsNullOrEmpty(audioUrl))
        {
            Debug.LogError("오디오 다운로드 URL 없음");
            return;
        }
        
        Debug.Log("오디오 다운로드 URL: " + audioUrl);

        var audioDownloadRequest = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.WAV);
        audioDownloadRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        await audioDownloadRequest.SendWebRequest();

        if (audioDownloadRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("오디오 다운로드 실패: " + audioDownloadRequest.error);
            return;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(audioDownloadRequest);
        if (clip == null)
        {
            Debug.LogError("AudioClip 변환 실패");
            return;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
    }
}


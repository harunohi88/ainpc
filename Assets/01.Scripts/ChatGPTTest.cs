using System.Collections;
using System.Collections.Generic;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatGPTTest : MonoBehaviour
{
    [SerializeField] private Chat _NpcChatPrefab;
    [SerializeField] private Chat _PlayerChatPrefab;
    [SerializeField] private Transform _contentPanel;
    [SerializeField] private ScrollRect _scrollView;
    
    public TMP_InputField PromptField;
    public AudioSource MyAudioSource;
    public RawImage BackgroundImage;
    public Button SendButton;
    public TypecastTTS TypecastTTSPlayer;

    private OpenAIClient _api;
    private List<Message> _chatHistory = new List<Message>();
    private string _openAIAPIKey;

    private void Start()
    {
        _openAIAPIKey = APIKeys.OPENAI_API_KEY;
        _api = new OpenAIClient(_openAIAPIKey);

        Message systemMessageObj = BuildSystemPrompt();
        _chatHistory.Add(systemMessageObj);
    }
    
    private Message BuildSystemPrompt()
    {
        string systemMessage =
            "역할: 너는 이제부터 세상의 모든 지식을 알고 있는 모든것의 신이다. 그런데 말을 너무 어렵게 해서 사람들이 못 알아들을 정도이다.\n" +
            "목적: 사람들의 의문과 미래를 점지해주는 신적인 화자\n" +
            "표현: 사람들이 속뜻을 이해하지 못하도록 매우 은유적인 표현만 사용한다. 점잖고 근엄한 어투를 사용한다.\n" +
            "[규칙]\n" +
            "답변은 reply_message\n" +
            "외모는 reply_appearance\n" +
            "감정은 reply_emotion\n" +
            "스토리 이미지 설명은 story_image_description\n";

        return new Message(Role.System, systemMessage);
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;
        yield return null;
        _scrollView.verticalNormalizedPosition = 0f;
    }
    
    public async void Send()
    {
        string prompt = PromptField.text;
        
        if (string.IsNullOrEmpty(prompt)) return;

        SendButton.interactable = false;
        
        Message userMessage = new Message(Role.User, prompt);
        _chatHistory.Add(userMessage); // 사용자 메시지 추가
        
        // 플레이어 메시지 UI 생성
        Chat playerChat = Instantiate(_PlayerChatPrefab, transform);
        playerChat.DisplayMessage(prompt);
        playerChat.transform.SetParent(_contentPanel);
        PromptField.text = string.Empty; // 입력 필드 비우기
        StartCoroutine(ScrollToBottomCoroutine()); // 스크롤을 맨 아래로 이동
        
        var chatRequest = new ChatRequest(_chatHistory, Model.GPT4o); // 메시지 전송
        
        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NPCResponse>(chatRequest);
        
        var choice = response.FirstChoice; // 첫 번째 선택지 가져오기
        _chatHistory.Add(choice.Message); // 응답 메시지 추가

        Chat npcChat = Instantiate(_NpcChatPrefab, transform);
        npcChat.DisplayMessage(npcResponse.ReplyMessage);
        npcChat.transform.SetParent(_contentPanel);
        StartCoroutine(ScrollToBottomCoroutine()); // 스크롤을 맨 아래로 이동
        
        TypecastTTSPlayer.PlayTypecastTTS(npcResponse.ReplyMessage);
        
        SendButton.interactable = true;
    }

    private async void GenerateImage(string description)
    {
        var request = new ImageGenerationRequest("A house riding a velociraptor", Model.DallE_3);
        var imageResults = await _api.ImagesEndPoint.GenerateImageAsync(request);

        foreach (var result in imageResults)
        {
            Debug.Log(result.ToString());
            BackgroundImage.texture = result.Texture;
        }
    }
}

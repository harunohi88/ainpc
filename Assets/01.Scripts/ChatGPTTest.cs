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

        string systemMessage = "역할: 너는 이제부터 게임 NPC다. 자신을 실제 게임 속 고양이 인간이라고 생각한다.";
        systemMessage += "목적: 실제 사람처럼 대화하는 게임 NPC 모드";
        systemMessage += "표현: 말 끝마다 '냥~'을 붙여서 말한다. 예를 들어, '안녕하세요'라고 말하면 '안녕하세요냥~'이라고 말한다.";
        systemMessage += "[규칙]";
        systemMessage += "답변은 reply_message";
        systemMessage += "외모는 reply_appearance";
        systemMessage += "감정은 reply_emotion";
        systemMessage += "스토리 이미지 설명은 story_image_description";
        
        Message systemMessageObj = new Message(Role.System, systemMessage);
        _chatHistory.Add(systemMessageObj); // 시스템 메시지 추가
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
        
        // var chatRequest = new ChatRequest(_chatHistory, Model.GPT4oAudioMini, audioConfig:Voice.Alloy); // 메시지 전송
        var chatRequest = new ChatRequest(_chatHistory, Model.GPT4o); // 메시지 전송
        
        // var response = await _api.ChatEndpoint.GetCompletionAsync(chatRequest); // 응답 받기
        var (npcResponse, response) = await _api.ChatEndpoint.GetCompletionAsync<NPCResponse>(chatRequest);
        
        var choice = response.FirstChoice; // 첫 번째 선택지 가져오기
        _chatHistory.Add(choice.Message); // 응답 메시지 추가

        // NPC 메시지 UI 생성
        Chat npcChat = Instantiate(_NpcChatPrefab, transform);
        npcChat.DisplayMessage(npcResponse.ReplyMessage);
        npcChat.transform.SetParent(_contentPanel);
        
        PromptField.text = string.Empty; // 입력 필드 비우기
        
        TypecastTTSPlayer.PlayTypecastTTS(npcResponse.ReplyMessage);
        // GenerateImage(npcResponse.StoryImageDescription);
        
        SendButton.interactable = true;
        // MyAudioSource.PlayOneShot(response.FirstChoice.Message.AudioOutput.AudioClip); 
    }

    private async void PlayTTS(string text)
    {
        var request = new SpeechRequest(text);
        var speechClip = await _api.AudioEndpoint.GetSpeechAsync(request);
        MyAudioSource.PlayOneShot(speechClip);
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

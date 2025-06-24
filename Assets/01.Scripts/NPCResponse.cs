using Newtonsoft.Json;
using UnityEngine;

public class NPCResponse
{
    [JsonProperty("reply_message")]
    public string ReplyMessage { get; set; }
    
    [JsonProperty("reply_appearance")]
    public string Appearance { get; set; }

    [JsonProperty("reply_emotion")]
    public string Emotion { get; set; }
    
    [JsonProperty("story_image_description")]
    public string StoryImageDescription { get; set; }
}

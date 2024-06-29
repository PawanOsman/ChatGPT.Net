using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatGPT.Net.DTO.ChatGPT;

[JsonConverter(typeof(ChatGptMessageConverter))]
public class ChatGptMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = ChatGptMessageRoles.USER;

    [JsonProperty("content")]
    private string Content { get; set; }

    public List<ChatGptMessageContentItem> ContentItems { get; set; } = new();

    public ChatGptMessage(string message)
    {
        Content = message;
        if(ContentItems is null)
            ContentItems = new();
    }
    public ChatGptMessage() 
    {
        ContentItems = new();
    
    }

    public string GetContent
        => Content ?? string.Empty;
    

}



public class ChatGptMessageContentItem
{
    /// <summary>
    /// Possible options: <see cref="ChatGptMessageContentType"/>
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Null/Empty when <see cref="ImageUrl"/> is provided. Only once be provided at a time
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }

    /// <summary>
    /// Null/Empty when <see cref="Text"/> is provided. Only once be provided at a time
    /// </summary>
    [JsonProperty("image_url")]
    private string ImageUrl { get; set; }

    /// <summary>
    /// Sets ImageUrl from file path
    /// </summary>
    /// <param name="filePath">Full path to file. eg, C:/MyFiles/image1.png</param>
    public void SetImageFromFile(string filePath)
    {
        var fileContent = File.ReadAllBytes(filePath);
        ImageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(fileContent);
    }

    /// <summary>
    /// Sets ImageUrl from URL
    /// </summary>
    /// <param name="url">Image URL</param>
    public void SetImageFromUrl(string url) 
    { 
        ImageUrl = url;
    }

    [JsonIgnore]
    public string GetImageUrl
        => ImageUrl;
}

public class ChatGptMessageContentType
{
    public const string TEXT = "text";
    public const string IMAGE = "image_url";
}

public class ChatGptMessageConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(ChatGptMessage));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }
        var chatMessage = (ChatGptMessage)value;
        var jsonObject = new JObject();

        jsonObject["role"] = chatMessage.Role;

        if (!string.IsNullOrEmpty(chatMessage.GetContent))
        {
            jsonObject["content"] = chatMessage.GetContent;
        }
        else
        {
            jsonObject["content"] = JToken.FromObject(chatMessage.ContentItems, serializer);
        }

        jsonObject.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        
        var chatMessage = new ChatGptMessage();
        if (jsonObject["content"].Type == JTokenType.String)
        {
            chatMessage = new(jsonObject["content"].ToString());
            
        }
        else if(jsonObject["content"].Type == JTokenType.Array)
        {
            chatMessage.ContentItems = jsonObject["content"].ToObject<List<ChatGptMessageContentItem>>(serializer);
        }
        else
        {
            chatMessage.ContentItems = new List<ChatGptMessageContentItem>(); //nothing in cotnentitems and nothing in content
        }

        chatMessage.Role = jsonObject["role"].ToString();
        return chatMessage;
    }

    
}
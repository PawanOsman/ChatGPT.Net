using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChatGPT.Net.DTO;
using ChatGPT.Net.DTO.ChatGPT;
using ChatGPT.Net.DTO.ChatGPTUnofficial;
using Newtonsoft.Json;

namespace ChatGPT.Net;

public class ChatGpt
{
    public Guid SessionId { get; set; }
    public ChatGptOptions Config { get; set; } = new();
    public List<ChatGptConversation> Conversations { get; set; } = new();
    public string APIKey { get; set; }

    public ChatGpt(string apikey, ChatGptOptions? config = null)
    {
        Config = config ?? new ChatGptOptions();
        SessionId = Guid.NewGuid();
        APIKey = apikey;
    }

    private async IAsyncEnumerable<string> StreamCompletion(Stream stream)
    {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                yield return line;
            }
        }
    }

    public void SetConversationSystemMessage(string conversationId, string message)
    {
        var conversation = GetConversation(conversationId);
        conversation.Messages.Add(new ChatGptMessage
        {
            Role = "system",
            Content = message
        });
    }

    public void ReplaceConversationSystemMessage(string conversationId, string message)
    {
        var conversation = GetConversation(conversationId);
        conversation.Messages = conversation.Messages.Where(x => x.Role != "system").ToList();
        conversation.Messages.Add(new ChatGptMessage
        {
            Role = "system",
            Content = message
        });
    }
    
    public void RemoveConversationSystemMessages(string conversationId, string message)
    {
        var conversation = GetConversation(conversationId);
        conversation.Messages = conversation.Messages.Where(x => x.Role != "system").ToList();
    }
    
    public List<ChatGptConversation> GetConversations()
    {
        return Conversations;
    }

    public void SetConversations(List<ChatGptConversation> conversations)
    {
        Conversations = conversations;
    }

    public ChatGptConversation GetConversation(string? conversationId)
    {
        if (conversationId is null)
        {
            return new ChatGptConversation();
        }

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null) return conversation;
        conversation = new ChatGptConversation()
        {
            Id = conversationId
        };
        Conversations.Add(conversation);

        return conversation;
    }
    
    public void SetConversation(string conversationId, ChatGptConversation conversation)
    {
        var conv = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conv != null)
        {
            conv = conversation;
        }
        else
        {
            Conversations.Add(conversation);
        }
    }
    
    public void RemoveConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null)
        {
            Conversations.Remove(conversation);
        }
    }

    public void ResetConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation == null) return;
        conversation.Messages = new();
    }

    public void ClearConversations()
    {
        Conversations.Clear();
    }

    public async Task<string> Ask(string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        conversation.Messages.Add(new ChatGptMessage
        {
            Role = "user",
            Content = prompt
        });
        
        var reply = await SendMessage(new ChatGptRequest
        {
            Messages = conversation.Messages,
            Model = Config.Model,
            Stream = false,
            Temperature = Config.Temperature,
            TopP = Config.TopP,
            FrequencyPenalty = Config.FrequencyPenalty,
            PresencePenalty = Config.PresencePenalty,
            Stop = Config.Stop,
            MaxTokens = Config.MaxTokens,
        });
        
        conversation.Updated = DateTime.Now;

        return reply.Choices.FirstOrDefault()?.Message.Content ?? "";
    }

    public async Task<string> AskStream(Action<string> callback, string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        conversation.Messages.Add(new ChatGptMessage
        {
            Role = "user",
            Content = prompt
        });

        var reply = await SendMessage(new ChatGptRequest
        {
            Messages = conversation.Messages,
            Model = Config.Model,
            Stream = true,
            Temperature = Config.Temperature,
            TopP = Config.TopP,
            FrequencyPenalty = Config.FrequencyPenalty,
            PresencePenalty = Config.PresencePenalty,
            Stop = Config.Stop,
            MaxTokens = Config.MaxTokens,
        }, response =>
        {
            var content = response.Choices.FirstOrDefault()?.Delta.Content;
            if (content is null) return;
            if (!string.IsNullOrWhiteSpace(content)) callback(content);
        });
        
        conversation.Updated = DateTime.Now;

        return reply.Choices.FirstOrDefault()?.Message.Content ?? "";
    }

    public async Task<ChatGptResponse> SendMessage(ChatGptRequest requestBody, Action<ChatGptStreamChunkResponse>? callback = null)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Config.BaseUrl}/v1/chat/completions"),
            Headers =
            {
                {"Authorization", $"Bearer {APIKey}" }
            },
            Content = new StringContent(JsonConvert.SerializeObject(requestBody))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };

        var response = await client.SendAsync(request,HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        if (requestBody.Stream)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != "text/event-stream")
            {
                var error = await response.Content.ReadFromJsonAsync<ChatGptResponse>();
                throw new Exception(error?.Error?.Message ?? "Unknown error");
            }

            var concatMessages = string.Empty;
            
            ChatGptStreamChunkResponse? reply = null;
            var stream = await response.Content.ReadAsStreamAsync();
            await foreach (var data in StreamCompletion(stream))
            {
                var jsonString = data.Replace("data: ", "");
                if (string.IsNullOrWhiteSpace(jsonString)) continue;
                if(jsonString == "[DONE]") break;
                reply = JsonConvert.DeserializeObject<ChatGptStreamChunkResponse>(jsonString);
                if (reply is null) continue;
                concatMessages += reply.Choices.FirstOrDefault()?.Delta.Content;
                callback?.Invoke(reply);
            }
            
            return new ChatGptResponse
            {
                Id = reply?.Id ?? Guid.NewGuid().ToString(),
                Model = reply?.Model ?? "gpt-3.5-turbo",
                Created = reply?.Created ?? 0,
                Choices = new List<Choice>
                {
                    new()
                    {
                        Message = new ChatGptMessage
                        {
                            Content = concatMessages
                        }
                    }
                }
            };
        }

        var content = await response.Content.ReadFromJsonAsync<ChatGptResponse>();
        if(content is null) throw new Exception("Unknown error");
        if(content.Error is not null) throw new Exception(content.Error.Message);
        return content;
    }
}

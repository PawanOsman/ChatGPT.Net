using ChatGPT.Net.DTO.ChatGPT;

namespace ChatGpt.Net.Tests.Official
{
    public class ChatCompletionTests
    {
        public ChatGptOptions options;
        public string ApiKey;
        public ChatCompletionTests()
        {
            options = new()
            {
                BaseUrl = "https://api.openai.com",         //The base URL for the OpenAI API
                Model = ChatGptModels.GPT_3_5_Turbo,        // The specific model to use
                Temperature = 0.7,                          // Controls randomness in the response (0-1)
                TopP = 0.9,                                 // Controls diversity in the response (0-1)
                MaxTokens = 3500,                           // The maximum number of tokens in the response
                Stop = null,                                // Sequence of tokens that will stop generation
                PresencePenalty = 0.0,                      // Penalizes new tokens based on their existing presence in the context
                FrequencyPenalty = 0.0                      // Penalizes new tokens based on their frequency in the context
            };
            ApiKey = Environment.GetEnvironmentVariable("API_KEY");

        }

        [Fact]
        public async Task ChatCompletion_ValidConfig_IsSuccessResponse()
        {
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);
            
            Assert.NotNull(bot);
            
            var response = await bot.AskStream(Console.Write, "Who are you?", "default");
            Assert.NotNull(response);

        }

        [Fact]
        public async Task ChatCompletion_ConversationContext_IsMaintained()
        {
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);

            var myFirstQuestion = "Who are you?";
            var response = await bot.AskStream(Console.Write, myFirstQuestion, "default");
            Assert.NotNull(response);

            response = await bot.AskStream(Console.Write, "Where are you from?", "default");
            Assert.NotNull(response);

            response = await bot.AskStream(Console.Write, "Can you remind me what was my first question to you? Just respond witht the question text, do not add anything else.", "default");
            Assert.Equal(myFirstQuestion.ToLower(), response.ToLower().Trim(' ', '\\', '"'));

        }

        [Fact]
        public async Task ChatCompletion_SystemMessage_IsSet()
        {
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);
            bot.SetConversationSystemMessage("default", "You are a helpful assistant. When I ask you 'who am I?', you simply respond with a word 'cool-dev'. Do not add anything else");

            var response = await bot.AskStream(Console.Write, "Who am I?", "default");
            Assert.NotNull(response);
            Assert.Equal("cool-dev", response);
        }

        [Fact]
        public async Task ChatCompletion_WithImage_IsWorking()
        {
            options.Model = ChatGptModels.GPT_4_Vision_Preview;
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);

            var contentItems = new List<ChatGptMessageContentItem>();
            contentItems.Add(new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.TEXT,
                Text = "what is this image about?"
            });

            var contentItemWithImage = new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.IMAGE
            };
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chocolate.png");
            contentItemWithImage.SetImageFromFile(filePath);
            contentItems.Add(contentItemWithImage);

            var response = await bot.AskStream(Console.Write, contentItems, "default");
            Assert.NotNull(response);
            Assert.True(response.ToLower().Contains("kit kat"));
        }

        [Fact]
        public async Task ConversationContext_WithSystemAndUserAndImageMixedContent_IsWorking()
        {
            options.Model = ChatGptModels.GPT_4_Vision_Preview;
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);
            var conversationId = "default";
            bot.SetConversationSystemMessage(conversationId, "You are a helpful assistant");

            var contentItems = new List<ChatGptMessageContentItem>();
            contentItems.Add(new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.TEXT,
                Text = "what is this image about?"
            });

            var contentItemWithImage = new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.IMAGE
            };
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chocolate.png");
            contentItemWithImage.SetImageFromFile(filePath);
            contentItems.Add(contentItemWithImage);

            //Calling overload expecting "ContentItems" for image based inputs etc. on same conversation thread.
            var response = await bot.AskStream(Console.Write, contentItems, conversationId);
            Assert.NotNull(response);
            Assert.True(response.ToLower().Contains("kit kat"));

            //Calling overload expecting simple string prompt inputs on same conversation thread.
            response = await bot.AskStream(Console.Write, "Say again, what is the name of this chocolate?", conversationId);
            Assert.NotNull(response);
            Assert.True(response.ToLower().Contains("kit kat"));

        }

        [Fact]
        public async Task ChatCompletionWithImage_UsingUnsupportedModel_ThrowsException()
        {
            options.Model = ChatGptModels.GPT_4o;
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);

            var contentItems = new List<ChatGptMessageContentItem>();
            contentItems.Add(new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.TEXT,
                Text = "what is this image about?"
            });

            var contentItemWithImage = new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.IMAGE
            };
            contentItemWithImage.SetImageFromUrl("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII");
            contentItems.Add(contentItemWithImage);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                                await bot.AskStream(Console.Write, contentItems, "default"));
            Assert.True(exception.Message.ToLower().Contains("invalid model"));
        }

        [Fact]
        public async Task ChatCompletionWithImage_UsingInvalidContentItems_ThrowsException()
        {
            options.Model = ChatGptModels.GPT_4_Vision_Preview;
            var bot = new ChatGPT.Net.ChatGpt(ApiKey, options);

            var contentItems = new List<ChatGptMessageContentItem>();
            contentItems.Add(new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.TEXT,
                Text = "what is this image about?"
            });

            var contentItemWithImage = new ChatGptMessageContentItem()
            {
                Type = ChatGptMessageContentType.IMAGE
            };
            contentItemWithImage.SetImageFromUrl("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII");
            
            //Intentionally setting "text" property of "image_url" type of content item. Invalid operation exception should be thrown
            contentItemWithImage.Text = "What is this image?";

            contentItems.Add(contentItemWithImage);

            

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                                await bot.AskStream(Console.Write, contentItems, "default"));
            Assert.True(exception.Message.ToLower().Contains("invalid operation"));
        }


    }
}
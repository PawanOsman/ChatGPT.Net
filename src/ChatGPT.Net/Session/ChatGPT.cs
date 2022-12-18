using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChatGPT.Net.DTO;
using ChatGPT.Net.Extensions;
using Microsoft.Playwright;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatGPT.Net.Session;

public class ChatGpt
{
    public bool Ready { get; set; } = false;
    private string ConversationId { get; set; }
    private string ParentMessageId { get; set; } = Guid.NewGuid().ToString();
    private string UserAgent { get; set; }
    private string CfClearance { get; set; }
    private string SessionToken { get; set; }
    private string AccessToken { get; set; }
    private bool Incognito { get; set; }
    private IBrowserContext BrowserContext { get; set; }
    private IBrowser Browser { get; set; }
    private IPage Page { get; set; }
    private Account Account { get; set; }
    private string DataDir { get; set; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private IDictionary<string, string> Headers { get; set; }
    private IDictionary<string, string> Cookies { get; set; }
    private IDictionary<string, string> Proxies { get; set; }

    public ChatGpt(ChatGptConfig config)
    {
        UserAgent = config.UserAgent;
        Account = config.Account;
        DataDir = config.DataDir;
        CfClearance = config.CfClearance;
        AccessToken = config.AccessToken;
        SessionToken = config.SessionToken;
        Headers = config.Headers;
        Cookies = config.Cookies;
        Proxies = config.Proxies;
        Incognito = config.Incognito;
        Init();
    }

    public async Task WaitForReady()
    {
        while (!Ready) await Task.Delay(25);
    }

    private void Init()
    {
        Task.Run(async () =>
        {
            var tries = 0;
            var playwright = await Playwright.CreateAsync();
            Console.WriteLine("Started getting CF Cookies...");
            if (Incognito)
            {
                Browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions()
                    {
                        Headless = false,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox"
                        }
                    });
            }
            else
            {
                BrowserContext = await playwright.Chromium.LaunchPersistentContextAsync(string.IsNullOrWhiteSpace(DataDir) ? Path.Combine(Directory.GetCurrentDirectory(), "Data") : DataDir,
                    new BrowserTypeLaunchPersistentContextOptions
                    {
                        Headless = false,
                        // UserAgent = UserAgent,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox"
                        },
                        ScreenSize = new ScreenSize
                        {
                            Height = 768,
                            Width = 1024
                        },
                        HasTouch = true,
                        JavaScriptEnabled = true,
                        BypassCSP = true,
                        AcceptDownloads = true,
                        Geolocation = new Geolocation
                        {
                            Accuracy = 0.74f,
                            Latitude = 37.0902f,
                            Longitude = 95.7129f
                        }
                    });
            }

            while (true)
            {
                tries++;
                Console.WriteLine($"Try {tries}...");
                
                if(Incognito) Page = await Browser.NewPageAsync();
                else Page = await BrowserContext.NewPageAsync();
                await AsyncStealth(Page, new StealthConfig(), false);
                await Page.SetViewportSizeAsync(1024, 768);
                await Page.GotoAsync("https://chat.openai.com");
                var res = await AsyncCfRetry(Page);
                if (res)
                {
                    Console.WriteLine("CF challenge passed.");
                    Console.WriteLine("Searching for CF clearance cookie...");
                    var cookies = await Page.Context.CookiesAsync();
                    string cfClearance = null;
                    foreach (var cookie in cookies)
                    {
                        if (cookie.Name != "cf_clearance") continue;
                        cfClearance = cookie.Value;
                        Console.WriteLine($"CF clearance value: {cfClearance}");
                    }

                    var ua = await Page.EvaluateAsync<string>("() => navigator.userAgent");
                    Console.WriteLine($"User-Agent: {ua}");
                    var title = await Page.TitleAsync();
                    if (title == "Please Wait... | Cloudflare")
                    {
                        Console.WriteLine("CF clearance not yet available. Retrying...");
                        continue;
                    }

                    Console.WriteLine("Retrieved CF clearance successfully.");

                    UserAgent = ua;
                    CfClearance = cfClearance;

                    var cookiesData = await Page.Context.CookiesAsync();
                    
                    var loggedIn = cookiesData.Any(x => x.Name == "__Secure-next-auth.session-token");

                    if (!loggedIn)
                    {
                        switch (Account.Type)
                        {
                            case AccountType.Email:
                                // TODO: Implement email login
                                throw new Exception("Email login not yet implemented.");
                                break;
                            case AccountType.Gmail:
                                throw new Exception("Gmail login not yet implemented.");
                                break;
                            case AccountType.Microsoft:
                                await Page.GotoAsync("https://chat.openai.com/auth/login");
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button,
                                    new() { NameString = "Continue with Microsoft Account" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByPlaceholder("Email, phone, or Skype").ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByPlaceholder("Email, phone, or Skype").FillAsync(Account.Email);
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Next" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByPlaceholder("Password").ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByPlaceholder("Password").FillAsync(Account.Password);
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Sign in" }).ClickAsync();
                                
                            // {
                            //     await Page.WaitForSelectorAsync(".displaySign");
                            //     var code = await (await Page.QuerySelectorAsync("#idRemoteNGC_DisplaySign")).InnerTextAsync();
                            //     Console.WriteLine($"Verification Code is: {code}");
                            // }
                                
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Yes" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Next" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Next" }).ClickAsync();
                                await Task.Delay(500);
                                await Page.GetByRole(AriaRole.Button, new() { NameString = "Done" }).ClickAsync();
                                await Task.Delay(500);
                                Console.WriteLine("Write your question:");
                                break;
                        }
                    }

                    await Page.GotoAsync("https://chat.openai.com");
                    Ready = true;
                    await Task.Delay(1800000);
                }

                Ready = false;
                Console.WriteLine("CF challenge failed. Retrying...");
                await Page.CloseAsync();
            }
        });
    }

    private static async Task AsyncStealth(IPage pageOrContext, StealthConfig config, bool pure = true)
    {
        foreach (var script in config.EnabledScripts)
        {
            await pageOrContext.AddInitScriptAsync(script);
        }

        if (pure)
        {
            await pageOrContext.RouteAsync(
                new Regex(@"(.*\.png(\?.*|$))|(.*\.jpg(\?.*|$))|(.*\.jpeg(\?.*|$))|(.*\.css(\?.*|$))"),
                route => route.AbortAsync("blockedbyclient"));
        }
    }

    private static async Task<bool> AsyncCfRetry(IPage page, int tries = 10)
    {
        var success = false;
        while (tries > 0)
        {
            await page.WaitForTimeoutAsync(1500);
            try
            {
                success = (await page.QuerySelectorAsync("#challenge-form")) == null;
            }
            catch (Exception e)
            {
                success = false;
            }

            if (success)
            {
                break;
            }

            tries--;
        }

        return success;
    }

    public async Task<string> Ask(string prompt)
    {
        await WaitForReady();
        var conversation = new Conversation
        {
            ParentMessageId = ParentMessageId,
            Messages = new[]
            {
                new Message
                {
                    Content = new Content
                    {
                        Parts = new[] { prompt }
                    }
                }
            }
        };

        return await GetChatResponseAsync(conversation);
    }

    private async Task<string> GetChatResponseAsync(Conversation conversation)
    {
        var fetchData = "await SendMessage(BODY_DATA)".Replace("BODY_DATA", JsonConvert.ToString(JsonConvert.SerializeObject(conversation)));

        var response = await Page.EvaluateAsync<string>($"async () => {fetchData}");

        var data = response.Split("\n")?.ToList().Where(x => !string.IsNullOrEmpty(x) && !x.Contains("data: [DONE]"))
            .LastOrDefault()?.Substring(5);
        
        var reply = JsonSerializer.Deserialize<Reply>(data, _jsonSerializerOptions);
        ConversationId = reply.ConversationId;
        ParentMessageId = reply.Message.Id;

        return reply.Message.Content.Parts.FirstOrDefault();
    }

    public void ResetConversation()
    {
        ConversationId = null;
        ParentMessageId = Guid.NewGuid().ToString();
    }
}
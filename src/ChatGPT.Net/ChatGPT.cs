using System.Text.Json;
using System.Text.RegularExpressions;
using ChatGPT.Net.DTO;
using ChatGPT.Net.Enums;
using ChatGPT.Net.Session;
using Microsoft.Playwright;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatGPT.Net;

public class ChatGpt
{
    public List<ChatGptMessage> Messages { get; set; } = new();
    public bool UseCache { get; set; } = true;
    public bool SaveCache { get; set; } = true;
    public bool Ready { get; set; } = false;
    private string UserAgent { get; set; }
    private string CfClearance { get; set; }
    private bool Invisible { get; set; }
    private bool Incognito { get; set; }
    private List<ChatGptClient> ChatGptClients { get; set; } = new();
    private IBrowserContext BrowserContext { get; set; }
    private IBrowser Browser { get; set; }
    private IPage Page { get; set; }
    private string DataDir { get; set; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    public ChatGpt(ChatGptConfig config = null)
    {
        config ??= new ChatGptConfig();
        DataDir = config.DataDir;
        Incognito = config.Incognito;
        Invisible = config.Invisible;
        UseCache = config.UseCache;
        SaveCache = config.SaveCache;                
        if (SaveCache)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await WaitForReady();
                    var json = JsonConvert.SerializeObject(Messages, Formatting.Indented);
                    await File.WriteAllTextAsync("cache.json", json);
                    await Task.Delay(30000);
                }
            });
        }
        Init();
    }

    public async Task WaitForReady()
    {
        while (!Ready) await Task.Delay(25);
    }

    public bool GetReadyState()
    {
        return Ready;
    }

    private void Init()
    {
        Task.Run(async () =>
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "cache.json")))
            {
                var fileContent = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "cache.json"));
                Messages = JsonConvert.DeserializeObject<List<ChatGptMessage>>(fileContent) ?? new();
            }
            var tries = 0;
            var playwright = await Playwright.CreateAsync();
            Console.WriteLine("Started getting CF Cookies...");
            var browserArgs = new[]
            {
                "--no-sandbox",
                "--disable-setuid-sandbox"
            };

            if(Invisible)
                browserArgs = browserArgs.Append("--window-position=-4096,-4096").ToArray();

            if (Incognito)
            {
                Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
                {
                    Headless = false,
                    Args = browserArgs
                });
            }
            else
            {
                BrowserContext = await playwright.Chromium.LaunchPersistentContextAsync(string.IsNullOrWhiteSpace(DataDir) ? Path.Combine(Directory.GetCurrentDirectory(), "Data") : DataDir,
                    new BrowserTypeLaunchPersistentContextOptions
                    {
                        Headless = false,
                        Args = browserArgs,
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
                await Page.AddInitScriptAsync("async function sendMessage(e,t,r,n){return await(await fetch(\"https://chat.openai.com/backend-api/conversation\",{headers:{accept:\"text/event-stream\",\"accept-language\":\"en-US,en;q=0.9\",authorization:`Bearer ${n}`,\"cache-control\":\"no-cache\",\"content-type\":\"application/json\",pragma:\"no-cache\",\"sec-ch-ua\":'\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\"',\"sec-ch-ua-mobile\":\"?0\",\"sec-ch-ua-platform\":'\"Windows\"',\"sec-fetch-dest\":\"empty\",\"sec-fetch-mode\":\"cors\",\"sec-fetch-site\":\"same-origin\",\"x-openai-assistant-app-id\":\"\"},referrer:\"https://chat.openai.com/chat\",referrerPolicy:\"strict-origin-when-cross-origin\",body:`{\"action\":\"next\",\"messages\":[{\"id\":\"${t}\",\"role\":\"user\",\"content\":{\"content_type\":\"text\",\"parts\":[\"${e}\"]}}],\"parent_message_id\":\"${r}\",\"model\":\"text-davinci-002-render\"}`,method:\"POST\",mode:\"cors\",credentials:\"include\"})).text()}async function sendMessageByConversation(e,t,r,n,c){return await(await fetch(\"https://chat.openai.com/backend-api/conversation\",{headers:{accept:\"text/event-stream\",\"accept-language\":\"en-US,en;q=0.9\",authorization:`Bearer ${n}`,\"cache-control\":\"no-cache\",\"content-type\":\"application/json\",pragma:\"no-cache\",\"sec-ch-ua\":'\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\"',\"sec-ch-ua-mobile\":\"?0\",\"sec-ch-ua-platform\":'\"Windows\"',\"sec-fetch-dest\":\"empty\",\"sec-fetch-mode\":\"cors\",\"sec-fetch-site\":\"same-origin\",\"x-openai-assistant-app-id\":\"\"},referrer:\"https://chat.openai.com/chat\",referrerPolicy:\"strict-origin-when-cross-origin\",body:`{\"action\":\"next\",\"messages\":[{\"id\":\"${t}\",\"role\":\"user\",\"content\":{\"content_type\":\"text\",\"parts\":[\"${e}\"]}}],\"conversation_id\":\"${c}\",\"parent_message_id\":\"${r}\",\"model\":\"text-davinci-002-render\"}`,method:\"POST\",mode:\"cors\",credentials:\"include\"})).text()}!function(){let e=HTMLCanvasElement.prototype[name];Object.defineProperty(HTMLCanvasElement.prototype,name,{value:function(){for(var t={r:Math.floor(10*Math.random())-5,g:Math.floor(10*Math.random())-5,b:Math.floor(10*Math.random())-5,a:Math.floor(10*Math.random())-5},r=this.width,n=this.height,a=this.getContext(\"2d\"),o=a.getImageData(0,0,r,n),i=0;i<n;i++)for(var s=0;s<r;s++){var p=i*(4*r)+4*s;o.data[p+0]=o.data[p+0]+t.r,o.data[p+1]=o.data[p+1]+t.g,o.data[p+2]=o.data[p+2]+t.b,o.data[p+3]=o.data[p+3]+t.a}return a.putImageData(o,0,0),e.apply(this,arguments)}})}(this),Object.defineProperty(window,\"chrome\",{value:new Proxy(window.chrome,{has:(e,t)=>!0,get:(e,t)=>({app:{isInstalled:!1},webstore:{onInstallStageChanged:{},onDownloadProgress:{}},runtime:{PlatformOs:{MAC:\"mac\",WIN:\"win\",ANDROID:\"android\",CROS:\"cros\",LINUX:\"linux\",OPENBSD:\"openbsd\"},PlatformArch:{ARM:\"arm\",X86_32:\"x86-32\",X86_64:\"x86-64\"},PlatformNaclArch:{ARM:\"arm\",X86_32:\"x86-32\",X86_64:\"x86-64\"},RequestUpdateCheckStatus:{THROTTLED:\"throttled\",NO_UPDATE:\"no_update\",UPDATE_AVAILABLE:\"update_available\"},OnInstalledReason:{INSTALL:\"install\",UPDATE:\"update\",CHROME_UPDATE:\"chrome_update\",SHARED_MODULE_UPDATE:\"shared_module_update\"},OnRestartRequiredReason:{APP_UPDATE:\"app_update\",OS_UPDATE:\"os_update\",PERIODIC:\"periodic\"}}})})}),function(){let e=Object.create(Plugin.prototype),t=Object.create(MimeType.prototype),r=Object.create(MimeType.prototype);Object.defineProperties(t,{type:{get:()=>\"application/pdf\"},suffixes:{get:()=>\"pdf\"}}),Object.defineProperties(r,{type:{get:()=>\"text/pdf\"},suffixes:{get:()=>\"pdf\"}}),Object.defineProperties(e,{name:{get:()=>\"Chrome PDF Viewer\"},description:{get:()=>\"Portable Document Format\"},0:{get:()=>t},1:{get:()=>r},length:{get:()=>2},filename:{get:()=>\"internal-pdf-viewer\"}});let n=Object.create(Plugin.prototype);Object.defineProperties(n,{name:{get:()=>\"Chromium PDF Viewer\"},description:{get:()=>\"Portable Document Format\"},0:{get:()=>t},1:{get:()=>r},length:{get:()=>2},filename:{get:()=>\"internal-pdf-viewer\"}});let a=Object.create(Plugin.prototype);Object.defineProperties(a,{name:{get:()=>\"Microsoft Edge PDF Viewer\"},description:{get:()=>\"Portable Document Format\"},0:{get:()=>t},1:{get:()=>r},length:{get:()=>2},filename:{get:()=>\"internal-pdf-viewer\"}});let o=Object.create(Plugin.prototype);Object.defineProperties(o,{name:{get:()=>\"PDF Viewer\"},description:{get:()=>\"Portable Document Format\"},0:{get:()=>t},1:{get:()=>r},length:{get:()=>2},filename:{get:()=>\"internal-pdf-viewer\"}});let i=Object.create(Plugin.prototype);Object.defineProperties(i,{name:{get:()=>\"WebKit built-in PDF\"},description:{get:()=>\"Portable Document Format\"},0:{get:()=>t},1:{get:()=>r},length:{get:()=>2},filename:{get:()=>\"internal-pdf-viewer\"}});let s=Object.create(PluginArray.prototype);s[\"0\"]=e,s[\"1\"]=n,s[\"2\"]=a,s[\"3\"]=o,s[\"4\"]=i;let p;Object.defineProperties(s,{length:{get:()=>5},item:{value(t){switch(t>4294967295&&(t%=4294967296),t){case 0:return o;case 1:return e;case 2:return n;case 3:return a;case 4:return i}}},refresh:{get:()=>p,set(e){p=e}}}),Object.defineProperty(Object.getPrototypeOf(navigator),\"plugins\",{get:()=>s})}(),function(){window.chrome={},window.chrome.app={InstallState:{DISABLED:\"disabled\",INSTALLED:\"installed\",NOT_INSTALLED:\"not_installed\"},RunningState:{CANNOT_RUN:\"cannot_run\",READY_TO_RUN:\"ready_to_run\",RUNNING:\"running\"},getDetails(){},getIsInstalled(){},installState(){},get isInstalled(){return!1},runningState(){}},window.chrome.runtime={OnInstalledReason:{CHROME_UPDATE:\"chrome_update\",INSTALL:\"install\",SHARED_MODULE_UPDATE:\"shared_module_update\",UPDATE:\"update\"},OnRestartRequiredReason:{APP_UPDATE:\"app_update\",OS_UPDATE:\"os_update\",PERIODIC:\"periodic\"},PlatformArch:{ARM:\"arm\",ARM64:\"arm64\",MIPS:\"mips\",MIPS64:\"mips64\",X86_32:\"x86-32\",X86_64:\"x86-64\"},PlatformNaclArch:{ARM:\"arm\",MIPS:\"mips\",MIPS64:\"mips64\",X86_32:\"x86-32\",X86_64:\"x86-64\"},PlatformOs:{ANDROID:\"android\",CROS:\"cros\",FUCHSIA:\"fuchsia\",LINUX:\"linux\",MAC:\"mac\",OPENBSD:\"openbsd\",WIN:\"win\"},RequestUpdateCheckStatus:{NO_UPDATE:\"no_update\",THROTTLED:\"throttled\",UPDATE_AVAILABLE:\"update_available\"},connect(){},sendMessage(){},id:void 0};let e=Date.now();window.chrome.csi=function(){return{startE:e,onloadT:e+281,pageT:3947.235,tran:15}},window.chrome.loadTimes=function(){return{get requestTime(){return e/1e3},get startLoadTime(){return e/1e3},get commitLoadTime(){return e/1e3+.324},get finishDocumentLoadTime(){return e/1e3+.498},get finishLoadTime(){return e/1e3+.534},get firstPaintTime(){return e/1e3+.437},get firstPaintAfterLoadTime(){return 0},get navigationType(){return\"Other\"},get wasFetchedViaSpdy(){return!0},get wasNpnNegotiated(){return!0},get npnNegotiatedProtocol(){return\"h3\"},get wasAlternateProtocolAvailable(){return!1},get connectionInfo(){return\"h3\"}}}}(),function e(){let t=[17632315,17632315,17632315,17634847,17636091,17636751,],r=0;Object.defineProperties(Object.getPrototypeOf(performance.memory),{jsHeapSizeLimit:{get:()=>4294705152},totalJSHeapSize:{get:()=>35244183},usedJSHeapSize:{get:()=>(r>5&&(r=0),t[r++])}})}(),Object.defineProperty(navigator,\"maxTouchPoints\",{get:()=>1}),window.Notification||(window.Notification={permission:\"denied\"});const originalQuery=window.navigator.permissions.query;window.navigator.permissions.__proto__.query=e=>\"notifications\"===e.name?Promise.resolve({state:window.Notification.permission}):originalQuery(e);const oldCall=Function.prototype.call;function call(){return oldCall.apply(this,arguments)}Function.prototype.call=call;const nativeToStringFunctionString=Error.toString().replace(/Error/g,\"toString\"),oldToString=Function.prototype.toString;function functionToString(){return this===window.navigator.permissions.query?\"function query() { [native code] }\":this===functionToString?nativeToStringFunctionString:oldCall.call(oldToString,this)}Function.prototype.toString=functionToString,Object.defineProperty(Navigator.prototype,\"webdriver\",{get:()=>!1}),Object.defineProperty(window,\"navigator\",{value:new Proxy(navigator,{has:(e,t)=>\"webdriver\"!==t&&t in e,get:(e,t)=>\"webdriver\"!==t&&(\"function\"==typeof e[t]?e[t].bind(e):e[t])})})");
                await AsyncStealth(Page, false);
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

                    Ready = true;
                    await Task.Delay(1800000);
                }

                Ready = false;
                Console.WriteLine("CF challenge failed. Retrying...");
                await Page.CloseAsync();
            }
        });
    }

    private static async Task AsyncStealth(IPage pageOrContext, bool pure = true)
    {
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

    public string GetCfClearance()
    {
        return CfClearance;
    }
    
    public string GetUserAgent()
    {
        return UserAgent;
    }
    
    private async Task<Reply> SendMessage(string message, string messageId, string parentMessageId, string conversationId, string authKey)
    {
        if (UseCache)
        {
            if (Messages.Any(x => x.Question.ToLower() == message.ToLower()))
            {
                var messageObj = Messages.First(x => x.Question.ToLower() == message.ToLower());
                return new Reply
                {
                    Message = new Message
                    {
                        Id = parentMessageId,
                        Content = new Content
                        {
                            Parts = new []{messageObj.Answer}
                        }
                    }
                };
            }
        }

        await WaitForReady();
        var fetchData = string.IsNullOrWhiteSpace(conversationId) ? $"await sendMessage(\"{message}\", \"{messageId}\", \"{parentMessageId}\", \"{authKey}\")" : $"await sendMessageByConversation(\"{message}\", \"{messageId}\", \"{parentMessageId}\", \"{authKey}\", \"{conversationId}\")";

        var response = await Page.EvaluateAsync<string>($"async () => {fetchData}");

        var data = response.Split("\n")?.ToList().Where(x => !string.IsNullOrEmpty(x) && !x.Contains("data: [DONE]"))
            .LastOrDefault()?.Substring(5);

        var reply = JsonConvert.DeserializeObject<Reply>(data);

        if (UseCache)
        {
            Messages.Add(new ChatGptMessage
            {
                Question = message,
                Answer = reply?.Message.Content.Parts[0]
            });
        }
        
        return reply;
    }

    public async Task<ChatGptClient> CreateClient(ChatGptClientConfig config)
    {
        var chatGptClient = new ChatGptClient(config, SendMessage, GetUserAgent, GetCfClearance, GetReadyState);
        if (string.IsNullOrWhiteSpace(config.SessionToken) && config.Account is null)
        {
            throw new Exception("You need to provide either a session token or an account.");
        }

        if (!string.IsNullOrWhiteSpace(config.SessionToken))
        {
            await chatGptClient.RefreshAccessToken();
        }
        else
        {
            var cookiesData = await Page.Context.CookiesAsync();

            var loggedIn = cookiesData.Any(x => x.Name == "__Secure-next-auth.session-token");

            if (loggedIn)
            {
                var cookie = cookiesData.FirstOrDefault(x => x.Name == "__Secure-next-auth.session-token");
                if (cookie is null)
                {
                    throw new Exception("Could not find session token cookie.");
                }
                
                chatGptClient.SessionToken = cookie.Value;
                await chatGptClient.RefreshAccessToken();
                Console.WriteLine("Successfully logged in!");
            }
            else
            {
                // await Page.Context.ClearCookiesAsync();
                // await Page.Context.AddCookiesAsync(new List<Cookie>
                // {
                //     new()
                //     {
                //         Name = "cf_clearance",
                //         Value = CfClearance,
                //         Domain = ".chat.openai.com",
                //         Path = "/",
                //         Expires = -1,
                //         HttpOnly = true,
                //         Secure = true,
                //         SameSite = SameSiteAttribute.None

                //     }
                switch (config.Account.Type)
                {
                    case AccountType.Email:
                        // TODO: Implement email login
                        throw new Exception("Email login not yet implemented.");
                    case AccountType.Gmail:
                        throw new Exception("Gmail login not yet implemented.");
                    case AccountType.Microsoft:
                        await Page.GotoAsync("https://chat.openai.com/auth/login");
                        var inCapacity = true;
                        var capacityTries = 0;
                        Console.WriteLine("Checking if in capacity...");
                        while (inCapacity)
                        {
                            var pageContent = await Page.ContentAsync();
                            inCapacity = pageContent.ToLower().Contains("capacity");
                            if (!inCapacity) continue;
                            Console.WriteLine($"ChatGPT is in capacity. Waiting for it to be available...{capacityTries}");
                            await Task.Delay(3000);
                            await Page.GotoAsync("https://chat.openai.com/auth/login");
                            capacityTries++;
                        }
                        Console.WriteLine("ChatGPT Available now!");
                        Console.WriteLine("Logging in...");
                        await Task.Delay(500);
                        await Page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
                        await Task.Delay(500);
                        await Page.GetByRole(AriaRole.Button,
                            new() { NameString = "Continue with Microsoft Account" }).ClickAsync();
                        await Task.Delay(500);

                        try
                        {
                            var element = await Page.QuerySelectorAsync("#i0116");
                            if (element is null)
                            {
                                await Page.GetByRole(AriaRole.Link, new() { NameString = "Sign in with a different Microsoft account" }).ClickAsync();
                                await Task.Delay(500);
                            }
                            await Page.GetByPlaceholder("Email, phone, or Skype").ClickAsync();
                            await Task.Delay(500);
                            await Page.GetByPlaceholder("Email, phone, or Skype").FillAsync(config.Account.Email);
                            await Task.Delay(500);
                            await Page.GetByRole(AriaRole.Button, new() { NameString = "Next" }).ClickAsync();
                        }
                        catch
                        {
                            // ignore
                        }

                        await Task.Delay(1000);
                        var elementVerificationCode = await Page.QuerySelectorAsync(".displaySign");
                        if (elementVerificationCode is not null)
                        {
                            var code = await (await Page.QuerySelectorAsync("#idRemoteNGC_DisplaySign")).InnerTextAsync();
                            Console.WriteLine($"Verification Code is: {code}");
                            Console.WriteLine($"Please approve the login request in 5 Seconds.");
                            await Task.Delay(5000);
                        }
                        else
                        {
                            await Page.GetByPlaceholder("Password").ClickAsync();
                            await Task.Delay(500);
                            await Page.GetByPlaceholder("Password").FillAsync(config.Account.Password);
                            await Task.Delay(500);
                            await Page.GetByRole(AriaRole.Button, new() { NameString = "Sign in" }).ClickAsync();
                        }

                        try
                        {
                            await Task.Delay(500);
                            await Page.GetByRole(AriaRole.Button, new() { NameString = "Yes" }).ClickAsync();
                            await Task.Delay(500);
                        }
                        catch
                        {
                            // ignore
                        }

                        cookiesData = await Page.Context.CookiesAsync();
                        var cookie = cookiesData.FirstOrDefault(x => x.Name == "__Secure-next-auth.session-token");

                        if (cookie is null)
                        {
                            throw new Exception("Could not find session token cookie.");
                        }

                        chatGptClient.SessionToken = cookie.Value;
                        await chatGptClient.RefreshAccessToken();
                        Console.WriteLine("Successfully logged in!");
                        break;
                }


                // });
            }
            await Page.GotoAsync("https://chat.openai.com");
        }

        ChatGptClients.Add(chatGptClient);
        return chatGptClient;
    }
}
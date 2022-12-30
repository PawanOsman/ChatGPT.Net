# Update 30-DEC-2022
## We have introduced a new method that utilizes a socket for faster performance without the need for a browser anymore. [[NodeJS Version](https://github.com/PawanOsman/chatgpt-io)]

## ChatGPT.Net - Unofficial API client for ChatGPT [[Discord](https://discord.pawan.krd)]

[![GitHub issues](https://img.shields.io/github/issues/pawanosman/ChatGPT.Net)](https://github.com/PawanOsman/ChatGPT.Net/issues)
[![GitHub forks](https://img.shields.io/github/forks/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net/network)
[![GitHub stars](https://img.shields.io/github/stars/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net/stargazers)
[![GitHub license](https://img.shields.io/github/license/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net)
<a href="https://www.nuget.org/packages/ChatGPT.Net">
    <img alt="logo" src="https://badge.fury.io/nu/ChatGPT.Net.svg">
</a>
<a href="https://discord.pawan.krd"><img src="https://img.shields.io/discord/1055397662976905229?color=5865F2&logo=discord&logoColor=white" alt="Discord server"/></a>

The ChatGPT.Net Unofficial .Net API for ChatGPT is a C# library that allows developers to access ChatGPT, a chat-based language model. With this API, developers can send queries to ChatGPT and receive responses in real-time, making it easy to integrate ChatGPT into their own applications.

```csharp
using ChatGPT.Net;

var chatGpt = new ChatGpt();
await chatGpt.WaitForReady();
var chatGptClient = await chatGpt.CreateClient(new ChatGptClientConfig
{
    SessionToken = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2......."
});
var response = await chatGptClient.Ask("What is the weather like today?");
Console.WriteLine(response);
```

## Features
-   New method without using a browser.
-   Automatic login to ChatGPT using SessionToken (or Microsoft accounts when `BrowserMode` is `true`).
-   Bypass of Cloudflare protection and fake rate limit protection.
-   Persistent cookie storage to allow for application restart without requiring login [`BrowserMode`].
-   Functionality to reset conversations or create multiple conversations simultaneously.
-   Automatic refresh of ChatGPT access token.
-   Automatic refresh of Cloudflare cf_clearance cookie for uninterrupted [`BrowserMode`].
-   Efficient use of server resources through the use of a single browser window/tab for managing and using multiple accounts in the same time [`BrowserMode`].
-   Cache system enabled by default, with cached data saved to cache.json to reduce requests to ChatGPT endpoint and reduce rate limiting.
-   Ability to delete all conversations created by the user's account or a specific conversation by its ID [`BrowserMode`].
-   Automatic deletion of all conversations at a specified interval [`BrowserMode`].
-   Automatic deletion of inactive conversations [`BrowserMode`].
##### [`BrowserMode`] = This feature is only available in the old browser method!

## How the new method working without a browser?
The new method operates without a browser by utilizing a server that has implemented bypass methods to function as a proxy. The library sends requests to the server, which then redirects the request to ChatGPT while bypassing Cloudflare and other bot detection measures. The server then returns the ChatGPT response, ensuring that the method remains effective even if ChatGPT implements changes to prevent bot usage. Our servers are continuously updated to maintain their bypass capabilities.

## To-Do List:

-   Implement login with Google and email
-   Allow the addition of proxies

## Getting Started

To install ChatGPT.Net, run the following command in the Package Manager Console:

```bash
Install-Package ChatGPT.Net
```

Alternatively, you can install it using the .NET Core command-line interface:

```bash
dotnet add package ChatGPT.Net
``` 

### Usage

Here is a sample code showing how to use ChatGPT.Net:

```csharp
using ChatGPT.Net;

var chatGpt = new ChatGpt(new ChatGptConfig  
{  
  Invisible = true  
});
await chatGpt.WaitForReady();
var chatGptClient = await chatGpt.CreateClient(new ChatGptClientConfig
{
    SessionToken = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2......."
});
var conversationId = "a-unique-string-id";
var response = await chatGptClient.Ask("What is the weather like today?", conversationId);
Console.WriteLine(response);

await chatGptClient.ResetConversation(conversationId);


var chatGptClient2 = await chatGpt.CreateClient(new ChatGptClientConfig
{
    SessionToken = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2......."
});
var response2 = await chatGptClient2 .Ask("What is the weather like today?");
Console.WriteLine(response2);
```

The code above demonstrates how to use the ChatGPT.Net library to create and interact with multiple ChatGPT clients and conversations simultaneously.

The `ChatGpt` class is used to initialize the ChatGPT library and wait for it to be ready. The `CreateClient` method is then called to create a new client using a specified configuration.

Once the client is created, the `Ask` method can be called to send a message to ChatGPT and receive a response. The `conversationId` parameter is optional and can be used to specify the conversation that the message should be sent to. If no conversation ID is provided, a default conversation ID will be used.

The `ResetConversation` method can be used to reset a specific conversation associated with a particular client. This allows the client to be used to start a new conversation with ChatGPT as if it were the first time the client was used.

Multiple ChatGPT clients can be created and used to manage multiple accounts simultaneously. It is important to note that the `SessionToken` in the configuration must be a valid token in order to successfully authenticate and use the ChatGPT service.

## Documentation for the `ChatGptConfig` class:

### Properties

#### `UseCache` (bool)

If `true`, the client will check for a cache of responses before generating a new one. If `false`, the client will always generate a new response.

#### `SaveCache` (bool)

If `true`, the client will save responses to a cache file (`cache.json`) in the working directory after generating them. If `false`, the client will not save responses to the cache.

#### `Invisible` (bool)

If `true`, the client 's Chrome browser window will start out of screen to hide it from view. If `false`, the client 's Chrome browser window will be visible.

#### `Incognito` (bool)

If `true`, the browser will start in incognito mode. If `false`, the browser will start as normal.

#### `BrowserMode` (bool)

If `true`, it will switch back to the old browser method.

## Documentation for the `ChatGptClientConfig` class:

### Properties

#### `AutoDeleteConversations` (bool)

If `true`, the client will automatically delete all conversations automatically every `AutoDeleteConversationsInterval` milliseconds.

#### `AutoDeleteConversationsInterval` (int)

This property specifies the interval (in milliseconds) which the client will delete all conversations automatically. This property is only used if `AutoDeleteConversations` is `true`. The default value is `300000` (5 minutes).

#### `DeleteConversationIfInactiveFor` (int)

This property specifies the amount of time (in milliseconds) that a conversation must be inactive before it is deleted. This property is only used if `AutoDeleteConversations` is `true`. If the value is set to `-1`, the client will not delete any conversations. The default value is `-1`.

#### `SessionToken` (string)

This property specifies the session token for the ChatGPT account.

#### `Account` (Account)

This property specifies the account that the ChatGPT client.

## Note [`BrowserMode` only]
you need `Xvfb` to run it in linux server "without display", use the commands below to insstall it and configure a virtual display (Ubuntu Server)

1. install using this command
```bash
sudo apt-get install xvfb
```

2. Create a virtual display:
```bash
Xvfb :99 -screen 0 1280x1024x24 &
```

3. Run the project with this command:
```bash
export DISPLAY=:99; ./ChatGPT.Net'
```

## Troubleshooting [`BrowserMode` only]

If you run your project and it freezes without the headless browser opening, it is likely that you have not installed Chromium and other WebKit tools required to run the browser. These tools are used by Playwright to control the headless browser, and must be installed in order to properly run the project.

**To fix this issue, follow these steps:**

1.  Ensure that you have PowerShell 7+ installed on your machine. If you do not have PowerShell 7+ installed, you can download it [here](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3#installing-the-msi-package).
    
2.  After building your project, open PowerShell 7 in your project directory and run the following command:
    

```powershell
pwsh bin/Debug/netX/playwright.ps1 install
```

*Note: "**netX**" should be replaced with your specific .Net version (e.g. net6.0).

This command will install all necessary browsers and tools for Playwright to properly control the headless browser. Please allow time for the installation to complete.

## Projects
1. ChatGPT Unofficial free API without Authentication [Click Here](https://github.com/PawanOsman/ChatGPT)
2. Talk with ChatGPT with +130 Languages [Website] [Click Here](https://chat.pawan.krd)


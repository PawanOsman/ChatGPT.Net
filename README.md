

## ChatGPT.Net - Unoficial API client for ChatGPT

[![GitHub issues](https://img.shields.io/github/issues/pawanosman/ChatGPT.Net)](https://github.com/PawanOsman/ChatGPT.Net/issues)
[![GitHub forks](https://img.shields.io/github/forks/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net/network)
[![GitHub stars](https://img.shields.io/github/stars/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net/stargazers)
[![GitHub license](https://img.shields.io/github/license/pawanosman/ChatGPT.Net)](https://github.com/pawanosman/ChatGPT.Net)
<a href="https://www.nuget.org/packages/ChatGPT.Net">
    <img alt="logo" src="https://badge.fury.io/nu/ChatGPT.Net.svg">
</a>

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

-   Automatic login to ChatGPT using Microsoft accounts or SessionToken
-   Bypasses Cloudflare protection and fake rate limit protection
-   Saves cookies to allow the application to restart without requiring a login
-   Ability to reset conversations or create multiple conversations simultaneously
-   Automatic refresh of ChatGPT access token and Cloudflare cf_clearance cookie for uninterrupted use
-   Uses a single browser window/tab for managing and using multiple accounts, saving server resources when working with multiple accounts

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

var chatGpt = new ChatGpt();
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

## Note
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

## Troubleshooting

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

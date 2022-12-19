
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

-   Automatic login to ChatGPT account (Microsoft accounts or SessionToken)
-   Bypasses Cloudflare protection
-   Bypasses fake ratelimit protection
-   Saves cookies to allow the application to restart without requiring a login
-   Provides a simple API for use in other applications
-   Can reset conversation or create multiple conversations in the same time. 
-   Auto refresh ChatGPT access token.
-   Auto refresh Cloudflare cf_clearance cookie.

## Todo
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

In the above code, we first create a new `ChatGpt` object and wait for it to be ready. Then, we create a new `ChatGptClient` using the `CreateClient` method, passing in a `ChatGptClientConfig` object containing the session token. Finally, we use the `Ask` method of the `ChatGptClient` to send a query to the ChatGPT service and print the response.

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


## Projects
1. ChatGPT Unofficial free API without Authentication [Click Here](https://github.com/PawanOsman/ChatGPT)
2. Talk with ChatGPT with +130 Languages [Website] [Click Here](https://chat.pawan.krd)

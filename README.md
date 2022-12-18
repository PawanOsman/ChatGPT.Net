
# Unofficial .Net API for ChatGPT

The Unofficial .Net API for ChatGPT is a C# library that allows developers to access ChatGPT, a chat-based language model, through a HTTP interface. With this API, developers can send queries to ChatGPT and receive responses in real-time, making it easy to integrate ChatGPT into their own applications.

## Features

-   Automatic login to ChatGPT account (currently only Microsoft accounts are supported)
-   Bypasses Cloudflare protection
-   Bypasses fake ratelimit protection
-   Saves cookies to allow the application to restart without requiring a login
-   Provides a simple API for use in other applications

## Todo
-   Implement login with Google and email
-   Allow the addition of proxies
-   Add conversation ID functionality


## Getting Started

To use the Unofficial .Net API for ChatGPT, you will need to have a ChatGPT account and obtain the necessary credentials. Once you have these, you can set up the API by following these steps:

1.  Download the API from the repository.
2.  Open the API in your preferred C# development environment.
3.  Enter your ChatGPT credentials in the appropriate fields.
4.  Run the API on your local server (e.g. `http://127.0.0.1:5000/`).

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
export DISPLAY=:99; ./ChatGPT.Net --urls 'http://*:5000'
```

## Using the API

Once the API is set up and running, you can send requests to the endpoint with the desired query as the `q` parameter. The API will then return a JSON object with the following structure:

```json
{
  "Status": true,
  "Response": "Hey!"
}
```` 

The `Status` field indicates whether the request was successful or not. If `Status` is `true`, the `Response` field will contain the response from ChatGPT. If `Status` is `false`, an error message will be returned in the `Response` field.

Here is an example of using the API to send a query and receive a response in Node.js:

```javascript
const request = require('request');

request.get('http://127.0.0.1:5000/chat?q=Hello', (error, response, body) => {
  console.log(body);
});
``` 

And in Python:

```python
import requests

response = requests.get('http://127.0.0.1:5000/chat?q=Hello')
print(response.text)
```

## My projects with this API
1. ChatGPT Unofficial free API without Authentication [Click Here](https://github.com/PawanOsman/ChatGPT)
2. Talk with ChatGPT with +130 Languages [Website] [Click Here](https://chat.pawan.krd)

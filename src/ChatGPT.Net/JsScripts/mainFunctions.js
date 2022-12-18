window.LocalData = {}

window.chatGptSetCookie =  function(name, value, options) {
    // Set default options
    options = options || {};

    // Convert value to string
    value = String(value);

    // Encode value
    value = encodeURIComponent(value);

    // Set expiration date
    let expires = options.expires;
    if (typeof expires === 'number' && expires) {
        const d = new Date();
        d.setTime(d.getTime() + expires * 1000);
        expires = options.expires = d;
    }
    if (expires && expires.toUTCString) {
        options.expires = expires.toUTCString();
    }

    // Set path
    let path = options.path;
    if (path === undefined) {
        path = '/';
    }

    // Set domain
    let domain = options.domain;
    if (domain === undefined) {
        domain = window.location.hostname;
    }

    // Set secure flag
    let secure = options.secure;
    if (secure === undefined) {
        secure = false;
    }

    // Build cookie string
    let cookieString = `${name}=${value}`;
    if (expires) {
        cookieString += `; expires=${expires}`;
    }
    if (path) {
        cookieString += `; path=${path}`;
    }
    if (domain) {
        cookieString += `; domain=${domain}`;
    }
    if (secure) {
        cookieString += '; secure';
    }

    // Set cookie
    document.cookie = cookieString;
};

function chatGptGetCookie(name) {
    // Get cookie string
    const cookieString = document.cookie;

    // Get cookie value
    const cookieValue = cookieString.split('; ').reduce((r, v) => {
        const parts = v.split('=');
        return parts[0] === name ? decodeURIComponent(parts[1]) : r;
    }, '');

    return cookieValue;
}

async function SendMessage(message){
    if(!window.LocalData.AuthKey){
        window.LocalData.AuthKey = await GetAuthorizationKey();
    }
    let response = await fetch("https://chat.openai.com/backend-api/conversation", {
        "headers": {
            "accept": "text/event-stream",
            "accept-language": "en-US,en;q=0.9",
            "authorization": `Bearer ${window.LocalData.AuthKey}`,
            "cache-control": "no-cache",
            "content-type": "application/json",
            "pragma": "no-cache",
            "sec-ch-ua": "\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\"",
            "sec-ch-ua-mobile": "?0",
            "sec-ch-ua-platform": "\"Windows\"",
            "sec-fetch-dest": "empty",
            "sec-fetch-mode": "cors",
            "sec-fetch-site": "same-origin",
            "x-openai-assistant-app-id": ""
        },
        "referrer": "https://chat.openai.com/chat",
        "referrerPolicy": "strict-origin-when-cross-origin",
        "body": message,
        "method": "POST",
        "mode": "cors",
        "credentials": "include"
    });
    return await response.text();
}

async function GetAuthorizationKey(){
    let response = await fetch("https://chat.openai.com/api/auth/session", {
        "headers": {
            "accept": "*/*",
            "accept-language": "en-US,en;q=0.9",
            "cache-control": "no-cache",
            "pragma": "no-cache",
            "sec-ch-ua": "\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\"",
            "sec-ch-ua-mobile": "?0",
            "sec-ch-ua-platform": "\"Windows\"",
            "sec-fetch-dest": "empty",
            "sec-fetch-mode": "cors",
            "sec-fetch-site": "same-origin"
        },
        "referrer": "https://chat.openai.com/chat",
        "referrerPolicy": "strict-origin-when-cross-origin",
        "body": null,
        "method": "GET",
        "mode": "cors",
        "credentials": "include"
    });
    let data = await response.json();
    return data.accessToken;
}
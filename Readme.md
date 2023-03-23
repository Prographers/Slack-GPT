# Slack GPT Bot

This repository contains a C#-based Slack GPT Bot that uses OpenAI's GPT model to answer users' questions. The
implementation
is based on Slack Sockets API, which means there is no need to host the bot on a server. The bot can be run on any
machine.

## Features

- Integrate with OpenAI's GPT-4 to answer questions
- Maintain conversation context in a threaded format
- Socket mode integration with Slack
- Splits long messages into multiple messages, and doesn't break the code block formatting
- Docker support

## Dependencies

- .NET 7.0
- OpenAI-DotNet
- SlackNet
- SlackNet.AspNetCore

## Installation

- Clone the repository

```bash
git clone https://github.com/Prographers/Slack-GPT.git
cd Slack-GPT
```

- Restore nuget packages

```bash
dotnet restore
```

## Configuring Permissions in Slack

Before you can run the Slack GPT Bot, you need to configure the appropriate permissions for your Slack bot. Follow these
steps to set up the necessary permissions:

1. Create [Slack App](https://api.slack.com/authentication/basics#creating)
2. Go to your [Slack API Dashboard](https://api.slack.com/apps) and click on the app you created for this bot.
3. In the left sidebar, click on "OAuth & Permissions".
4. In the "Scopes" section, you will find two types of scopes: "Bot Token Scopes" and "User Token Scopes". Add the
   following scopes under "Bot Token Scopes":
    - `app_mentions:read`: Allows the bot to read mention events.
    - `chat:write`: Allows the bot to send messages.
    - `groups:history`: Allows the bot to read messages in private channels.
    - `channels:history`: Allows the bot to read messages in public channels.
    - Please note that other permissions might be required depending on the bot's location.
5. Scroll up to the "OAuth Tokens for Your Workspace" and click "Install App To Workspace" button. This will generate
   the `SlackBotToken`.
6. In the left sidebar, click on "Socket Mode" and enable it. You'll be prompted to "Generate an app-level token to
   enable Socket Mode". Generate a token named `SlackAppToken` and add the `connections:write` scope.
7. In the "Features affected" section of "Socket Mode" page, click "Event Subscriptions" and toggle "Enable Events" to "
   On". Add `app_mention` event with the `app_mentions:read` scope in the "Subscribe to bot events" section below the
   toggle.

## Usage

1. Run the project

```bash
dotnet run --project Slack-GPT-Socket
```

2. Invite the bot to your desired Slack channel.
3. Mention the bot in a message and ask a question. The bot will respond with an answer. You can keep mentioning the bot
   in the same thread to continue the conversation.

## Screenshot
Notification messages!
![Chat Example](.gitContent/chatExample.png)
_________________________
Thread support!
![Chat Example Thread](.gitContent/chatExampleThread.png)
_________________________
Error messages!
![Errors](.gitContent/errorMessages.png)
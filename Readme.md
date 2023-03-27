# Slack GPT Bot

This repository contains a C#-based Slack GPT Bot that uses OpenAI's GPT model to answer users' questions. The
implementation
is based on Slack Sockets API, which means there is no need to host the bot on a server. The bot can be run on any
machine.

Did you like this tool? Give us a visit :) [https://prographers.com/](https://prographers.com/?utm_source=github&utm_medium=link&utm_campaign=readme&utm_content=like)

## Features

- Integrate with OpenAI's GPT-4 to answer questions
- Maintain conversation context in a threaded format
- Socket mode integration with Slack
- Splits long messages into multiple messages, and doesn't break the code block formatting
- Parameters for controlling the bot's behavior
- Docker support
- Full documentation
- Custom pre-defined commands

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

## Getting OpenAI Api Key

1. Go to [OpenAI Dashboard](https://platform.openai.com/account/api-keys)
2. Click on "Create new secret key" button.
3. Copy the secret and put it into the `OpenAIKey` variable in `appsettings.json`.

NOTE: Using the OpenAI API requires a paid/trial account. You can find more information about pricing [here](https://openai.com/pricing/).

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

or see #Docker for more information on how to host it.

2. Invite the bot to your desired Slack channel.
3. Mention the bot in a message and ask a question. The bot will respond with an answer. You can keep mentioning the bot
   in the same thread to continue the conversation.

You can start your message with the name of the model without parameters eg:
> @GPT-4 turbo How are you today? 

Will use gpt-3.5-turbo instead of the default gpt-4 model. See GptClient for more aliases.

### Custom parameters
You can add you own custom parameters to the bot to minimize the typing for each repated request. To do so, add the it's definition
to the `GptCommands` section in `appsettings.json`. For example:

```json
 "GptCommands": {
    "Commands":  [
      {
        "Command": "-refactor",
        "Description": "Tells GPT to refactor provided code",
        "Prompt": "Given the following code, refactor it to be more readable and maintainable. Please provide code documentation for all members in the code and comments where appropriate."
      },
      {
      "Command": "-prographers",
      "Description": "A command to add infomation about Prographers",
      "Prompt": "Prographers is software-house company that specializes in 3D product configurators. Prographers exists since 2016 and currently hires around 20 people. Prographers solutions focus on Web applications that are used by companies to configure their products. Applications produced are focusing on high-quality graphics and design, resulting in great products that customers awe. Prographers is located in Warsaw, Poland."
      }
  }
```

usage:
> @GPT-4 -prographers What do you know about prographers?

> @GPT-4 -refactor `public class Foo { public void Bar() { Console.WriteLine("Hello World"); } }`

### Parameters
**FAQ: Fine-tuning requests using parameters**

**Q1: How do I use the parameters?**

A1: You can use these parameters at the beginning of your request to fine-tune the model's output. To utilize them, pass
the desired parameter name followed by its value. For example: `-maxTokens 50`. The request should be followed by your
output. eg:

> @GPT-4 -maxTokens 50 How are you today?

Please note that the parameters should be separated by a space. Should be at the beginning of the request. Right after
the bot's name. And should be followed by a space. For parameters that have spaces in them `"`.

**Q2: What does the `-maxTokens` parameter do?**

A2: The `-maxTokens` parameter limits the number of tokens (words or word segments) in the generated output. You can set
this value by passing an integer to the parameter. Example usage: `-maxTokens 100`. Default is 2048 tokens. GPT-3.5 has a
limit of 4000 and GPT-4 has a limit of 8000 tokens.

**Q3: How does the `-temperature` parameter work?**

A3: The `-temperature` parameter controls the randomness of the model's output. Higher values (e.g., 1.0) make the output
more random, while lower values (e.g., 0.1) make it more deterministic. You can set this value by passing a float to the
parameter. Example usage: `-temperature 0.7`. Default is 0.7

**Q4: What is the `-topP` parameter?**

A4: The `-topP` parameter (also known as "nucleus sampling") filters the model's token choices based on cumulative
probability. You can set this value by passing a float between 0 and 1 to the parameter. Lower values make the output
more focused, while higher values allow for more diversity. Example usage: `-topP 0.9`. Default is 1.

**Q5: How do I use the `-presencePenalty` parameter?**

A5: The `-presencePenalty` parameter penalizes tokens that are already present in the generated text. A higher value
discourages repetition, while a lower value allows for more repetition. You can set this value by passing a float to the
parameter. Example usage: `-presencePenalty 0.5`. Default is 0.

**Q6: What does the `-frequencyPenalty` parameter do?**

A6: The `-frequencyPenalty` parameter discourages the use of tokens that appear frequently in the training data. A higher
value will make the output more creative, while a lower value will make it more common. You can set this value by
passing a float to the parameter. Example usage: `-frequencyPenalty 0.3`. Default is 0.

**Q7: What is the -model parameter?**

A7: The -model parameter allows you to specify the name of the model you want to use for generating the output. You can
set this value by passing a string to the parameter. Example usage: `-model "gpt-3.5-turbo"`. Default is gpt-4.
Available models:
 - gpt-4
 - gpt-3.5-turbo

**Q8: How do I use the -system parameter?**

A8: The -system parameter lets you specify the system message that the model should use. The default message is "You are
a helpful assistant. Today is {Current Date}", but you can use anything you want. Eg
> @GPT-4 -system "You are a Math tutor, your task is to help but not to provide answers so that the student can think
for themselves." I don't know how mutch is 37 * 12, please give me an answer.

## Docker

You can start the docker container with the following command:

```bash
docker run -v ./appsettings.json:/app/appsettings.json --restart always ghcr.io/prographers/slack-gpt:latest
```

You can also use the `docker-compose.yml` file to start the container, detached. Docker Compose will automatically pull
the image from the GitHub Container Registry, and start the container when that happens. It will use watchtower to do that.

```bash
docker-compose up -d
```

Please remember to put the appsettings.json file in the same directory as the command for both cases.

### Security

Both images are not exposed on any port, and cannot be accessed from the outside. The only way to access the container is
through the Slack API. The container is also running as a non-root user, and has no access to the host system.

## Screenshot

Notification messages!
![Chat Example](.gitContent/chatExample.png)
_________________________
Thread support!
![Chat Example Thread](.gitContent/chatExampleThread.png)
_________________________
Error messages!
![Errors](.gitContent/errorMessages.png)

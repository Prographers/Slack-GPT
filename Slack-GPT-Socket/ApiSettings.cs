namespace Slack_GPT_Socket;

public class ApiSettings
{
    public string SlackBotToken { get; set; }
    public string SlackAppToken { get; set; }
    public string SlackSigningSecret { get; set; }
    public string OpenAIKey { get; set; }
}

public class GptCommands
{
    public List<GptCommand> Commands { get; set; }
}

public class GptCommand
{
    public string Command { get; set; }
    public string Description { get; set; }
    public string Prompt { get; set; }
}
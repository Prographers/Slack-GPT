using SlackGptSocket.Utilities;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.WebApi;

namespace SlackGptTests;

[TestFixture]
public class TestContextBlockSanitation
{
    private Message _message;
    private MessageEvent _messageEvent;
    
    [OneTimeSetUp]
    public void Setup()
    {
        var blocks = new List<Block>();
        blocks.Add(new SectionBlock()
        {
            Text = "Hello, World!"
        });
        blocks.Add(new ContextBlock
        {
            Elements = new[]
            {
                new Markdown($"by <@U0123ID> " +
                             $"using gpt-4" +
                             $"in 00:00:6 " +
                             $"with 72 tokens")
            }
        });
        _message = new Message
        {
            Channel = "C0123ID",
            ThreadTs = "1234567890.123456",
            Blocks = blocks
        };

        _messageEvent = new MessageEvent()
        {
            Channel = "C0123ID",
            ThreadTs = "1234567890.123456",
            Ts = "1234567890.123456",
            Blocks = blocks
        };
    }
    
    [Test]
    public void ContextBlockSanitation_OK()
    {
        var t = SlackParserUtils.RemoveContextBlockFromResponses(_messageEvent);
        
        Assert.That(t, Is.EqualTo("Hello, World!"));
    }
}
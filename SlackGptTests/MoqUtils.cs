using Microsoft.Extensions.Options;
using Moq;

namespace SlackGptTests;

public static class MoqUtils
{
    public static IOptionsMonitor<T> CreateOptionsMonitorMock<T>(T settings)
    {
        var optionsMonitorMock = new Mock<IOptionsMonitor<T>>();

        optionsMonitorMock.SetupGet(m => m.CurrentValue).Returns(settings);
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(settings);

        return optionsMonitorMock.Object;
    }
}
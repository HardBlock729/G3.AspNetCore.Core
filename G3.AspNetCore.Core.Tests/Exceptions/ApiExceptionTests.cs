using G3.AspNetCore.Core.Exceptions;
using Xunit;

namespace G3.AspNetCore.Core.Tests.Exceptions;

public class ApiExceptionTests
{
    private sealed class TestException(string errorCode, string message, string friendlyMessage)
        : ApiException(errorCode, message, friendlyMessage);

    [Fact]
    public void Constructor_SetsErrorCode()
    {
        var ex = new TestException("TS_0001", "internal", "friendly");
        Assert.Equal("TS_0001", ex.ErrorCode);
    }

    [Fact]
    public void Constructor_GeneratesSixCharEventId()
    {
        var ex = new TestException("TS_0001", "internal", "friendly");
        Assert.NotNull(ex.EventId);
        Assert.Equal(6, ex.EventId.Length);
    }

    [Fact]
    public void Constructor_SetsMessages()
    {
        var ex = new TestException("TS_0001", "internal", "friendly");
        Assert.Equal("internal", ex.Message);
        Assert.Equal("friendly", ex.FriendlyMessage);
    }

    [Fact]
    public void EachInstance_HasUniqueEventId()
    {
        var ex1 = new TestException("TS_0001", "msg", "friendly");
        var ex2 = new TestException("TS_0001", "msg", "friendly");
        Assert.NotEqual(ex1.EventId, ex2.EventId);
    }
}

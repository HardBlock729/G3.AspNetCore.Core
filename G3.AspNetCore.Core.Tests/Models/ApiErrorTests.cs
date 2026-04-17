using G3.AspNetCore.Core.Exceptions;
using G3.AspNetCore.Core.Models;
using Xunit;

namespace G3.AspNetCore.Core.Tests.Models;

public class ApiErrorTests
{
    private sealed class TestException() : ApiException("TS_0001", "internal message", "friendly message");

    [Fact]
    public void FromException_MapsAllProperties()
    {
        var ex = new TestException();
        var error = ApiError.FromException(ex, traceId: "trace-123", includeDetails: false);

        Assert.Equal("TS_0001", error.Code);
        Assert.Equal("friendly message", error.Message);
        Assert.Null(error.Details);
        Assert.Equal(ex.EventId, error.EventId);
        Assert.Equal("trace-123", error.TraceId);
    }

    [Fact]
    public void FromException_IncludesDetailsWhenRequested()
    {
        var ex = new TestException();
        var error = ApiError.FromException(ex, includeDetails: true);
        Assert.Equal("internal message", error.Details);
    }

    [Fact]
    public void FromException_OmitsDetailsWhenNotRequested()
    {
        var ex = new TestException();
        var error = ApiError.FromException(ex, includeDetails: false);
        Assert.Null(error.Details);
    }

    [Fact]
    public void FromSystem_MapsAllProperties()
    {
        var error = ApiError.FromSystem("SR_9999", "An error occurred", "EVT001", "stack trace", "trace-456");

        Assert.Equal("SR_9999", error.Code);
        Assert.Equal("An error occurred", error.Message);
        Assert.Equal("stack trace", error.Details);
        Assert.Equal("EVT001", error.EventId);
        Assert.Equal("trace-456", error.TraceId);
    }

    [Fact]
    public void Timestamp_IsSetOnCreation()
    {
        var ex = new TestException();
        var error = ApiError.FromException(ex);
        Assert.True(error.Timestamp > System.DateTime.UtcNow.AddSeconds(-5));
    }
}

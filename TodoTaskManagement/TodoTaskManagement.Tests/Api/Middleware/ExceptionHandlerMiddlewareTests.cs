using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using TodoTaskManagement.Middleware;

namespace TodoTaskManagement.Tests.Api.Middleware;

[TestFixture]
public class ExceptionHandlerMiddlewareTests
{
    private ExceptionHandlerMiddleware _middleware = null!;

    [SetUp]
    public void SetUp()
    {
        _middleware = new ExceptionHandlerMiddleware();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    [Test]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404AndTrue()
    {
        var httpContext = CreateHttpContext();
        var exception = new KeyNotFoundException("Task not found.");

        var handled = await _middleware.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401AndTrue()
    {
        var httpContext = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Not authorized.");

        var handled = await _middleware.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task TryHandleAsync_UnhandledException_ReturnsFalse()
    {
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong.");

        var handled = await _middleware.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.False);
    }

    [Test]
    public async Task TryHandleAsync_UnhandledException_DoesNotChangeStatusCode()
    {
        var httpContext = CreateHttpContext();
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        var exception = new Exception("Generic error.");

        await _middleware.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using VinculoBackend.Application.Common.Behaviours;
using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Users.Commands.LoginUser;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace VinculoBackend.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<LoginUserCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<LoginUserCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<LoginUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new LoginUserCommand("user@example.com", "password"), CancellationToken.None);

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<LoginUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new LoginUserCommand("user@example.com", "password"), CancellationToken.None);

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}

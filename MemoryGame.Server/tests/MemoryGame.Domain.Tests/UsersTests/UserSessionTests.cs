using Xunit;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Common;

namespace MemoryGame.Tests;

public class UserSessionTests
{
    // -----------------------------------------------------------------------
    // Create - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Create_TokenIsValid_ReturnNewUserSession()
    {
        // Arrange
        string token = "a7e6ffba123";
        int userId = 1;
        TimeSpan duration = new TimeSpan(1, 0, 0); // One hour.

        // Act
        UserSession session = UserSession.Create(token, userId, duration);

        // Assert
        Assert.Equal(token, session.Token);
    }

    [Fact]
    public void Create_UserIdIsValid_ReturnNewUserSession()
    {
        // Arrange
        string token = "a7e6ffba123";
        int userId = 1;
        TimeSpan duration = new TimeSpan(1, 0, 0); // One hour.

        // Act
        UserSession session = UserSession.Create(token, userId, duration);

        // Assert
        Assert.Equal(userId, session.UserId);
    }

    // -----------------------------------------------------------------------
    // Create - Invalid/Exception Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Create_TokenIsNull_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        TimeSpan duration = new TimeSpan(1, 0, 0); // One hour.

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            UserSession.Create(null, userId, duration)
        );
    }

    [Fact]
    public void Create_TokenIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string token = " ";
        int userId = 1;
        TimeSpan duration = new TimeSpan(1, 0, 0); // One hour.

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            UserSession.Create(token, userId, duration)
        );
    }


    // -----------------------------------------------------------------------
    // IsExpired - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void IsExpired_Expired_ReturnTrue()
    {
        // Arrange
        string token = "a7e6ffba123";
        int userId = 1;
        TimeSpan duration = new TimeSpan(0); // No time.

        UserSession session = UserSession.Create(token, userId, duration);

        // Act
        bool expired = session.IsExpired();

        // Assert
        Assert.True(expired);
    }

    [Fact]
    public void IsExpired_NotExpired_ReturnFalse()
    {
        // Arrange
        string token = "a7e6ffba123";
        int userId = 1;
        TimeSpan duration = new TimeSpan(1, 0, 0); // One hour.

        UserSession session = UserSession.Create(token, userId, duration);

        // Act
        bool expired = session.IsExpired();

        // Assert
        Assert.False(expired);
    }


    // -----------------------------------------------------------------------
    // Renew - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Renew_DurationIsValid_UpdateExpirationDate()
    {
        // Arrange
        string token = "a7e6ffba123";
        int userId = 1;
        TimeSpan duration = new TimeSpan(0, 0, 0, 1); // One millisecond.

        UserSession session = UserSession.Create(token, userId, duration);

        // Act
        session.Renew(new TimeSpan(1, 0, 0)); // Adds one hour.

        // Assert
        Assert.True(session.ExpiresAt > DateTime.UtcNow);
    }
    
}
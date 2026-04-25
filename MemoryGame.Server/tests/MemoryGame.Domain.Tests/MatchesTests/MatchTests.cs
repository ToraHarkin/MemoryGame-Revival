using MemoryGame.Domain.Common.Enums;
using MemoryGame.Domain.Matches;
using GameMatch = MemoryGame.Domain.Matches.Match;
using Xunit;
using MemoryGame.Domain.Common;

namespace MemoryGame.Tests;

public class MatchTests
{
    // -----------------------------------------------------------------------
    // Create - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Create_StatusIsInProgress_ReturnNewMatch()
    {
        // Act
        GameMatch match = GameMatch.Create();

        // Assert
        Assert.Equal(MatchStatus.InProgress, match.Status);
    }


    // -----------------------------------------------------------------------
    // Finish - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Finish_WinnerIdIsValid_SetMatchStatusAsFinished()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;
        match.AddParticipant(userId);

        // Act
        match.Finish(userId);

        // Assert
        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    // -----------------------------------------------------------------------
    // Create - Invalid/Exception Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Finish_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int winnerUserId = 1;
        match.AddParticipant(winnerUserId);
        
        match.Finish(winnerUserId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Finish(2)
        );
    }

    [Fact]
    public void Finish_UserIdIsNotParticipant_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Finish(userId)
        );
    }


    // -----------------------------------------------------------------------
    // Cancel - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Cancel_CancelIsValid_SetMatchStatusAsCancelled()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        // Act
        match.Cancel();

        // Assert
        Assert.Equal(MatchStatus.Cancelled, match.Status);
    }

    // -----------------------------------------------------------------------
    // Create - Invalid/Exception Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void Cancel_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        match.Cancel();

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.Cancel()
        );
    }

    // -----------------------------------------------------------------------
    // AddParticipant - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void AddParticipant_UserIdIsValid_ReturnNewMatchParticipation()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Act
        MatchParticipation participation = match.AddParticipant(userId);

        // Assert
        Assert.Equal(userId, participation.UserId);
    }

    [Fact]
    public void AddParticipant_MatchIdIsValid_ReturnNewMatchParticipation()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;

        // Act
        MatchParticipation participation = match.AddParticipant(userId);

        // Assert
        Assert.Equal(1, participation.MatchId);
    }

    // -----------------------------------------------------------------------
    // AddParticipant - Invalid/Exception Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void AddParticipant_MatchStatusIsNotInProgress_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int winnerUserId = 1;
        match.AddParticipant(winnerUserId);

        match.Finish(winnerUserId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.AddParticipant(2)
        );
    }

    [Fact]
    public void AddParticipant_ParticipantIsAlreadyInMatch_ThrowDomainException()
    {
        // Arrange
        GameMatch match = GameMatch.Create();

        int userId = 1;
        match.AddParticipant(userId);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            match.AddParticipant(userId)
        );
    }
}
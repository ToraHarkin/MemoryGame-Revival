using GameMatch = MemoryGame.Domain.Matches.Match;
using MemoryGame.Domain.Matches;
using Xunit;

namespace MemoryGame.Tests;

public class MatchParticipationTests
{
    // -----------------------------------------------------------------------
    // AddPoints - Happy Paths
    // -----------------------------------------------------------------------
    [Fact]
    public void AddPoints_PointsAreValid_UpdateScore()
    {
        // Arrange
        GameMatch match = GameMatch.Create();
        MatchParticipation participation = match.AddParticipant(1);

        int points = 750;

        // Act
        participation.AddPoints(points);

        // Assert
        Assert.Equal(points, participation.Score.Value);
    }
}
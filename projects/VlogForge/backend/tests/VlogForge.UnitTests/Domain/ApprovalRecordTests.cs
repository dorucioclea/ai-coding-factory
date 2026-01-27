using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for ApprovalRecord entity.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class ApprovalRecordTests
{
    private readonly Guid _validContentItemId = Guid.NewGuid();
    private readonly Guid _validTeamId = Guid.NewGuid();
    private readonly Guid _validActorId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Act
        var record = ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            _validActorId,
            ApprovalAction.Submitted,
            IdeaStatus.Draft,
            IdeaStatus.InReview,
            "Ready for review");

        // Assert
        record.Should().NotBeNull();
        record.ContentItemId.Should().Be(_validContentItemId);
        record.TeamId.Should().Be(_validTeamId);
        record.ActorId.Should().Be(_validActorId);
        record.Action.Should().Be(ApprovalAction.Submitted);
        record.PreviousStatus.Should().Be(IdeaStatus.Draft);
        record.NewStatus.Should().Be(IdeaStatus.InReview);
        record.Feedback.Should().Be("Ready for review");
    }

    [Fact]
    public void CreateWithNullFeedbackShouldSucceed()
    {
        // Act
        var record = ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            _validActorId,
            ApprovalAction.Approved,
            IdeaStatus.InReview,
            IdeaStatus.Approved);

        // Assert
        record.Feedback.Should().BeNull();
    }

    [Fact]
    public void CreateShouldTrimFeedback()
    {
        // Act
        var record = ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            _validActorId,
            ApprovalAction.ChangesRequested,
            IdeaStatus.InReview,
            IdeaStatus.ChangesRequested,
            "  Needs revision  ");

        // Assert
        record.Feedback.Should().Be("Needs revision");
    }

    [Fact]
    public void CreateWithEmptyContentItemIdShouldThrow()
    {
        // Act
        var act = () => ApprovalRecord.Create(
            Guid.Empty,
            _validTeamId,
            _validActorId,
            ApprovalAction.Submitted,
            IdeaStatus.Draft,
            IdeaStatus.InReview);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Content item ID*empty*");
    }

    [Fact]
    public void CreateWithEmptyTeamIdShouldThrow()
    {
        // Act
        var act = () => ApprovalRecord.Create(
            _validContentItemId,
            Guid.Empty,
            _validActorId,
            ApprovalAction.Submitted,
            IdeaStatus.Draft,
            IdeaStatus.InReview);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Team ID*empty*");
    }

    [Fact]
    public void CreateWithEmptyActorIdShouldThrow()
    {
        // Act
        var act = () => ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            Guid.Empty,
            ApprovalAction.Submitted,
            IdeaStatus.Draft,
            IdeaStatus.InReview);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Actor ID*empty*");
    }

    [Fact]
    public void CreateWithTooLongFeedbackShouldThrow()
    {
        // Arrange
        var longFeedback = new string('a', ApprovalRecord.MaxFeedbackLength + 1);

        // Act
        var act = () => ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            _validActorId,
            ApprovalAction.ChangesRequested,
            IdeaStatus.InReview,
            IdeaStatus.ChangesRequested,
            longFeedback);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Feedback*{ApprovalRecord.MaxFeedbackLength}*");
    }

    #endregion

    #region ApprovalAction Tests

    [Theory]
    [InlineData(ApprovalAction.Submitted)]
    [InlineData(ApprovalAction.Approved)]
    [InlineData(ApprovalAction.ChangesRequested)]
    [InlineData(ApprovalAction.Resubmitted)]
    public void CreateWithAllApprovalActionsShouldSucceed(ApprovalAction action)
    {
        // Act
        var record = ApprovalRecord.Create(
            _validContentItemId,
            _validTeamId,
            _validActorId,
            action,
            IdeaStatus.Draft,
            IdeaStatus.InReview);

        // Assert
        record.Action.Should().Be(action);
    }

    #endregion
}

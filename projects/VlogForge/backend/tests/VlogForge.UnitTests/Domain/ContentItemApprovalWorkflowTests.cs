using FluentAssertions;
using VlogForge.Domain.Entities;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for ContentItem approval workflow status transitions.
/// Story: ACF-009
/// </summary>
[Trait("Story", "ACF-009")]
public class ContentItemApprovalWorkflowTests
{
    private readonly Guid _validUserId = Guid.NewGuid();

    #region Approval Status Transitions

    [Fact]
    public void UpdateStatusFromInReviewToApprovedShouldSucceed()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.InReview);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Approved);

        // Assert
        item.Status.Should().Be(IdeaStatus.Approved);
    }

    [Fact]
    public void UpdateStatusFromInReviewToChangesRequestedShouldSucceed()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.InReview);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.ChangesRequested);

        // Assert
        item.Status.Should().Be(IdeaStatus.ChangesRequested);
    }

    [Fact]
    public void UpdateStatusFromChangesRequestedToDraftShouldSucceed()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.ChangesRequested);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Draft);

        // Assert
        item.Status.Should().Be(IdeaStatus.Draft);
    }

    [Fact]
    public void UpdateStatusFromApprovedToScheduledShouldSucceed()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.Approved);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Scheduled);

        // Assert
        item.Status.Should().Be(IdeaStatus.Scheduled);
    }

    [Fact]
    public void UpdateStatusFromApprovedToInReviewShouldSucceed()
    {
        // Arrange - Allow going back for re-review
        var item = CreateItemInStatus(IdeaStatus.Approved);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.InReview);

        // Assert
        item.Status.Should().Be(IdeaStatus.InReview);
    }

    [Fact]
    public void UpdateStatusFromScheduledToApprovedShouldSucceed()
    {
        // Arrange - Allow going back to Approved
        var item = CreateItemInStatus(IdeaStatus.Scheduled);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Approved);

        // Assert
        item.Status.Should().Be(IdeaStatus.Approved);
    }

    #endregion

    #region Invalid Transitions for Approval Workflow

    [Theory]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Approved)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.ChangesRequested)]
    [InlineData(IdeaStatus.Idea, IdeaStatus.Approved)]
    [InlineData(IdeaStatus.Idea, IdeaStatus.ChangesRequested)]
    [InlineData(IdeaStatus.ChangesRequested, IdeaStatus.Approved)]
    [InlineData(IdeaStatus.Approved, IdeaStatus.Draft)]
    [InlineData(IdeaStatus.Approved, IdeaStatus.ChangesRequested)]
    public void UpdateStatusWithInvalidApprovalTransitionShouldThrow(IdeaStatus from, IdeaStatus to)
    {
        // Arrange
        var item = CreateItemInStatus(from);

        // Act
        var act = () => item.UpdateStatus(to);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Cannot transition from {from} to {to}*");
    }

    #endregion

    #region GetValidTransitions Tests

    [Fact]
    public void GetValidTransitionsFromInReviewShouldIncludeApprovalOptions()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.InReview);

        // Act
        var validTransitions = item.GetValidTransitions();

        // Assert
        validTransitions.Should().Contain(IdeaStatus.Draft);
        validTransitions.Should().Contain(IdeaStatus.Approved);
        validTransitions.Should().Contain(IdeaStatus.ChangesRequested);
        validTransitions.Should().HaveCount(3);
    }

    [Fact]
    public void GetValidTransitionsFromApprovedShouldReturnCorrectOptions()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.Approved);

        // Act
        var validTransitions = item.GetValidTransitions();

        // Assert
        validTransitions.Should().Contain(IdeaStatus.InReview);
        validTransitions.Should().Contain(IdeaStatus.Scheduled);
        validTransitions.Should().HaveCount(2);
    }

    [Fact]
    public void GetValidTransitionsFromChangesRequestedShouldReturnDraftAndInReview()
    {
        // Arrange
        var item = CreateItemInStatus(IdeaStatus.ChangesRequested);

        // Act
        var validTransitions = item.GetValidTransitions();

        // Assert
        validTransitions.Should().Contain(IdeaStatus.Draft);
        validTransitions.Should().Contain(IdeaStatus.InReview);
        validTransitions.Should().HaveCount(2);
    }

    #endregion

    #region Full Workflow Tests

    [Fact]
    public void CompleteApprovalWorkflowShouldSucceed()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Video Idea", "Notes");

        // Act & Assert - Full workflow
        item.UpdateStatus(IdeaStatus.Draft);
        item.Status.Should().Be(IdeaStatus.Draft);

        item.UpdateStatus(IdeaStatus.InReview);
        item.Status.Should().Be(IdeaStatus.InReview);

        item.UpdateStatus(IdeaStatus.Approved);
        item.Status.Should().Be(IdeaStatus.Approved);

        item.UpdateStatus(IdeaStatus.Scheduled);
        item.Status.Should().Be(IdeaStatus.Scheduled);

        item.UpdateStatus(IdeaStatus.Published);
        item.Status.Should().Be(IdeaStatus.Published);
    }

    [Fact]
    public void WorkflowWithChangesRequestedLoopShouldSucceed()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Video Idea", "Notes");

        // Act & Assert - Workflow with changes requested loop
        item.UpdateStatus(IdeaStatus.Draft);
        item.UpdateStatus(IdeaStatus.InReview);
        item.UpdateStatus(IdeaStatus.ChangesRequested); // Reviewer requests changes
        item.UpdateStatus(IdeaStatus.Draft); // Creator revises
        item.UpdateStatus(IdeaStatus.InReview); // Resubmit for review
        item.UpdateStatus(IdeaStatus.Approved); // Approved this time
        item.UpdateStatus(IdeaStatus.Scheduled);

        item.Status.Should().Be(IdeaStatus.Scheduled);
    }

    #endregion

    #region Helper Methods

    private ContentItem CreateItemInStatus(IdeaStatus targetStatus)
    {
        var item = ContentItem.Create(_validUserId, "Test Title", "Test Notes");

        // Define the path to reach each status
        var pathToStatus = targetStatus switch
        {
            IdeaStatus.Idea => Array.Empty<IdeaStatus>(),
            IdeaStatus.Draft => new[] { IdeaStatus.Draft },
            IdeaStatus.InReview => new[] { IdeaStatus.Draft, IdeaStatus.InReview },
            IdeaStatus.Approved => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved },
            IdeaStatus.ChangesRequested => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.ChangesRequested },
            IdeaStatus.Scheduled => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved, IdeaStatus.Scheduled },
            IdeaStatus.Published => new[] { IdeaStatus.Draft, IdeaStatus.InReview, IdeaStatus.Approved, IdeaStatus.Scheduled, IdeaStatus.Published },
            _ => throw new ArgumentException($"Unknown status: {targetStatus}")
        };

        foreach (var status in pathToStatus)
        {
            item.UpdateStatus(status);
        }

        return item;
    }

    #endregion
}

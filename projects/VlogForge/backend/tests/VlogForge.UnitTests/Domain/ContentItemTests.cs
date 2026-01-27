using FluentAssertions;
using VlogForge.Domain.Entities;
using VlogForge.Domain.Events;
using Xunit;

namespace VlogForge.UnitTests.Domain;

/// <summary>
/// Unit tests for ContentItem entity.
/// Story: ACF-005
/// </summary>
[Trait("Story", "ACF-005")]
public class ContentItemTests
{
    private readonly Guid _validUserId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void CreateWithValidDataShouldSucceed()
    {
        // Act
        var item = ContentItem.Create(_validUserId, "My Video Idea", "Notes about the video");

        // Assert
        item.Should().NotBeNull();
        item.UserId.Should().Be(_validUserId);
        item.Title.Should().Be("My Video Idea");
        item.Notes.Should().Be("Notes about the video");
        item.Status.Should().Be(IdeaStatus.Idea);
        item.PlatformTags.Should().BeEmpty();
        item.IsDeleted.Should().BeFalse();
        item.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void CreateShouldTrimTitle()
    {
        // Act
        var item = ContentItem.Create(_validUserId, "  My Video Idea  ", "Notes");

        // Assert
        item.Title.Should().Be("My Video Idea");
    }

    [Fact]
    public void CreateShouldRaiseContentItemCreatedEvent()
    {
        // Act
        var item = ContentItem.Create(_validUserId, "My Video Idea", "Notes");

        // Assert
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContentItemCreatedEvent>()
            .Which.Title.Should().Be("My Video Idea");
    }

    [Fact]
    public void CreateWithEmptyUserIdShouldThrow()
    {
        // Act
        var act = () => ContentItem.Create(Guid.Empty, "Title", "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithEmptyTitleShouldThrow(string? title)
    {
        // Act
        var act = () => ContentItem.Create(_validUserId, title!, "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*");
    }

    [Fact]
    public void CreateWithTooLongTitleShouldThrow()
    {
        // Arrange
        var longTitle = new string('a', ContentItem.MaxTitleLength + 1);

        // Act
        var act = () => ContentItem.Create(_validUserId, longTitle, "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Title cannot exceed {ContentItem.MaxTitleLength} characters.*");
    }

    [Fact]
    public void CreateWithTooLongNotesShouldThrow()
    {
        // Arrange
        var longNotes = new string('a', ContentItem.MaxNotesLength + 1);

        // Act
        var act = () => ContentItem.Create(_validUserId, "Title", longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Notes cannot exceed {ContentItem.MaxNotesLength} characters.*");
    }

    [Fact]
    public void CreateWithNullNotesShouldSucceed()
    {
        // Act
        var item = ContentItem.Create(_validUserId, "Title", null);

        // Assert
        item.Notes.Should().BeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void UpdateShouldUpdateTitleAndNotes()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Old Title", "Old Notes");
        item.ClearDomainEvents();

        // Act
        item.Update("New Title", "New Notes");

        // Assert
        item.Title.Should().Be("New Title");
        item.Notes.Should().Be("New Notes");
    }

    [Fact]
    public void UpdateShouldRaiseContentItemUpdatedEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.Update("New Title", "New Notes");

        // Assert
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContentItemUpdatedEvent>();
    }

    [Fact]
    public void UpdateWithEmptyTitleShouldThrow()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");

        // Act
        var act = () => item.Update("", "Notes");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty.*");
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void UpdateStatusFromIdeaToDraftShouldSucceed()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Draft);

        // Assert
        item.Status.Should().Be(IdeaStatus.Draft);
    }

    [Fact]
    public void UpdateStatusShouldRaiseStatusChangedEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Draft);

        // Assert
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContentItemStatusChangedEvent>()
            .Which.NewStatus.Should().Be(IdeaStatus.Draft);
    }

    [Theory]
    [InlineData(IdeaStatus.Idea, IdeaStatus.Draft)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.InReview)]
    [InlineData(IdeaStatus.InReview, IdeaStatus.Scheduled)]
    [InlineData(IdeaStatus.Scheduled, IdeaStatus.Published)]
    public void UpdateStatusToNextValidStatusShouldSucceed(IdeaStatus from, IdeaStatus to)
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        MoveToStatus(item, from);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(to);

        // Assert
        item.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(IdeaStatus.Idea, IdeaStatus.InReview)]
    [InlineData(IdeaStatus.Idea, IdeaStatus.Scheduled)]
    [InlineData(IdeaStatus.Idea, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Scheduled)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Published)]
    [InlineData(IdeaStatus.InReview, IdeaStatus.Published)]
    public void UpdateStatusSkippingStepsShouldThrow(IdeaStatus from, IdeaStatus to)
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        MoveToStatus(item, from);

        // Act
        var act = () => item.UpdateStatus(to);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Cannot transition from {from} to {to}.*");
    }

    [Theory]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Idea)]
    [InlineData(IdeaStatus.InReview, IdeaStatus.Draft)]
    [InlineData(IdeaStatus.Scheduled, IdeaStatus.InReview)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Scheduled)]
    public void UpdateStatusBackwardsShouldSucceed(IdeaStatus from, IdeaStatus to)
    {
        // Arrange - Allow moving backwards in workflow
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        MoveToStatus(item, from);
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(to);

        // Assert
        item.Status.Should().Be(to);
    }

    [Fact]
    public void UpdateStatusToSameStatusShouldNotRaiseEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.UpdateStatus(IdeaStatus.Idea);

        // Assert
        item.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Platform Tags Tests

    [Fact]
    public void AddPlatformTagShouldAddTag()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");

        // Act
        var result = item.AddPlatformTag("YouTube");

        // Assert
        result.Should().BeTrue();
        item.PlatformTags.Should().ContainSingle().Which.Should().Be("youtube");
    }

    [Fact]
    public void AddPlatformTagShouldNormalizeToLowerCase()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");

        // Act
        item.AddPlatformTag("YOUTUBE");

        // Assert
        item.PlatformTags.Should().ContainSingle().Which.Should().Be("youtube");
    }

    [Fact]
    public void AddDuplicatePlatformTagShouldReturnFalse()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.AddPlatformTag("YouTube");

        // Act
        var result = item.AddPlatformTag("youtube");

        // Assert
        result.Should().BeFalse();
        item.PlatformTags.Should().ContainSingle();
    }

    [Fact]
    public void AddPlatformTagBeyondMaxShouldReturnFalse()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        for (var i = 0; i < ContentItem.MaxPlatformTags; i++)
        {
            item.AddPlatformTag($"tag{i}");
        }

        // Act
        var result = item.AddPlatformTag("extra-tag");

        // Assert
        result.Should().BeFalse();
        item.PlatformTags.Should().HaveCount(ContentItem.MaxPlatformTags);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddEmptyPlatformTagShouldReturnFalse(string? tag)
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");

        // Act
        var result = item.AddPlatformTag(tag!);

        // Assert
        result.Should().BeFalse();
        item.PlatformTags.Should().BeEmpty();
    }

    [Fact]
    public void RemovePlatformTagShouldRemoveTag()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.AddPlatformTag("YouTube");

        // Act
        var result = item.RemovePlatformTag("YouTube");

        // Assert
        result.Should().BeTrue();
        item.PlatformTags.Should().BeEmpty();
    }

    [Fact]
    public void RemovePlatformTagShouldBeCaseInsensitive()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.AddPlatformTag("YouTube");

        // Act
        var result = item.RemovePlatformTag("YOUTUBE");

        // Assert
        result.Should().BeTrue();
        item.PlatformTags.Should().BeEmpty();
    }

    [Fact]
    public void SetPlatformTagsShouldReplaceAllTags()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.AddPlatformTag("old-tag");
        var newTags = new[] { "YouTube", "TikTok" };

        // Act
        item.SetPlatformTags(newTags);

        // Assert
        item.PlatformTags.Should().HaveCount(2);
        item.PlatformTags.Should().Contain("youtube");
        item.PlatformTags.Should().Contain("tiktok");
    }

    [Fact]
    public void SetPlatformTagsShouldLimitToMax()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        var tags = Enumerable.Range(0, 20).Select(i => $"tag{i}").ToList();

        // Act
        item.SetPlatformTags(tags);

        // Assert
        item.PlatformTags.Should().HaveCount(ContentItem.MaxPlatformTags);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDeleteShouldMarkAsDeleted()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.SoftDelete();

        // Assert
        item.IsDeleted.Should().BeTrue();
        item.DeletedAt.Should().NotBeNull();
        item.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SoftDeleteShouldRaiseDeletedEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.SoftDelete();

        // Assert
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContentItemDeletedEvent>();
    }

    [Fact]
    public void SoftDeleteAlreadyDeletedShouldNotRaiseEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.SoftDelete();
        item.ClearDomainEvents();

        // Act
        item.SoftDelete();

        // Assert
        item.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RestoreShouldUnmarkAsDeleted()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.SoftDelete();
        item.ClearDomainEvents();

        // Act
        item.Restore();

        // Assert
        item.IsDeleted.Should().BeFalse();
        item.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void RestoreShouldRaiseRestoredEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.SoftDelete();
        item.ClearDomainEvents();

        // Act
        item.Restore();

        // Assert
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContentItemRestoredEvent>();
    }

    [Fact]
    public void RestoreNotDeletedShouldNotRaiseEvent()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.ClearDomainEvents();

        // Act
        item.Restore();

        // Assert
        item.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDeletedItemShouldThrow()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.SoftDelete();

        // Act
        var act = () => item.Update("New Title", "New Notes");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot modify a deleted content item.*");
    }

    [Fact]
    public void UpdateStatusDeletedItemShouldThrow()
    {
        // Arrange
        var item = ContentItem.Create(_validUserId, "Title", "Notes");
        item.SoftDelete();

        // Act
        var act = () => item.UpdateStatus(IdeaStatus.Draft);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot modify a deleted content item.*");
    }

    #endregion

    #region Helper Methods

    private static void MoveToStatus(ContentItem item, IdeaStatus targetStatus)
    {
        var transitions = new[]
        {
            IdeaStatus.Idea,
            IdeaStatus.Draft,
            IdeaStatus.InReview,
            IdeaStatus.Scheduled,
            IdeaStatus.Published
        };

        var currentIndex = Array.IndexOf(transitions, item.Status);
        var targetIndex = Array.IndexOf(transitions, targetStatus);

        for (var i = currentIndex + 1; i <= targetIndex; i++)
        {
            item.UpdateStatus(transitions[i]);
        }
    }

    #endregion
}

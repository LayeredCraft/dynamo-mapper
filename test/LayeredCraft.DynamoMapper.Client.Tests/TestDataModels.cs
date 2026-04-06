namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class UserProfile
{
    public required string Pk { get; init; }

    public required string Sk { get; init; }

    public required string EntityType { get; init; }

    public required string UserId { get; init; }

    public required string Email { get; init; }

    public required string DisplayName { get; init; }

    public int Age { get; init; }

    public bool IsActive { get; init; }

    public decimal AccountBalance { get; init; }

    public required string CreatedAt { get; init; }

    public long LastLoginEpoch { get; init; }

    public required List<string> Tags { get; init; }

    public required UserPreferences Preferences { get; init; }

    public required List<LoginHistoryEntry> LoginHistory { get; init; }

    public required byte[] ProfilePhoto { get; init; }
}

public sealed class ProjectRecord
{
    public required string Pk { get; init; }

    public required string Sk { get; init; }

    public required string EntityType { get; init; }

    public required string ProjectId { get; init; }

    public required string OwnerUserId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public decimal Budget { get; init; }

    public bool IsArchived { get; init; }

    public int Priority { get; init; }

    public required string StartDate { get; init; }

    public required string DueDate { get; init; }

    public required List<string> Labels { get; init; }

    public required ProjectSettings Settings { get; init; }

    public required ProjectMetrics Metrics { get; init; }
}

public sealed class TaskRecord
{
    public required string Pk { get; init; }

    public required string Sk { get; init; }

    public required string EntityType { get; init; }

    public required string TaSkId { get; init; }

    public required string ProjectId { get; init; }

    public required string AssignedUserId { get; init; }

    public required string Title { get; init; }

    public required string Notes { get; init; }

    public decimal EstimateHours { get; init; }

    public bool Completed { get; init; }

    public int Order { get; init; }

    public required string CreatedAt { get; init; }

    public required string DueAt { get; init; }

    public required List<TaSkChecklistItem> Checklist { get; init; }

    public required TaSkMetadata Metadata { get; init; }
}

public sealed class UserPreferences
{
    public required string Theme { get; init; }

    public bool NotificationsEnabled { get; init; }

    public required string Language { get; init; }
}

public sealed class LoginHistoryEntry
{
    public required string At { get; init; }

    public required string IpAddress { get; init; }
}

public sealed class ProjectSettings
{
    public required string Visibility { get; init; }

    public bool AllowGuestComments { get; init; }
}

public sealed class ProjectMetrics
{
    public int TaSkCount { get; init; }

    public int CompletedTaSkCount { get; init; }
}

public sealed class TaSkChecklistItem
{
    public required string Text { get; init; }

    public bool Done { get; init; }
}

public sealed class TaSkMetadata
{
    public required string Color { get; init; }

    public string? BlockedBy { get; init; }
}

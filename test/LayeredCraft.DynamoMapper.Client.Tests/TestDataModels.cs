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

public static class TestDataSamples
{
    public static IReadOnlyList<UserProfile> UserProfiles { get; } =
    [
        new()
        {
            Pk = "USER#u-1001",
            Sk = "PROFILE#u-1001",
            EntityType = "UserProfile",
            UserId = "u-1001",
            Email = "alex.carter@example.com",
            DisplayName = "Alex Carter",
            Age = 34,
            IsActive = true,
            AccountBalance = 1520.75m,
            CreatedAt = "2025-01-10T09:15:00Z",
            LastLoginEpoch = 1739529600,
            Tags = ["admin", "beta", "us-east-1"],
            Preferences =
                new UserPreferences
                {
                    Theme = "dark", NotificationsEnabled = true, Language = "en-US",
                },
            LoginHistory =
            [
                new LoginHistoryEntry
                {
                    At = "2025-02-11T08:00:00Z", IpAddress = "203.0.113.10",
                },
                new LoginHistoryEntry
                {
                    At = "2025-02-12T18:45:00Z", IpAddress = "203.0.113.11",
                },
            ],
            ProfilePhoto = [1, 2, 3, 4, 5],
        },
        new()
        {
            Pk = "USER#u-1002",
            Sk = "PROFILE#u-1002",
            EntityType = "UserProfile",
            UserId = "u-1002",
            Email = "maya.chen@example.com",
            DisplayName = "Maya Chen",
            Age = 29,
            IsActive = false,
            AccountBalance = 87.40m,
            CreatedAt = "2024-11-03T14:20:00Z",
            LastLoginEpoch = 1738771200,
            Tags = ["designer", "trial"],
            Preferences =
                new UserPreferences
                {
                    Theme = "light", NotificationsEnabled = false, Language = "en-GB",
                },
            LoginHistory =
            [
                new LoginHistoryEntry
                {
                    At = "2025-01-28T12:15:00Z", IpAddress = "198.51.100.25",
                },
                new LoginHistoryEntry
                {
                    At = "2025-02-05T07:32:00Z", IpAddress = "198.51.100.44",
                },
            ],
            ProfilePhoto = [10, 20, 30, 40],
        },
    ];

    public static IReadOnlyList<ProjectRecord> ProjectRecords { get; } =
    [
        new()
        {
            Pk = "USER#u-1001",
            Sk = "PROJECT#p-2001",
            EntityType = "ProjectRecord",
            ProjectId = "p-2001",
            OwnerUserId = "u-1001",
            Name = "Apollo Migration",
            Description = "Move customer workflows to the new platform.",
            Budget = 125000.00m,
            IsArchived = false,
            Priority = 1,
            StartDate = "2025-02-01",
            DueDate = "2025-06-30",
            Labels = ["migration", "high-priority", "enterprise"],
            Settings =
                new ProjectSettings { Visibility = "private", AllowGuestComments = false },
            Metrics = new ProjectMetrics { TaSkCount = 18, CompletedTaSkCount = 7 },
        },
        new()
        {
            Pk = "USER#u-1002",
            Sk = "PROJECT#p-2002",
            EntityType = "ProjectRecord",
            ProjectId = "p-2002",
            OwnerUserId = "u-1002",
            Name = "Website Refresh",
            Description = "Update marketing pages and design tokens.",
            Budget = 18000.50m,
            IsArchived = false,
            Priority = 2,
            StartDate = "2025-03-15",
            DueDate = "2025-05-01",
            Labels = ["design", "marketing"],
            Settings =
                new ProjectSettings { Visibility = "team", AllowGuestComments = true },
            Metrics = new ProjectMetrics { TaSkCount = 9, CompletedTaSkCount = 3 },
        },
    ];

    public static IReadOnlyList<TaskRecord> TaskRecords { get; } =
    [
        new()
        {
            Pk = "PROJECT#p-2001",
            Sk = "TASK#t-3001",
            EntityType = "TaskRecord",
            TaSkId = "t-3001",
            ProjectId = "p-2001",
            AssignedUserId = "u-1001",
            Title = "Audit existing integrations",
            Notes = "Document external dependencies and rate limits.",
            EstimateHours = 6.5m,
            Completed = true,
            Order = 1,
            CreatedAt = "2025-02-02T10:00:00Z",
            DueAt = "2025-02-05T17:00:00Z",
            Checklist =
            [
                new TaSkChecklistItem { Text = "List current providers", Done = true },
                new TaSkChecklistItem { Text = "Capture auth mechanisms", Done = true },
            ],
            Metadata = new TaSkMetadata { Color = "green", BlockedBy = null },
        },
        new()
        {
            Pk = "PROJECT#p-2002",
            Sk = "TASK#t-3002",
            EntityType = "TaskRecord",
            TaSkId = "t-3002",
            ProjectId = "p-2002",
            AssignedUserId = "u-1002",
            Title = "Create homepage mockups",
            Notes = "Deliver desktop and mobile variants for review.",
            EstimateHours = 12.0m,
            Completed = false,
            Order = 2,
            CreatedAt = "2025-03-16T09:30:00Z",
            DueAt = "2025-03-20T16:00:00Z",
            Checklist =
            [
                new TaSkChecklistItem { Text = "Collect brand assets", Done = true },
                new TaSkChecklistItem { Text = "Draft hero section", Done = false },
            ],
            Metadata = new TaSkMetadata
            {
                Color = "orange", BlockedBy = "Awaiting stakeholder feedback",
            },
        },
    ];
}

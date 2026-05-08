using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Client.Tests;

[DynamoMapper]
public partial class UserProfileMapper : IDynamoMapper<UserProfile>
{
    public partial Dictionary<string, AttributeValue> ToItem(UserProfile source);

    public partial UserProfile FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper]
public partial class ProjectRecordMapper : IDynamoMapper<ProjectRecord>
{
    public partial Dictionary<string, AttributeValue> ToItem(ProjectRecord source);

    public partial ProjectRecord FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper]
public partial class TaskRecordMapper : IDynamoMapper<TaskRecord>
{
    public partial Dictionary<string, AttributeValue> ToItem(TaskRecord source);

    public partial TaskRecord FromItem(Dictionary<string, AttributeValue> item);
}

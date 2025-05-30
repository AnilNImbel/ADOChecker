using System.ComponentModel;

namespace ADOAnalyser.Enum
{
    enum ResultEnum
    {
        Missing,
        Attached,
        Completed,
        Yes,
        No,
        Filled,
        Pending,
        [Description("Not Applicable")]
        NotApplicable
    }

    enum StateStatusEnum
    {
        Closed,
        Resolved,
        New,
        Test,
        Active
    }

    enum DevelopmentStatusEnum
    {

        [Description("In Development")]
        InDevelopment,

    }

    enum WorkTypeEnum
    {
        [Description("User Story")]
        UserStory,
        [Description("Bug")]
        Bug,
        [Description("Production Defect")]
        ProductionDefect,
    }
}

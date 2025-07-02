using Newtonsoft.Json;

namespace ADOAnalyser.Models.TestModel
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Attributes
    {
        public bool isLocked { get; set; }
        public string name { get; set; }
    }

    public class Fields
    {
        [JsonProperty("CivicaAgile.TestLevel")]
        public string CivicaAgileTestLevel { get; set; }

        [JsonProperty("CivicaAgile.TestPhase")]
        public string CivicaAgileTestPhase { get; set; }

        [JsonProperty("Custom.TestType")]
        public string CustomTestType { get; set; }

        [JsonProperty("Microsoft.VSTS.TCM.AutomationStatus")]
        public string MicrosoftVSTSTCMAutomationStatus { get; set; }

        [JsonProperty("System.AreaPath")]
        public string SystemAreaPath { get; set; }

        [JsonProperty("System.TeamProject")]
        public string SystemTeamProject { get; set; }

        [JsonProperty("System.IterationPath")]
        public string SystemIterationPath { get; set; }

        [JsonProperty("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        [JsonProperty("System.State")]
        public string SystemState { get; set; }

        [JsonProperty("System.Reason")]
        public string SystemReason { get; set; }

        [JsonProperty("System.AssignedTo")]
        public string SystemAssignedTo { get; set; }

        [JsonProperty("System.CreatedDate")]
        public DateTime SystemCreatedDate { get; set; }

        [JsonProperty("System.CreatedBy")]
        public string SystemCreatedBy { get; set; }

        [JsonProperty("System.ChangedDate")]
        public DateTime SystemChangedDate { get; set; }

        [JsonProperty("System.ChangedBy")]
        public string SystemChangedBy { get; set; }

        [JsonProperty("System.CommentCount")]
        public int SystemCommentCount { get; set; }

        [JsonProperty("System.Title")]
        public string SystemTitle { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.StateChangeDate")]
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ActivatedDate")]
        public DateTime MicrosoftVSTSCommonActivatedDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ActivatedBy")]
        public string MicrosoftVSTSCommonActivatedBy { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int MicrosoftVSTSCommonPriority { get; set; }

        [JsonProperty("System.History")]
        public string SystemHistory { get; set; }

        [JsonProperty("Microsoft.VSTS.TCM.Steps")]
        public string MicrosoftVSTSTCMSteps { get; set; }
    }

    public class Relation
    {
        public string rel { get; set; }
        public string url { get; set; }
        public Attributes attributes { get; set; }
    }

    public class TestCaseModel
    {
        public int count { get; set; }
        public List<Value> value { get; set; }
    }

    public class Value
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public List<Relation> relations { get; set; }
        public string url { get; set; }
    }


}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOAnalyser.Models
{
    public class WorkItemModel
    {
        public int count { get; set; }
        public List<Values> value { get; set; }
    }

    public class Fields
    {
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

        [JsonProperty("System.BoardColumn")]
        public string SystemBoardColumn { get; set; }

        [JsonProperty("System.BoardColumnDone")]
        public bool SystemBoardColumnDone { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.StateChangeDate")]
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ValueArea")]
        public string MicrosoftVSTSCommonValueArea { get; set; }

        [JsonProperty("CivicaAgile.VIEWCAP")]
        public bool CivicaAgileVIEWCAP { get; set; }

        [JsonProperty("Custom.JiraKey")]
        public string CustomJiraKey { get; set; }

        [JsonProperty("Custom.JiraSync")]
        public bool CustomJiraSync { get; set; }

        [JsonProperty("WEF_A799486F497940478970853F5B3EA295_Kanban.Column")]
        public string WEF_A799486F497940478970853F5B3EA295_KanbanColumn { get; set; }

        [JsonProperty("WEF_A799486F497940478970853F5B3EA295_Kanban.Column.Done")]
        public bool WEF_A799486F497940478970853F5B3EA295_KanbanColumnDone { get; set; }

        [JsonProperty("System.Description")]
        public string SystemDescription { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.AcceptanceCriteria")]
        public string MicrosoftVSTSCommonAcceptanceCriteria { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ActivatedDate")]
        public DateTime? MicrosoftVSTSCommonActivatedDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ActivatedBy")]
        public string MicrosoftVSTSCommonActivatedBy { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ResolvedDate")]
        public DateTime? MicrosoftVSTSCommonResolvedDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ResolvedBy")]
        public string MicrosoftVSTSCommonResolvedBy { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ClosedDate")]
        public DateTime? MicrosoftVSTSCommonClosedDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.ClosedBy")]
        public string MicrosoftVSTSCommonClosedBy { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int? MicrosoftVSTSCommonPriority { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.StackRank")]
        public double? MicrosoftVSTSCommonStackRank { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.TargetDate")]
        public DateTime? MicrosoftVSTSSchedulingTargetDate { get; set; }

        [JsonProperty("CivicaAgile.FunctionalArea")]
        public string CivicaAgileFunctionalArea { get; set; }

        [JsonProperty("Custom.VIEWPerformanceAnalysisComplete")]
        public string CustomVIEWPerformanceAnalysisComplete { get; set; }

        [JsonProperty("Custom.VIEWPerformanceSuiteUpdateRequired")]
        public string CustomVIEWPerformanceSuiteUpdateRequired { get; set; }

        [JsonProperty("Custom.VIEWPerformanceTestAdded")]
        public string CustomVIEWPerformanceTestAdded { get; set; }

        [JsonProperty("WEF_C73ED0D35FEC4159BB0D807A6B360EE6_Kanban.Column")]
        public string WEF_C73ED0D35FEC4159BB0D807A6B360EE6_KanbanColumn { get; set; }

        [JsonProperty("WEF_C73ED0D35FEC4159BB0D807A6B360EE6_Kanban.Column.Done")]
        public bool? WEF_C73ED0D35FEC4159BB0D807A6B360EE6_KanbanColumnDone { get; set; }

        [JsonProperty("Microsoft.VSTS.CMMI.ImpactAssessmentHtml")]
        public string MicrosoftVSTSCMMIImpactAssessmentHtml { get; set; }

        [JsonProperty("System.Tags")]
        public string SystemTags { get; set; }

        [JsonProperty("Microsoft.VSTS.Scheduling.StoryPoints")]
        public double? MicrosoftVSTSSchedulingStoryPoints { get; set; }

        [JsonProperty("Custom.StateUSStatus")]
        public string CustomStateUSStatus { get; set; }

        [JsonProperty("Custom.VIEWPRFollowingVCPolicy")]
        public string CustomVIEWPRFollowingVCPolicy { get; set; }

        [JsonProperty("Custom.VIEWPRWorkItemCount")]
        public int? CustomVIEWPRWorkItemCount { get; set; }

        [JsonProperty("Custom.VIEWPRExceeded50Files")]
        public string CustomVIEWPRExceeded50Files { get; set; }

        [JsonProperty("Custom.VIEWPRCourtsFunctionality")]
        public string CustomVIEWPRCourtsFunctionality { get; set; }

        [JsonProperty("Custom.VIEWPRImpactsPerformance")]
        public string CustomVIEWPRImpactsPerformance { get; set; }

        [JsonProperty("Custom.VIEWPRAcceptanceCriteriaCount")]
        public int? CustomVIEWPRAcceptanceCriteriaCount { get; set; }

        [JsonProperty("Custom.VIEWPRSonarQubeClean")]
        public string CustomVIEWPRSonarQubeClean { get; set; }

        [JsonProperty("Custom.VIEWPRSQLMappedParamsCount")]
        public int? CustomVIEWPRSQLMappedParamsCount { get; set; }

        [JsonProperty("Custom.VIEWPRImpactedSPCount")]
        public int? CustomVIEWPRImpactedSPCount { get; set; }

        [JsonProperty("Custom.VIEWPRManualUnitTestCount")]
        public int? CustomVIEWPRManualUnitTestCount { get; set; }

        [JsonProperty("Custom.VIEWPRUITestCount")]
        public int? CustomVIEWPRUITestCount { get; set; }

        [JsonProperty("Custom.VIEWPRMaskedDBINTUsed")]
        public string CustomVIEWPRMaskedDBINTUsed { get; set; }

        [JsonProperty("Custom.VIEWPRAutomatedUnitTestCount")]
        public int? CustomVIEWPRAutomatedUnitTestCount { get; set; }

        [JsonProperty("Custom.VIEWPRDBTestCount")]
        public int? CustomVIEWPRDBTestCount { get; set; }

        [JsonProperty("Custom.VIEWPRLegacyCode")]
        public string CustomVIEWPRLegacyCode { get; set; }

        [JsonProperty("Custom.VIEWPRCompletedDemo")]
        public string CustomVIEWPRCompletedDemo { get; set; }

        [JsonProperty("Custom.VIEWPRCodeReviewed")]
        public string CustomVIEWPRCodeReviewed { get; set; }

        [JsonProperty("Custom.VIEWPRExtraTechDebt")]
        public string CustomVIEWPRExtraTechDebt { get; set; }

        [JsonProperty("Custom.VIEWPRLocalisedTechDebt")]
        public string CustomVIEWPRLocalisedTechDebt { get; set; }

        [JsonProperty("Custom.VIEWPRPrintandCorro")]
        public string CustomVIEWPRPrintandCorro { get; set; }

        [JsonProperty("Custom.VIEWPRDBVIEWChanges")]
        public string CustomVIEWPRDBVIEWChanges { get; set; }

        [JsonProperty("Custom.VIEWPRFollowPG")]
        public string CustomVIEWPRFollowPG { get; set; }

        [JsonProperty("Custom.VIEWPRImpactAnalysisHours")]
        public double? CustomVIEWPRImpactAnalysisHours { get; set; }

        [JsonProperty("Custom.VIEWPRPlannedEffortHours")]
        public double? CustomVIEWPRPlannedEffortHours { get; set; }

        [JsonProperty("Custom.VIEWPRActualEffortHours")]
        public double? CustomVIEWPRActualEffortHours { get; set; }

        [JsonProperty("Custom.VIEWPRPlannedvsActualGap")]
        public string CustomVIEWPRPlannedvsActualGap { get; set; }

        [JsonProperty("Custom.VIEWPRDetailsTab")]
        public string CustomVIEWPRDetailsTab { get; set; }

        [JsonProperty("Custom.VIEWPRCompatibleDataTypes")]
        public string CustomVIEWPRCompatibleDataTypes { get; set; }

        [JsonProperty("Custom.VIEWPRChecklistConfidence")]
        public double? CustomVIEWPRChecklistConfidence { get; set; }

        [JsonProperty("Custom.DevelopmentStatus")]
        public string CustomDevelopmentStatus { get; set; }

        [JsonProperty("Custom.QAStatus")]
        public string CustomQAStatus { get; set; }

        [JsonProperty("Custom.VIEWPRImpactAssessmentCompleted")]
        public string CustomVIEWPRImpactAssessmentCompleted { get; set; }

        [JsonProperty("WEF_439E363528544D1AA401EECA721CB80C_Kanban.Column")]
        public string WEF_439E363528544D1AA401EECA721CB80C_KanbanColumn { get; set; }

        [JsonProperty("WEF_439E363528544D1AA401EECA721CB80C_Kanban.Column.Done")]
        public bool? WEF_439E363528544D1AA401EECA721CB80C_KanbanColumnDone { get; set; }

        [JsonProperty("WEF_A5E90557CE584CF58671D0A68EE15CC4_Kanban.Column")]
        public string WEF_A5E90557CE584CF58671D0A68EE15CC4_KanbanColumn { get; set; }

        [JsonProperty("WEF_A5E90557CE584CF58671D0A68EE15CC4_Kanban.Column.Done")]
        public bool? WEF_A5E90557CE584CF58671D0A68EE15CC4_KanbanColumnDone { get; set; }

        [JsonProperty("CivicaAgile.VIEWTargetDate")]
        public DateTime? CivicaAgileVIEWTargetDate { get; set; }
        public string IAAttached { get; set; }
    }

    public class Values
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public string url { get; set; }
    }
}

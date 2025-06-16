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

        public int missingIACount { get; set; }
        public int missingRootCauseCount { get; set; }
        public int missingProjectZeroCount { get; set; }
        public int missingPRLifeCycleCount { get; set; }
        public int missingStatusDiscreCount { get; set; }
        public int missingTestCaseCount { get; set; }
        public int missingVTDCount { get; set; }
        public int missingVLDBCount { get; set; }
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

        [JsonProperty("System.BoardColumn")]
        public string SystemBoardColumn { get; set; }

        [JsonProperty("System.BoardColumnDone")]
        public bool SystemBoardColumnDone { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.StateChangeDate")]
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int MicrosoftVSTSCommonPriority { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Severity")]
        public string MicrosoftVSTSCommonSeverity { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.StackRank")]
        public double MicrosoftVSTSCommonStackRank { get; set; }

        [JsonProperty("CivicaAgile.CallReference")]
        public string CivicaAgileCallReference { get; set; }

        [JsonProperty("CivicaAgile.Customer")]
        public string CivicaAgileCustomer { get; set; }

        [JsonProperty("CivicaAgile.Reproducible")]
        public string CivicaAgileReproducible { get; set; }

        [JsonProperty("CivicaAgile.VIEWFoundInDrop")]
        public string CivicaAgileVIEWFoundInDrop { get; set; }

        [JsonProperty("CivicaAgile.VisibletoCustomer")]
        public string CivicaAgileVisibletoCustomer { get; set; }

        [JsonProperty("CivicaAgile.VIEWCivicaAusVerified")]
        public bool CivicaAgileVIEWCivicaAusVerified { get; set; }

        [JsonProperty("CivicaAgile.VIEWStateVerified")]
        public bool CivicaAgileVIEWStateVerified { get; set; }

        [JsonProperty("CivicaAgile.VIEWTargetDate")]
        public DateTime? CivicaAgileVIEWTargetDate { get; set; }

        [JsonProperty("CivicaAgile.VIEWTargetDrop")]
        public string CivicaAgileVIEWTargetDrop { get; set; }

        [JsonProperty("CivicaAgile.VIEWCAP")]
        public bool CivicaAgileVIEWCAP { get; set; }

        [JsonProperty("CivicaAgile.VIEWDevelopmentOrderLocked")]
        public bool CivicaAgileVIEWDevelopmentOrderLocked { get; set; }

        [JsonProperty("CivicaAgile.VIEWFunctionalArea")]
        public string CivicaAgileVIEWFunctionalArea { get; set; }

        [JsonProperty("CivicaAgile.RootCauseAnalysis")]
        public string CivicaAgileRootCauseAnalysis { get; set; }

        [JsonProperty("WEF_EF1163EDF32B44FD8AB42CB9A846C39B_Kanban.Column")]
        public string WEF_EF1163EDF32B44FD8AB42CB9A846C39B_KanbanColumn { get; set; }

        [JsonProperty("WEF_EF1163EDF32B44FD8AB42CB9A846C39B_Kanban.Column.Done")]
        public bool WEF_EF1163EDF32B44FD8AB42CB9A846C39B_KanbanColumnDone { get; set; }

        [JsonProperty("WEF_EF1163EDF32B44FD8AB42CB9A846C39B_Kanban.Lane")]
        public string WEF_EF1163EDF32B44FD8AB42CB9A846C39B_KanbanLane { get; set; }

        [JsonProperty("CivicaAgileCE.VIEWDataOCR")]
        public string CivicaAgileCEVIEWDataOCR { get; set; }

        [JsonProperty("CivicaAgileCE.CallCreatedDate")]
        public DateTime CivicaAgileCECallCreatedDate { get; set; }

        [JsonProperty("Custom.VIEWDefectPrioritised")]
        public bool CustomVIEWDefectPrioritised { get; set; }

        [JsonProperty("Custom.VIEWDefectPrioritisedByName")]
        public string CustomVIEWDefectPrioritisedByName { get; set; }

        [JsonProperty("Custom.VIEWDataFixRequired")]
        public bool CustomVIEWDataFixRequired { get; set; }

        [JsonProperty("Custom.VIEWLanDeskBreakDate")]
        public DateTime? CustomVIEWLanDeskBreakDate { get; set; }

        [JsonProperty("WEF_FBE0420FA58B4F4FB191B6529535685E_Kanban.Column")]
        public string WEF_FBE0420FA58B4F4FB191B6529535685E_KanbanColumn { get; set; }

        [JsonProperty("WEF_FBE0420FA58B4F4FB191B6529535685E_Kanban.Column.Done")]
        public bool WEF_FBE0420FA58B4F4FB191B6529535685E_KanbanColumnDone { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisDetail")]
        public string CustomRootCauseAnalysisDetail { get; set; }

        [JsonProperty("WEF_D598F79C3D004AC1B346418A9456718C_Kanban.Column")]
        public string WEF_D598F79C3D004AC1B346418A9456718C_KanbanColumn { get; set; }

        [JsonProperty("WEF_D598F79C3D004AC1B346418A9456718C_Kanban.Column.Done")]
        public bool WEF_D598F79C3D004AC1B346418A9456718C_KanbanColumnDone { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisWhy1")]
        public string CustomRootCauseAnalysisWhy1 { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisWhy2")]
        public string CustomRootCauseAnalysisWhy2 { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisWhy3")]
        public string CustomRootCauseAnalysisWhy3 { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisRemediation")]
        public string CustomRootCauseAnalysisRemediation { get; set; }

        [JsonProperty("Custom.RootCauseAnalysisRemediationLead")]
        public string CustomRootCauseAnalysisRemediationLead { get; set; }

        [JsonProperty("Custom.RemediationOwner")]
        public string CustomRemediationOwner { get; set; }

        [JsonProperty("Custom.RemediationStatus")]
        public string CustomRemediationStatus { get; set; }

        [JsonProperty("Custom.VIEWClosedinSprint")]
        public string CustomVIEWClosedinSprint { get; set; }

        [JsonProperty("Custom.ProjectZEROTrend")]
        public string CustomProjectZEROTrend { get; set; }

        [JsonProperty("Custom.ProjectZEROAnalysisStatus")]
        public string CustomProjectZEROAnalysisStatus { get; set; }

        [JsonProperty("Custom.VIEWPRFollowingVCPolicy")]
        public string CustomVIEWPRFollowingVCPolicy { get; set; }

        [JsonProperty("Custom.VIEWPRWorkItemCount")]
        public int CustomVIEWPRWorkItemCount { get; set; }

        [JsonProperty("Custom.VIEWPRExceeded50Files")]
        public string CustomVIEWPRExceeded50Files { get; set; }

        [JsonProperty("Custom.VIEWPRCourtsFunctionality")]
        public string CustomVIEWPRCourtsFunctionality { get; set; }

        [JsonProperty("Custom.VIEWPRImpactsPerformance")]
        public string CustomVIEWPRImpactsPerformance { get; set; }

        [JsonProperty("Custom.VIEWPRAcceptanceCriteriaCount")]
        public int CustomVIEWPRAcceptanceCriteriaCount { get; set; }

        [JsonProperty("Custom.VIEWPRSonarQubeClean")]
        public string CustomVIEWPRSonarQubeClean { get; set; }

        [JsonProperty("Custom.VIEWPRSQLMappedParamsCount")]
        public int CustomVIEWPRSQLMappedParamsCount { get; set; }

        [JsonProperty("Custom.VIEWPRImpactedSPCount")]
        public int CustomVIEWPRImpactedSPCount { get; set; }

        [JsonProperty("Custom.VIEWPRManualUnitTestCount")]
        public int? CustomVIEWPRManualUnitTestCount { get; set; }

        [JsonProperty("Custom.VIEWPRUITestCount")]
        public int CustomVIEWPRUITestCount { get; set; }

        [JsonProperty("Custom.VIEWPRMaskedDBINTUsed")]
        public string CustomVIEWPRMaskedDBINTUsed { get; set; }

        [JsonProperty("Custom.VIEWPRAutomatedUnitTestCount")]
        public int CustomVIEWPRAutomatedUnitTestCount { get; set; }

        [JsonProperty("Custom.VIEWPRDBTestCount")]
        public int CustomVIEWPRDBTestCount { get; set; }

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
        public double CustomVIEWPRPlannedEffortHours { get; set; }

        [JsonProperty("Custom.VIEWPRActualEffortHours")]
        public double? CustomVIEWPRActualEffortHours { get; set; }

        [JsonProperty("Custom.VIEWPRPlannedvsActualGap")]
        public string CustomVIEWPRPlannedvsActualGap { get; set; }

        [JsonProperty("Custom.VIEWPRDetailsTab")]
        public string CustomVIEWPRDetailsTab { get; set; }

        [JsonProperty("Custom.VIEWPRCompatibleDataTypes")]
        public string CustomVIEWPRCompatibleDataTypes { get; set; }

        [JsonProperty("Custom.VIEWPRChecklistConfidence")]
        public double CustomVIEWPRChecklistConfidence { get; set; }

        [JsonProperty("Custom.DevelopmentStatus")]
        public string CustomDevelopmentStatus { get; set; }

        [JsonProperty("Custom.QAStatus")]
        public string CustomQAStatus { get; set; }

        [JsonProperty("Custom.VIEWPRImpactAssessmentCompleted")]
        public string CustomVIEWPRImpactAssessmentCompleted { get; set; }

        [JsonProperty("Custom.SignedOffBy")]
        public string CustomSignedOffBy { get; set; }

        [JsonProperty("WEF_439E363528544D1AA401EECA721CB80C_Kanban.Column")]
        public string WEF_439E363528544D1AA401EECA721CB80C_KanbanColumn { get; set; }

        [JsonProperty("WEF_439E363528544D1AA401EECA721CB80C_Kanban.Column.Done")]
        public bool WEF_439E363528544D1AA401EECA721CB80C_KanbanColumnDone { get; set; }

        [JsonProperty("Microsoft.VSTS.TCM.ReproSteps")]
        public string MicrosoftVSTSTCMReproSteps { get; set; }

        [JsonProperty("Microsoft.VSTS.CMMI.ImpactAssessmentHtml")]
        public string MicrosoftVSTSCMMIImpactAssessmentHtml { get; set; }

        [JsonProperty("System.Tags")]
        public string SystemTags { get; set; }
        public string IAStatus { get; set; }

        public string RootCauseStatus { get; set; }
        public string ProjectZeroStatus { get; set; }
        public string PRLifeCycleStatus { get; set; }
        public string StatusDiscrepancyStatus { get; set; }
        public string TestCaseGapeHTML { get; set; }

        public string TestCaseGapeStatus { get; set; }

        public string VTDMissingStatus{ get; set; }
        public string VLDBMissingStatus{ get; set; }
    }

    public class Values
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public string url { get; set; }
        public List<Relation> relations { get; set; }
        public List<TestByRelationField> testByRelationField { get; set; }

        public string? TlPrReviewAssignedTo
        {
            get; set;
        }
    }

    public class Relation
    {
        public string rel { get; set; }
        public string url { get; set; }
    }

    public class WorkItemReference
    {
        public int id { get; set; }
    }

    public class TestByRelationField
    {
        public string CivicaAgileTestLevel { get; set; }

        public string CivicaAgileTestPhase { get; set; }

        public string CustomTestType { get; set; }

        public string CustomAutomation { get; set; }

        public string SystemState { get; set; }

        public int TestId { get; set; }

        public string SystemAssignedTo { get; set; }

        public string? CivicaAgileTestLevelStatus { get; set; }

        public string? CivicaAgileTestPhaseStatus { get; set; }

        public string? CustomTestTypeStatus { get; set; }

        public string? CustomAutomationStatus { get; set; }
        public string? TestCaseUpdated { get; set; }
    }
}

using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using Microsoft.Extensions.Hosting;
using Mono.TextTemplating;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace ADOAnalyser.Repository
{
    public class AutoSpotCheck
    {
        #region variables
        private const string UserStory = "User Story";
        private const string InAnalysis = "01. Analysis & Estimate";
        private const string PRRaised = "11. PR Raised to DevOps";
        #endregion

        #region Rules
        public void CheckImpactAssessment(Fields workData)
        {
            string iaData = workData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = workData.SystemState ?? string.Empty;
            string devStatus = workData.CustomDevelopmentStatus ?? string.Empty;
            bool hasIA = ImpactAssessmentRegex(iaData);

            if (state == StateStatusEnum.Test.ToString() || state == StateStatusEnum.Resolved.ToString() || state == StateStatusEnum.Closed.ToString())
                workData.IAStatus = hasIA ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
            else if (state == StateStatusEnum.Active.ToString() && devStatus == InAnalysis)
                workData.IAStatus = ResultEnum.Pending.ToString();
            else
                workData.IAStatus = hasIA ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
        }

        public void CheckRootCause(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;

            if (workType == UserStory)
                fieldData.RootCauseStatus = ResultEnum.NA.ToString();
            else if (state == StateStatusEnum.Active.ToString())
                fieldData.RootCauseStatus = ResultEnum.Pending.ToString();
            else if (!string.IsNullOrWhiteSpace(rca))
                fieldData.RootCauseStatus = ResultEnum.Completed.ToString();
            else
                fieldData.RootCauseStatus = ResultEnum.Missing.ToString();
        }

        public void CheckProjectZero(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;

            if (workType == UserStory)
                fieldData.ProjectZeroStatus = ResultEnum.NA.ToString();
            else if (state == StateStatusEnum.Active.ToString())
                fieldData.ProjectZeroStatus = ResultEnum.Pending.ToString();
            else if ((state == StateStatusEnum.Test.ToString() || state == StateStatusEnum.Resolved.ToString() || state == StateStatusEnum.Closed.ToString()) && rca == "Code")
            {
                bool missing = string.IsNullOrEmpty(fieldData.CustomRootCauseAnalysisWhy1)
                || string.IsNullOrEmpty(fieldData.CustomRootCauseAnalysisWhy2)
                || string.IsNullOrEmpty(fieldData.CustomRootCauseAnalysisWhy3)
                || string.IsNullOrEmpty(fieldData.CustomRemediationOwner);

                fieldData.ProjectZeroStatus = missing ? ResultEnum.Missing.ToString() : ResultEnum.Completed.ToString();
            }
            else
                fieldData.ProjectZeroStatus = ResultEnum.NA.ToString();
        }

        public void CheckPRLifeCycle(Fields fieldData)
        {
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;

            if (state == StateStatusEnum.Active.ToString())
                fieldData.PRLifeCycleStatus = devStatus == PRRaised && CheckPRCheckList(fieldData)
                ? ResultEnum.Completed.ToString()
                : ResultEnum.Pending.ToString();
            else if (state == StateStatusEnum.Test.ToString() || state == StateStatusEnum.Closed.ToString() || state == StateStatusEnum.Resolved.ToString())
                fieldData.PRLifeCycleStatus = CheckPRCheckList(fieldData)
                ? ResultEnum.Completed.ToString()
                : ResultEnum.Missing.ToString();
        }

        public void CheckStatusDiscre(Fields fieldData)
        {
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            string qaStatus = fieldData.CustomQAStatus ?? string.Empty;

            bool isTestOrClosedOrResolved = state == StateStatusEnum.Test.ToString() || state == StateStatusEnum.Closed.ToString() || state == StateStatusEnum.Resolved.ToString();
            bool isTestActiveClosedResolved = isTestOrClosedOrResolved || state == StateStatusEnum.Active.ToString();

            bool discrepancy =
            (isTestActiveClosedResolved && string.IsNullOrWhiteSpace(devStatus)) ||
            (isTestOrClosedOrResolved && (string.IsNullOrWhiteSpace(qaStatus) || string.IsNullOrWhiteSpace(devStatus))) ||
            (isTestOrClosedOrResolved && !string.IsNullOrWhiteSpace(qaStatus) && devStatus != PRRaised);

            fieldData.StatusDiscrepancyStatus = discrepancy ? ResultEnum.Yes.ToString() : ResultEnum.No.ToString();
        }

        public void CheckTestCaseGape(Values values)
        {
            string state = values.fields.SystemState ?? string.Empty;

            if (state == StateStatusEnum.Active.ToString())
            {
                values.fields.TestCaseGapeStatus = ResultEnum.Pending.ToString();
            }
            else if (state == StateStatusEnum.Test.ToString() ||
                     state == StateStatusEnum.Closed.ToString() ||
                     state == StateStatusEnum.Resolved.ToString())
            {
                if (values.testByRelationField == null || !values.testByRelationField.Any())
                {
                    values.fields.TestCaseGapeStatus = ResultEnum.Missing.ToString();
                }
                else
                {
                    values.fields.TestCaseGapeStatus = ResultEnum.Missing.ToString();

                    foreach (var testCase in values.testByRelationField)
                    {
                        bool allFieldsPresent = true;

                        bool isMissingType = IsFieldMissing(testCase.CustomTestType, out var typeStatus);
                        testCase.CustomTestTypeStatus = typeStatus;
                        allFieldsPresent &= !isMissingType;

                        bool isMissingLevel = IsFieldMissing(testCase.CivicaAgileTestLevel, out var levelStatus);
                        testCase.CivicaAgileTestLevelStatus = levelStatus;
                        allFieldsPresent &= !isMissingLevel;

                        bool isMissingPhase = IsFieldMissing(testCase.CivicaAgileTestPhase, out var phaseStatus);
                        testCase.CivicaAgileTestPhaseStatus = phaseStatus;
                        allFieldsPresent &= !isMissingPhase;

                        bool isMissingAutomation = IsFieldMissing(testCase.CustomAutomation, out var automationStatus);
                        testCase.CustomAutomationStatus = automationStatus;
                        allFieldsPresent &= !isMissingAutomation;

                        if (allFieldsPresent)
                        {
                            values.fields.TestCaseGapeStatus = ResultEnum.Updated.ToString();
                            testCase.TestCaseUpdated = ResultEnum.Updated.ToString();
                        }
                    }
                }
            }

            values.fields.TestCaseGapeHTML = HTMLString(values);
        }

        public void CheckVTDRequired(Fields fieldData)
        {
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            bool hasVTD = fieldData.CivicaAgileVIEWTargetDate.HasValue;

            bool isFinalState = state == StateStatusEnum.Closed.ToString() ||
                                state == StateStatusEnum.Resolved.ToString() ||
                                state == StateStatusEnum.Test.ToString();

            if (isFinalState)
            {
                fieldData.VTDMissingStatus = hasVTD ? ResultEnum.Updated.ToString() : ResultEnum.Missing.ToString();
            }
            else if (state == StateStatusEnum.Active.ToString())
            {
                fieldData.VTDMissingStatus = devStatus == InAnalysis
                    ? ResultEnum.Pending.ToString()
                    : (hasVTD ? ResultEnum.Updated.ToString() : ResultEnum.Missing.ToString());
            }
        }

        public void CheckVLDBRequired(Fields fieldData)
        {
            string vtdStatus = fieldData.VTDMissingStatus;
            bool hasVLDB = fieldData.CustomVIEWLanDeskBreakDate.HasValue;

            fieldData.VLDBMissingStatus = vtdStatus switch
            {
                var s when s == ResultEnum.Pending.ToString() => ResultEnum.Pending.ToString(),
                var s when s == ResultEnum.Missing.ToString() => ResultEnum.Missing.ToString(),
                _ => hasVLDB ? ResultEnum.Updated.ToString() : ResultEnum.Missing.ToString()
            };
        }

        private bool CheckPRCheckList(Fields fieldData)
        {
            return !(string.IsNullOrEmpty(fieldData.CustomVIEWPRCompletedDemo)
            || string.IsNullOrEmpty(fieldData.CustomSignedOffBy)
            || fieldData.CustomVIEWPRManualUnitTestCount == null
            || fieldData.CustomVIEWPRImpactAnalysisHours == null
            || fieldData.CustomVIEWPRActualEffortHours == null);
        }

        #endregion

        #region MissingCount
        public int MissingImpactAssessmentCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.IAStatus == ResultEnum.Missing.ToString());

        public int MissingRootCauseCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.RootCauseStatus == ResultEnum.Missing.ToString());

        public int MissingProjectZeroCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.ProjectZeroStatus == ResultEnum.Missing.ToString());

        public int MissingPRLifeCycleCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.PRLifeCycleStatus == ResultEnum.Missing.ToString());

        public int MissingStatusDiscreCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.StatusDiscrepancyStatus == ResultEnum.Yes.ToString());

        public int MissingTestCaseGapeCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.TestCaseGapeStatus == ResultEnum.Missing.ToString());

        public int MissingVLDBCount(WorkItemModel workData) =>
            workData.value.Count(a => a.fields.VLDBMissingStatus == ResultEnum.Missing.ToString());

        public int MissingVTDCount(WorkItemModel workData) =>
        workData.value.Count(a => a.fields.VTDMissingStatus == ResultEnum.Missing.ToString());

        #endregion

        #region Others
        public async Task CheckMissingDataAsync(WorkItemModel workData)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(workData.value, item =>
                {
                    CheckImpactAssessment(item.fields);
                    CheckRootCause(item.fields);
                    CheckProjectZero(item.fields);
                    CheckPRLifeCycle(item.fields);
                    CheckStatusDiscre(item.fields);
                    CheckTestCaseGape(item);
                    CheckVTDRequired(item.fields);
                    CheckVLDBRequired(item.fields);
                });
            });
        }

        public void CheckMissingData(WorkItemModel workData)
        {
            Parallel.ForEach(workData.value, item =>
            {
                CheckImpactAssessment(item.fields);
                CheckRootCause(item.fields);
                CheckProjectZero(item.fields);
                CheckPRLifeCycle(item.fields);
                CheckStatusDiscre(item.fields);
                CheckTestCaseGape(item);
                CheckVTDRequired(item.fields);
                CheckVLDBRequired(item.fields);
            });
        }

        public bool ImpactAssessmentRegex(string data)
        {
            string pattern = @"https?://[^""']*Assessment[^""']*\.xlsx";
            return Regex.IsMatch(data, pattern, RegexOptions.IgnoreCase);
        }

        private bool IsFieldMissing(string fieldValue, out string? status)
        {
            if (string.IsNullOrEmpty(fieldValue))
            {
                status = ResultEnum.Missing.ToString();
                return true;
            }
            status = null;
            return false;
        }

        private string HTMLString(Values values)
        {
            var status = values.fields.TestCaseGapeStatus;
            var testCases = values.testByRelationField;

            if (testCases == null || !testCases.Any())
                return $"<span class=\"{ResultEnum.Missing}\">No Test case Attached</span>";

            if (status == ResultEnum.Pending.ToString())
                return $"<span class=\"{ResultEnum.Pending}\">In Progress</span>";

            var workItemNumber = Convert.ToString(values.id);
            var testBuilder = new StringBuilder();
            int countUpdated = testCases.Count(tc => tc.TestCaseUpdated == ResultEnum.Updated.ToString());
            int totalCount = testCases.Count;
            int missingCount = testCases.Count(tc =>
            tc.CustomTestTypeStatus == ResultEnum.Missing.ToString() ||
            tc.CivicaAgileTestLevelStatus == ResultEnum.Missing.ToString() ||
            tc.CivicaAgileTestPhaseStatus == ResultEnum.Missing.ToString() ||
            tc.CustomAutomationStatus == ResultEnum.Missing.ToString());

            if (missingCount > 0)
            {
                testBuilder.AppendFormat(
                "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type=Missing\" target=\"_blank\" style=\"text-decoration:none\">" +
                "<span class=\"Missing\">Missing Details</span><br></a>",
                workItemNumber);
            }

            if (countUpdated > 0)
            {
                string msg = countUpdated == totalCount ? "All fields updated." : "Updated Details";
                testBuilder.AppendFormat(
                "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type=Updated\" target=\"_blank\" style=\"text-decoration:none\">" +
                "<span class=\"Updated\">{1}</span><br></a>",
                workItemNumber, msg);
            }

            return testBuilder.ToString();
        }

        public void SetCountForMissing(WorkItemModel workData)
        {
            workData.missingIACount = MissingImpactAssessmentCount(workData);
            workData.missingRootCauseCount = MissingRootCauseCount(workData);
            workData.missingProjectZeroCount = MissingProjectZeroCount(workData);
            workData.missingPRLifeCycleCount = MissingPRLifeCycleCount(workData);
            workData.missingStatusDiscreCount = MissingStatusDiscreCount(workData);
            workData.missingTestCaseCount = MissingTestCaseGapeCount(workData);
            workData.missingVTDCount = MissingVTDCount(workData);
            workData.missingVLDBCount = MissingVLDBCount(workData);
        }
        #endregion
    }
}
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
        private string UserStory = "User Story";

        private string InAnalysis = "01. Analysis & Estimate";

        private string PRRaised = "11. PR Raised to DevOps";

        public void CheckImpactAssessment(Fields workData)
        {
            string iaData = workData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = workData.SystemState;
            string devStatus = workData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Resolved.ToString())
                || state.Equals(StateStatusEnum.Closed.ToString()))
            {
                workData.IAStatus = ImpactAssessmentRegex(iaData) ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
            }
            if (state.Equals(StateStatusEnum.Active.ToString()) && devStatus.Equals(InAnalysis))
            {
                workData.IAStatus = ResultEnum.Pending.ToString();
            }
            else
            {
                workData.IAStatus = ImpactAssessmentRegex(iaData) ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
            }
        }

        public bool ImpactAssessmentRegex(string data)
        {
            string pattern = @"https?://[^""']*Assessment[^""']*\.xlsx";
            return Regex.IsMatch(data, pattern, RegexOptions.IgnoreCase);
        }

        public int MissingImpactAssessmentCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.IAStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckRootCause(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;
            if (!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString()))
                && string.IsNullOrWhiteSpace(rca))
            {
                fieldData.RootCauseStatus = ResultEnum.Missing.ToString();
            }
            if (!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.New.ToString()) || state.Equals(StateStatusEnum.Active.ToString())))
            {
                fieldData.RootCauseStatus = ResultEnum.Pending.ToString();
            }
            if (!workType.Equals(UserStory) && !string.IsNullOrWhiteSpace(rca))
            {
                fieldData.RootCauseStatus = ResultEnum.Completed.ToString();
            }
            if (workType.Equals(UserStory))
            {
                fieldData.RootCauseStatus = ResultEnum.NA.ToString();
            }
        }

        public int MissingRootCauseCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.RootCauseStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckProjectZero(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            if (workType.Equals(UserStory))
            {
                fieldData.ProjectZeroStatus = ResultEnum.NA.ToString();
            }
            if (!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Resolved.ToString()) || state.Equals(StateStatusEnum.Closed.ToString())))
            {
                if (rca.Equals("Code"))
                {
                    string why1 = fieldData.CustomRootCauseAnalysisWhy1;
                    string why2 = fieldData.CustomRootCauseAnalysisWhy2;
                    string why3 = fieldData.CustomRootCauseAnalysisWhy3;
                    string owner = fieldData.CustomRemediationOwner;
                    if (string.IsNullOrEmpty(why1) || string.IsNullOrEmpty(why2) || string.IsNullOrEmpty(why3) || string.IsNullOrEmpty(owner))
                    {
                        fieldData.ProjectZeroStatus = ResultEnum.Missing.ToString();
                    }
                    else
                    {
                        fieldData.ProjectZeroStatus = ResultEnum.Completed.ToString();
                    }
                }
                else
                {
                    fieldData.ProjectZeroStatus = ResultEnum.NA.ToString();
                }
                
            }
            if (!workType.Equals(UserStory) && state.Equals(StateStatusEnum.Active.ToString()))
            {
                fieldData.ProjectZeroStatus = ResultEnum.Pending.ToString();
            }
        }

        public int MissingProjectZeroCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.ProjectZeroStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckPRLifeCycle(Fields fieldData)
        {
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString()) || state.Equals(StateStatusEnum.Resolved.ToString()))
            {
                fieldData.PRLifeCycleStatus = !CheckPRCheckList(fieldData) ? ResultEnum.Missing.ToString() : ResultEnum.Completed.ToString();
            }
            if (state.Equals(StateStatusEnum.Active.ToString()))
            {
                if (!devStatus.Equals(PRRaised))
                {
                    fieldData.PRLifeCycleStatus = ResultEnum.Pending.ToString();
                }
                else
                {
                    fieldData.PRLifeCycleStatus = !CheckPRCheckList(fieldData) ? ResultEnum.Missing.ToString() : ResultEnum.Completed.ToString();
                }
            }
            if (state.Equals(StateStatusEnum.New.ToString()))
            {
                fieldData.PRLifeCycleStatus = ResultEnum.Pending.ToString();
            }
        }

        public int MissingPRLifeCycleCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.PRLifeCycleStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckStatusDiscre(Fields fieldData)
        {
            string iaData = fieldData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            string qaStatus = fieldData.CustomQAStatus ?? string.Empty;
            if ((state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Active.ToString()) || state.Equals(StateStatusEnum.Closed.ToString()))
                && string.IsNullOrWhiteSpace(devStatus))
            {
                fieldData.StatusDiscrepancyStatus = ResultEnum.Yes.ToString();
            }
            if ((state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString()))
                && (string.IsNullOrWhiteSpace(qaStatus) || string.IsNullOrWhiteSpace(devStatus)))
            {
                fieldData.StatusDiscrepancyStatus = ResultEnum.Yes.ToString();
            }
            if ((state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString())) &&
                !string.IsNullOrWhiteSpace(qaStatus) && !devStatus.Equals(PRRaised))
            {
                fieldData.StatusDiscrepancyStatus = ResultEnum.Yes.ToString();
            }
            string current = fieldData.StatusDiscrepancyStatus;
            fieldData.StatusDiscrepancyStatus = !string.IsNullOrWhiteSpace(current) ? ResultEnum.Yes.ToString() : ResultEnum.No.ToString();
        }

        public int MissingStatusDiscreCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.StatusDiscrepancyStatus.Equals(ResultEnum.Yes.ToString())).Count();
        }

        public void CheckTestCaseGape(Values values)
        {
            string state = values.fields.SystemState ?? string.Empty;

            if (state == StateStatusEnum.Active.ToString())
            {
                values.fields.TestCaseGapeStatus = ResultEnum.Pending.ToString();
            }
            else if (state is var s &&
            (s == StateStatusEnum.Test.ToString() ||
            s == StateStatusEnum.Closed.ToString() ||
            s == StateStatusEnum.Resolved.ToString()))
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

                        if (IsFieldMissing(testCase.CustomTestType, out var testTypeStatus))
                        {
                            testCase.CustomTestTypeStatus = testTypeStatus;
                            allFieldsPresent = false;
                        }

                        if (IsFieldMissing(testCase.CivicaAgileTestLevel, out var testLevelStatus))
                        {
                            testCase.CivicaAgileTestLevelStatus = testLevelStatus;
                            allFieldsPresent = false;
                        }

                        if (IsFieldMissing(testCase.CivicaAgileTestPhase, out var testPhaseStatus))
                        {
                            testCase.CivicaAgileTestPhaseStatus = testPhaseStatus;
                            allFieldsPresent = false;
                        }

                        if (IsFieldMissing(testCase.CustomAutomation, out var automationStatus))
                        {
                            testCase.CustomAutomationStatus = automationStatus;
                            allFieldsPresent = false;
                        }

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

        public int MissingTestCaseGapeCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.TestCaseGapeStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckVTDRequired(Fields fieldData)
        {
            DateTime? vtdDate = fieldData.CivicaAgileVIEWTargetDate;
            string vtd = vtdDate.HasValue ? vtdDate.Value.ToString() : string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Closed.ToString()) || state.Equals(StateStatusEnum.Resolved.ToString()) 
                || state.Equals(StateStatusEnum.Test.ToString()))
            {
                fieldData.VTDMissingStatus = string.IsNullOrWhiteSpace(vtd) ? ResultEnum.Missing.ToString() : ResultEnum.Updated.ToString();
            }
            if (state.Equals(StateStatusEnum.Active.ToString()))
            {
                if (!devStatus.Equals(InAnalysis))
                {
                    fieldData.VTDMissingStatus = string.IsNullOrWhiteSpace(vtd) ? ResultEnum.Missing.ToString() : ResultEnum.Updated.ToString();
                }
                else
                {
                    fieldData.VTDMissingStatus = ResultEnum.Pending.ToString();
                }
            }
        }

        public int MissingVTDCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.VTDMissingStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckVLDBRequired(Fields fieldData)
        {
            string VTDStatus = fieldData.VTDMissingStatus;
            DateTime? vldbDate = fieldData.CustomVIEWLanDeskBreakDate;
            string vldb = vldbDate.HasValue ? vldbDate.Value.ToString() : string.Empty;
            if (VTDStatus.Equals(ResultEnum.Pending))
            {
                fieldData.VLDBMissingStatus = ResultEnum.Pending.ToString();
            }
            if (VTDStatus.Equals(ResultEnum.Missing))
            {
                fieldData.VLDBMissingStatus = ResultEnum.Missing.ToString();
            }
            else
            {
                fieldData.VLDBMissingStatus = string.IsNullOrWhiteSpace(vldb) ? ResultEnum.Missing.ToString() : ResultEnum.Updated.ToString();
            }
        }

        public int MissingVLDBCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.VLDBMissingStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        private bool CheckPRCheckList(Fields fieldData)
        {
            int? manualUnitTestCount = fieldData.CustomVIEWPRManualUnitTestCount;
            string demo = fieldData.CustomVIEWPRCompletedDemo?? string.Empty;
            double? iahours = fieldData.CustomVIEWPRImpactAnalysisHours;
            double? prActualEffortHour = fieldData.CustomVIEWPRActualEffortHours;
            string signedOff = fieldData.CustomSignedOffBy?? string.Empty;
            if(string.IsNullOrEmpty(demo) || string.IsNullOrEmpty(signedOff) || manualUnitTestCount == null || iahours == null || prActualEffortHour == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

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
            {
                return $"<span class=\"{ResultEnum.Missing}\">No Test case Attached</span>";
            }

            if (status == ResultEnum.Pending.ToString())
            {
                return $"<span class=\"{ResultEnum.Pending}\">In Progress</span>";
            }

            var workItemNumber = Convert.ToString(values.id);
            var testBuilder = new StringBuilder();
            int countUpdated = testCases.Count(tc => tc.TestCaseUpdated == ResultEnum.Updated.ToString());
            int totalCount = testCases.Count;
            int missingCount = testCases.Count(tc => tc.CustomTestTypeStatus == ResultEnum.Missing.ToString()
                                                || tc.CivicaAgileTestLevelStatus == ResultEnum.Missing.ToString()
                                                || tc.CivicaAgileTestPhaseStatus == ResultEnum.Missing.ToString()
                                                || tc.CustomAutomationStatus == ResultEnum.Missing.ToString());
            if (missingCount > 0)
            {
                string msg = "Missing Details";// "<span style=\"font-weight: bold\">Test case information: <span>";
                testBuilder.AppendFormat(
                "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type=Missing\" target=\"_blank\" style=\"text-decoration:none\"\" ><span class=\"Missing\">{1}</span><br>",
                workItemNumber, msg);
            }

            if (countUpdated > 0)
            {
                string msg = countUpdated == totalCount ? "All fields updated." : $"Updated Details";
                testBuilder.AppendFormat(
                "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type=Updated\" target=\"_blank\" style=\"text-decoration:none\"\"><span class=\"Updated\">{1}</span><br>",
                workItemNumber, msg);
            }

            return testBuilder.ToString();
        }
    }
}

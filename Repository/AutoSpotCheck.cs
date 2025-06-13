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
                    foreach (var testCase in values.testByRelationField)
                    {
                        string customTestType = testCase.CustomTestType == null ? string.Empty : testCase.CustomTestType;
                        string customtestLevel = testCase.CivicaAgileTestLevel == null ? string.Empty : testCase.CivicaAgileTestLevel;
                        string customtestPhase = testCase.CivicaAgileTestPhase == null ? string.Empty : testCase.CivicaAgileTestPhase;
                        string customAutomation = testCase.CustomAutomation == null ? string.Empty : testCase.CustomAutomation;

                        values.fields.TestCaseGapeStatus = ResultEnum.Missing.ToString();

                        if (string.IsNullOrEmpty(customTestType))
                        {
                            testCase.CustomTestTypeStatus = ResultEnum.Missing.ToString();
                        }
                        if (string.IsNullOrEmpty(customtestLevel))
                        {
                            testCase.CivicaAgileTestLevelStatus = ResultEnum.Missing.ToString();
                        }
                        if (string.IsNullOrEmpty(customtestPhase))
                        {
                            testCase.CivicaAgileTestPhaseStatus = ResultEnum.Missing.ToString();
                        }
                        if (string.IsNullOrEmpty(customAutomation))
                        {
                            testCase.CustomAutomationStatus = ResultEnum.Missing.ToString();
                        }

                        if (!string.IsNullOrEmpty(customTestType) &&
                        !string.IsNullOrEmpty(customtestLevel) &&
                        !string.IsNullOrEmpty(customtestPhase) &&
                        !string.IsNullOrEmpty(customAutomation))
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


        private string HTMLString(Values values)
        {
            if (values.testByRelationField == null || !values.testByRelationField.Any()) 
            {
                string status = ResultEnum.Missing.ToString();
                string displayText = status switch
                {
                    var s when s == ResultEnum.Missing.ToString() => "No Test case Attached",
                    var s when s == ResultEnum.Pending.ToString() => "In Progress",
                    _ => status
                };
                return $"<span class=\"{status}\">{displayText}</span>";
            }
            else
            {
                int countUpdated = values.testByRelationField.Where(a =>  a.TestCaseUpdated != null && a.TestCaseUpdated.Equals(ResultEnum.Updated.ToString())).Count();
                int TotalCount = values.testByRelationField.Count();
                if (values.fields.TestCaseGapeStatus.Equals(ResultEnum.Pending.ToString()))
                {
                    string status = ResultEnum.Pending.ToString();
                    string displayText = status switch
                    {
                        var s when s == ResultEnum.Missing.ToString() => "No Test case Attached",
                        var s when s == ResultEnum.Pending.ToString() => "In Progress",
                        _ => status
                    };
                    return $"<span class=\"{status}\">{displayText}</span>";
                }
                else
                {
                    string workItemNumber = Convert.ToString(values.id);
                    var testBuilder = new StringBuilder();
                    string cssClass = ResultEnum.Missing.ToString();
                    int countCustomTestType = values.testByRelationField.Where(a => a.CustomTestTypeStatus != null).Count();
                    int CivicaAgileTestLevel = values.testByRelationField.Where(a => a.CivicaAgileTestLevelStatus != null).Count(); 
                    int CivicaAgileTestPhase = values.testByRelationField.Where(a => a.CivicaAgileTestPhaseStatus != null).Count(); 
                    int CustomAutomationStatus = values.testByRelationField.Where(a => a.CustomAutomationStatus != null).Count();
                    if (countCustomTestType > 0) {
                        string msg = string.Format("Test Type missing in {0} test case(s).", countCustomTestType);
                        testBuilder.AppendFormat(
                         "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\" style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         cssClass,
                         msg,
                         "TestType"
                        );
                    }
                    if (CivicaAgileTestLevel > 0)
                    {
                        string msg = string.Format("Test Level missing in {0} test case(s).", CivicaAgileTestLevel);
                        testBuilder.AppendFormat(
                          "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\"  style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         cssClass,
                         msg,
                         "TestLevel"
                        );
                    }
                    if (CivicaAgileTestPhase > 0)
                    {
                        string msg = string.Format("Test Phase missing in {0} test case(s).", CivicaAgileTestPhase);
                        testBuilder.AppendFormat(
                          "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\"  style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         cssClass,
                         msg,
                         "TestPhase"
                        );
                    }
                    if (CustomAutomationStatus > 0)
                    {
                        string msg = string.Format("Automation Status missing in {0} test case(s).", CustomAutomationStatus);
                        testBuilder.AppendFormat(
                          "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\"  style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         cssClass,
                         msg,
                         "Automation"
                        );
                    }
                    if (countUpdated > 0 && countUpdated != TotalCount)
                    {
                        string msg = string.Format("Updated Test Case is {0}.", countUpdated);
                        testBuilder.AppendFormat(
                         "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\"  style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         "Updated",
                         msg,
                         "Updated"
                        );
                    }
                    if(countUpdated > 0 && countUpdated == TotalCount)
                    {
                        string msg = string.Format("All Field Updated.");
                        testBuilder.AppendFormat(
                          "<a href=\"/TestedByRelationGrid/Index?workItemNumber={0}&type={3}\"  style=\"text-decoration:none\" target=\"_blank\"><span class=\"{1}\">{2}</span><br>",
                         workItemNumber,
                         "Updated",
                         msg,
                         "Updated"
                        );
                    }
                    return testBuilder.ToString();
                }
            } 
        }

    }
}

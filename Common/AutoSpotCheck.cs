using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using Mono.TextTemplating;
using System.Text.RegularExpressions;

namespace ADOAnalyser.Common
{
    public class AutoSpotCheck
    {
        public void CheckImpactAssessment(Fields workData)
        {
            string iaData = workData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = workData.SystemState;
            string devStatus = workData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Test.ToString())
                || state.Equals(StateStatusEnum.Test.ToString()) || devStatus.Equals(DevelopmentStatusEnum.InDevelopment.ToString()))
            {
                workData.IAStatus = ImpactAssessmentRegex(iaData) ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
            }
            else
            {
                workData.IAStatus = ResultEnum.Pending.ToString();
            }
        }

        private bool ImpactAssessmentRegex(string data)
        {
            string pattern = @"https?://[^""']*Assessment[^""']*\.xlsx";
            return Regex.IsMatch(data, pattern, RegexOptions.IgnoreCase);
        }

        private int MissingImpactAssessmentCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.IAStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckRootCause(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string rcaDetail = fieldData.CustomRootCauseAnalysisDetail ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && state.Equals(StateStatusEnum.Test.ToString()) && (string.IsNullOrWhiteSpace(rca) || string.IsNullOrWhiteSpace(rcaDetail)))
            {
                fieldData.RootCauseStatus = ResultEnum.Missing.ToString();
            }
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && !string.IsNullOrWhiteSpace(rca) && !string.IsNullOrWhiteSpace(rcaDetail))
            {
                fieldData.RootCauseStatus = ResultEnum.Completed.ToString();
            }
        }

        private int MissingRootCauseCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.RootCauseStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckProjectZero(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string rcaDetail = fieldData.CustomRootCauseAnalysisDetail ?? string.Empty;
            string workType = fieldData.SystemWorkItemType ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()))
            {
                fieldData.ProjectZeroStatus = ResultEnum.NotApplicable.ToString();
            }
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && state.Equals(StateStatusEnum.Test.ToString()) &&
               rca.Equals("Code") && !string.IsNullOrEmpty(rcaDetail))
            {
                string why1 = fieldData.CustomRootCauseAnalysisWhy1;
                string why2 = fieldData.CustomRootCauseAnalysisWhy2;
                string why3 = fieldData.CustomRootCauseAnalysisWhy3;
                if (string.IsNullOrEmpty(why1) || string.IsNullOrEmpty(why2) || string.IsNullOrEmpty(why3))
                {
                    fieldData.ProjectZeroStatus = ResultEnum.Missing.ToString();
                }
                else
                {
                    fieldData.ProjectZeroStatus = ResultEnum.Completed.ToString();
                }
            }
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && !state.Equals(StateStatusEnum.Test.ToString()))
            {
                fieldData.ProjectZeroStatus = ResultEnum.Pending.ToString();
            }
        }

        private int MissingProjectZeroCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.ProjectZeroStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckPRLifeCycle(Fields fieldData)
        {
            string rca = fieldData.CivicaAgileRootCauseAnalysis ?? string.Empty;
            string rcaDetail = fieldData.CustomRootCauseAnalysisDetail ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rca) || string.IsNullOrWhiteSpace(rcaDetail))
            {
                fieldData.PRLifeCycleStatus = ResultEnum.Missing.ToString();
            }
            else
            {
                fieldData.PRLifeCycleStatus = ResultEnum.Completed.ToString();
            }
        }

        private int MissingPRLifeCycleCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.PRLifeCycleStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckStatusDiscre(Fields fieldData)
        {
            string iaData = fieldData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            string qaStatus = fieldData.CustomQAStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) && string.IsNullOrWhiteSpace(qaStatus))
            {
                fieldData.StatusDiscrepancyStatus = ResultEnum.Yes.ToString();
            }
            if ((state.Equals(StateStatusEnum.Active.ToString()) || state.Equals(StateStatusEnum.Test.ToString())) && string.IsNullOrWhiteSpace(devStatus))
            {
                fieldData.StatusDiscrepancyStatus = ResultEnum.Yes.ToString();
            }
            string current = fieldData.StatusDiscrepancyStatus;
            fieldData.StatusDiscrepancyStatus = !string.IsNullOrWhiteSpace(current) ? ResultEnum.Yes.ToString() : ResultEnum.No.ToString();
        }

        private int MissingStatusDiscreCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.StatusDiscrepancyStatus.Equals(ResultEnum.Yes.ToString())).Count();
        }

        public void CheckTestCaseGape(Fields fieldData)
        {
            //string rca = fieldData.CivicaAgileRootCauseAnalysis;
            //string rcaDetail = fieldData.CustomRootCauseAnalysisDetail;
            //if (string.IsNullOrWhiteSpace(rca) || string.IsNullOrWhiteSpace(rcaDetail))
            //{
            //    fieldData.TestCaseGapeStatus = ResultEnum.Missing.ToString();
            //}
            //else
            //{
            //    fieldData.TestCaseGapeStatus = ResultEnum.Completed.ToString();
            //}
        }

        private int MissingTestCaseGapeCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.TestCaseGapeStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }


        private void IsVTDRequired(Fields fieldData)
        {
            string vtd = fieldData.CivicaAgileVIEWTargetDate.Date.ToString();
            string state = fieldData.SystemState ?? string.Empty;
            string devStatus = fieldData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Active.ToString()) && devStatus.Equals(DevelopmentStatusEnum.InDevelopment.ToString()) && string.IsNullOrWhiteSpace(vtd))
            {
                fieldData.VTDMissingStatus = ResultEnum.Missing.ToString();
            }
            string current = fieldData.StatusDiscrepancyStatus ?? string.Empty;
            fieldData.StatusDiscrepancyStatus = !string.IsNullOrWhiteSpace(current) ? ResultEnum.Yes.ToString() : ResultEnum.No.ToString();
        }

        private void IsVLBDRequired(Fields fieldData)
        {
            string vtd = fieldData.CivicaAgileVIEWTargetDate.Date.ToString();
            string vldb = fieldData.CustomVIEWLanDeskBreakDate.Date.ToString();
            if (!string.IsNullOrWhiteSpace(vtd) && string.IsNullOrWhiteSpace(vldb))
            {
                fieldData.VLBDMissingStatus = ResultEnum.Missing.ToString();
            }
        }
    }
}

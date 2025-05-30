﻿using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using Mono.TextTemplating;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ADOAnalyser.Common
{
    public class AutoSpotCheck
    {
        public void CheckImpactAssessment(Fields workData)
        {
            string iaData = workData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = workData.SystemState;
            string devStatus = workData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Active.ToString())
                || state.Equals(StateStatusEnum.Closed.ToString()) || devStatus.Equals(DevelopmentStatusEnum.InDevelopment.ToString()))
            {
                workData.IAStatus = ImpactAssessmentRegex(iaData) ? ResultEnum.Attached.ToString() : ResultEnum.Missing.ToString();
            }
            else
            {
                workData.IAStatus = ResultEnum.Pending.ToString();
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
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && state.Equals(StateStatusEnum.Test.ToString()) && (string.IsNullOrWhiteSpace(rca)))
            {
                fieldData.RootCauseStatus = ResultEnum.Missing.ToString();
            }
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && !string.IsNullOrWhiteSpace(rca))
            {
                fieldData.RootCauseStatus = ResultEnum.Completed.ToString();
            }
            if (workType.Equals(WorkTypeEnum.UserStory.ToString()))
            {
                fieldData.RootCauseStatus = ResultEnum.NotApplicable.ToString();
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
            if (workType.Equals(WorkTypeEnum.UserStory.ToString()))
            {
                fieldData.ProjectZeroStatus = ResultEnum.NotApplicable.ToString();
            }
            if (!workType.Equals(WorkTypeEnum.UserStory.ToString()) && (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString())) &&
               rca.Equals("Code"))
            {
                string why1 = fieldData.CustomRootCauseAnalysisWhy1;
                string why2 = fieldData.CustomRootCauseAnalysisWhy2;
                string why3 = fieldData.CustomRootCauseAnalysisWhy3;
                string owner = fieldData.CustomRootCauseAnalysisRemediation;
                if (string.IsNullOrEmpty(why1) || string.IsNullOrEmpty(why2) || string.IsNullOrEmpty(why3) || string.IsNullOrEmpty(owner))
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

        public int MissingProjectZeroCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.ProjectZeroStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckPRLifeCycle(Fields fieldData)
        {
            //string state = fieldData.SystemState ?? string.Empty;
            //int manualUnitTestCount = fieldData.CustomVIEWPRManualUnitTestCount;
            //int demo = fieldData.CustomVIEWPRManualUnitTestCount;
            //double iahours = fieldData.CustomVIEWPRImpactAnalysisHours;
            //double prEffortHour = fieldData.CustomVIEWPRActualEffortHours;
            //string signedOff = fieldData.CustomSignedOffBy;

            //if (state.Equals(StateStatusEnum.Test.ToString()))
            //{
            //    fieldData.PRLifeCycleStatus = ResultEnum.Missing.ToString();
            //}
            //else
            //{
            //    fieldData.PRLifeCycleStatus = ResultEnum.Completed.ToString();
            //}
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

        public int MissingStatusDiscreCount(WorkItemModel workData)
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

        public int MissingTestCaseGapeCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.TestCaseGapeStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }


        public void CheckVTDRequired(Fields fieldData)
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

        public int MissingVTDCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.VTDMissingStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }

        public void CheckVLDBRequired(Fields fieldData)
        {
            string vtd = fieldData.CivicaAgileVIEWTargetDate.Date.ToString();
            string vldb = fieldData.CustomVIEWLanDeskBreakDate.Date.ToString();
            string state = fieldData.SystemState ?? string.Empty;
            if ((state.Equals(StateStatusEnum.Active.ToString()) || state.Equals(StateStatusEnum.Test.ToString())) && !string.IsNullOrWhiteSpace(vtd) && string.IsNullOrWhiteSpace(vldb))
            {
                fieldData.VLDBMissingStatus = ResultEnum.Missing.ToString();
            }
        }

        public int MissingVLDBCount(WorkItemModel workData)
        {
            return workData.value.Where(a => a.fields.VLDBMissingStatus.Equals(ResultEnum.Missing.ToString())).Count();
        }
    }
}

﻿using ADOAnalyser.Enum;
using ADOAnalyser.Models;
using Mono.TextTemplating;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ADOAnalyser.Common
{
    public class AutoSpotCheck
    {
        private string UserStory = "User Story";

        private string InDevelopment = "03. In Development";

        private string InAnalysis = "01. Analysis & Estimate";

        private string PRRaised = "11. PR Raised to DevOps";

        public void CheckImpactAssessment(Fields workData)
        {
            string iaData = workData.MicrosoftVSTSCMMIImpactAssessmentHtml ?? string.Empty;
            string state = workData.SystemState;
            string devStatus = workData.CustomDevelopmentStatus ?? string.Empty;
            if (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Active.ToString())
                || state.Equals(StateStatusEnum.Closed.ToString()) || devStatus.Equals(InDevelopment))
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
            if (!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString())) && (string.IsNullOrWhiteSpace(rca)))
            {
                fieldData.RootCauseStatus = ResultEnum.Missing.ToString();
            }
            if(!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.New.ToString()) || state.Equals(StateStatusEnum.Active.ToString())))
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
            if (!workType.Equals(UserStory) && (state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString())) &&
               rca.Equals("Code"))
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
            if (!workType.Equals(UserStory) && !state.Equals(StateStatusEnum.Test.ToString()))
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

            if ((state.Equals(StateStatusEnum.Test.ToString()) || state.Equals(StateStatusEnum.Closed.ToString()) || state.Equals(StateStatusEnum.Resolved.ToString())))
            {
                fieldData.PRLifeCycleStatus = !CheckPRCheckList(fieldData) ? ResultEnum.Missing.ToString() : ResultEnum.Completed.ToString();
            }
            if(state.Equals(StateStatusEnum.Active.ToString()))
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
            if(state.Equals(StateStatusEnum.New.ToString()))
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
                (!string.IsNullOrWhiteSpace(qaStatus) && !devStatus.Equals(PRRaised)))
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
            fieldData.TestCaseGapeStatus = string.Empty;
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
            if (state.Equals(StateStatusEnum.Active.ToString()) && !devStatus.Equals(InAnalysis))
            {
                fieldData.VTDMissingStatus = string.IsNullOrWhiteSpace(vtd) ? ResultEnum.Missing.ToString() : ResultEnum.Updated.ToString();
            }
            else
            {
                fieldData.VTDMissingStatus = ResultEnum.Pending.ToString();
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
            if (VTDStatus.Equals(ResultEnum.Missing) || VTDStatus.Equals(ResultEnum.Pending))
            {
                fieldData.VLDBMissingStatus = ResultEnum.Pending.ToString();
            }
            else
            {
                fieldData.VLDBMissingStatus = string.IsNullOrWhiteSpace(vldb) ? ResultEnum.Pending.ToString() :  ResultEnum.Updated.ToString();
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
    }
}

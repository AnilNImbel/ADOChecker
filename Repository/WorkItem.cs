using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
namespace ADOAnalyser
{

    public class WorkItem : IWorkItem
    {
        private readonly IUtility _Utility;

        public WorkItem(IUtility Utility)
        {
            _Utility = Utility;
        }


        public string GetWorkItem(string projectName, string ids)
        {
            string Url = string.Format("{0}/_apis/wit/workitems?ids={1}&api-version=7.1-preview.2", projectName, ids);
            return _Utility.GetDataSync(Url);
            //return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public string GetProjects()
        {
            string Url = string.Format("_apis/projects?getDefaultTeamImageUrl=true");
            return _Utility.GetDataSync(Url);
            //return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public string GetAllWiql(string projectName)
        {
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);
            var query = new
            {
                query = @"
                SELECT [System.Id], [System.Title]
                FROM WorkItems
                WHERE [System.WorkItemType] = 'Task'
                  AND [System.AssignedTo] = ''"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            var result = _Utility.PostDataSync(Url, content);
            return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public string GetAllWiqlByType(string projectName, string workItemType)
        {
            string Url = string.Format("{0}/_apis/wit/wiql?api-version=7.1-preview.2", projectName);
            var query = new
            {
                query = $@"Select [System.Id], [System.Title], [System.State] 
                     From WorkItems
                     Where [System.WorkItemType] = '{workItemType}'
                        AND [System.AssignedTo] = ''
                        AND [System.TeamProject] = '{projectName}'
                     order by [System.CreatedDate] desc"
            };

            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            return _Utility.PostDataSync(Url, content);
        }
    }
}

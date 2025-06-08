using Newtonsoft.Json;

namespace ADOAnalyser.TestModel
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
    }

    public class Relation
    {
        public string rel { get; set; }
        public string url { get; set; }
        public Attributes attributes { get; set; }
    }

    public class TestedByModel
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

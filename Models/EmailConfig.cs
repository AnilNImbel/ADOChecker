using System.ComponentModel.DataAnnotations;

namespace ADOAnalyser.Models
{
    public class EmailConfig
    {
        [Key]
        public int Id { get; set; }
        public string EmailIds { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }

    }
}

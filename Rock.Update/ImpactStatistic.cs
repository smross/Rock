using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Update
{
    [Serializable]
    public class ImpactStatistic
    {
        public Guid RockInstanceId { get; set; }
        public string Version { get; set; }
        public string IpAddress { get; set; }
        public string PublicUrl { get; set; }
        public string OrganizationName { get; set; }
        public ImpactLocation OrganizationLocation { get; set; }
        public int NumberOfActiveRecords { get; set; }
        public string EnvironmentData { get; set; }
    }
}

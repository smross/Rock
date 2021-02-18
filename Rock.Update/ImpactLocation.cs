using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Update
{
    [Serializable]
    public class ImpactLocation
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public ImpactLocation( Rock.Model.Location location )
        {
            Street1 = location.Street1;
            Street2 = location.Street2;
            City = location.City;
            State = location.State;
            PostalCode = location.PostalCode;
            Country = location.Country;
        }
    }
}

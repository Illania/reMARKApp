using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class ContactsModuleInfo
    {
        List<PhysicalAddressType> physicalAddressTypes;

        public List<PhysicalAddressType> PhysicalAddressTypes
        {
            get
            {
                if (physicalAddressTypes == null)
                    physicalAddressTypes = new List<PhysicalAddressType>();
                return physicalAddressTypes;
            }
            set => physicalAddressTypes = value;
        }

        List<CountryInfo> countries;

        public List<CountryInfo> Countries
        {
            get
            {
                if (countries == null)
                    countries = new List<CountryInfo>();
                return countries;
            }
            set => countries = value;
        }

        public Permissions Permissions { get; set; }
    }
}
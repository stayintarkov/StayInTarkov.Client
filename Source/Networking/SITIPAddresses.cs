using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Networking
{
    public class SITIPAddresses
    {
        public SITIPAddresses() 
        { 

        }

        public class SITAddressGroup
        {
            public string IPAddressV4 { get; set; }
            public string IPAddressV6 { get; set; }

            public void ProcessIPAddressResult(string result)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // if contains : then IPv6
                    if (result.Contains(':'))
                    {
                        IPAddressV6 = result;
                    }
                    else if (result.Contains('.'))
                    {
                        IPAddressV4 = result;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }

        public SITAddressGroup ExternalAddresses { get; } = new ();   

        public SITAddressGroup InternalAddresses { get; } = new ();

       
    }
}

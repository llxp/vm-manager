using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManager {
    internal sealed class AzureManagementData {
        public string ClientID { get; set; }
        public string Secret { get; set; }
        public string Tenant { get; set; }
        public string VMName { get; set; }
        public string ResourceGroupName { get; set; }
        public string SubscriptionID { get; set; }
    }
}

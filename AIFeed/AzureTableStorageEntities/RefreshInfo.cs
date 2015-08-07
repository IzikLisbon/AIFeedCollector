using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedCommon.AzureTableStorageEntities
{
    [DataContract]
    public class RefreshInfo : TableEntity
    {
        [DataMember]
        public DateTime? StartRefreshDate { get; set; }

        [DataMember]
        public DateTime? EndRefreshDate { get; set; }
    }
}

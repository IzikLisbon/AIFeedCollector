using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AIFeedCommon.AzureTableStorageEntities;
using AIRssCollection;
using Microsoft.WindowsAzure.Storage.Table;

namespace AIFeedCommon
{
    public class RefreshManager
    {
        private CloudTable RefreshingStateCloudTable;
        private RefreshInfo RefreshInfo;

        public RefreshManager(string azureTableConnectionString)
        {
            this.RefreshingStateCloudTable = AzureTableUtils.GetCloudTable("RefreshingState", azureTableConnectionString);
            this.RefreshInfo = this.RefreshingStateCloudTable.CreateQuery<RefreshInfo>().ToList().FirstOrDefault();
            if (this.RefreshInfo == null)
            {
                this.RefreshInfo = new RefreshInfo();
            }
        }

        public void UpdateStartDate(DateTime dateTime)
        {
            DateTime? backupStartDate = this.RefreshInfo.StartRefreshDate;
            try
            {
                this.RefreshInfo.StartRefreshDate = DateTime.UtcNow;
                this.InsertOrUpdateRefreshInfo();
            }
            catch (Exception e)
            {
                this.RefreshInfo.StartRefreshDate = backupStartDate;
                throw;
            }
        }

        public void UpdateEndRefreshDate(DateTime dateTime)
        {   
            DateTime? backupStartDate = this.RefreshInfo.EndRefreshDate;
            try
            {
                this.RefreshInfo.EndRefreshDate = DateTime.UtcNow;
                this.InsertOrUpdateRefreshInfo();
            }
            catch (Exception e)
            {
                this.RefreshInfo.EndRefreshDate = backupStartDate;
                throw;
            }
        }

        private void InsertOrUpdateRefreshInfo()
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(this.RefreshInfo);
            var res = RefreshingStateCloudTable.Execute(insertOrReplaceOperation);
            if (res.HttpStatusCode >= 300 || res.HttpStatusCode < 200)
            {
                throw new WebException("Status code is " + res.HttpStatusCode);
            }
        }
    }
}

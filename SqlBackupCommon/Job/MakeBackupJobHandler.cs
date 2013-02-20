using Microsoft.SqlServer.Dac;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Quartz;
using System;
using System.IO;

namespace SqlBackupCommon.Job
{
    internal class MakeBackupJobHandler : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var connectionString = context.Trigger.JobDataMap["ExportSqlConnectionString"].ToString();
            var databaseName = context.Trigger.JobDataMap["DatabaseName"].ToString();
            var storageConnection = context.Trigger.JobDataMap["StorageConnection"].ToString();
            var blobContainerName = context.Trigger.JobDataMap["BlobContainerName"].ToString();

            var tempFile = Path.GetTempFileName();
            var errorMessage = "";

            try
            {
                var service = new DacServices(connectionString);
                service.ExportBacpac(tempFile, databaseName);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (!File.Exists(tempFile))
                errorMessage += Environment.NewLine + "BACPAC file was not created.";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(storageConnection);
                var client = storageAccount.CreateCloudBlobClient();

                var container = client.GetContainerReference(blobContainerName);
                container.CreateIfNotExist();

                var blob = container.GetBlockBlobReference(
                    string.Format("{0}_{1}UTC{2}",
                                  databaseName,
                                  DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"),
                                  string.IsNullOrEmpty(errorMessage) ? ".bacpac" : ".error"));
                
                if (string.IsNullOrEmpty(errorMessage))
                    blob.UploadFile(tempFile);
                else
                    blob.UploadText(errorMessage);
            }
            catch { }
        }
    }
}

using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.TransactSql;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.SqlServer.Types;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Quartz;
using System;
using System.IO;

namespace SqlBackupCommon.Job
{
    /// <summary>
    /// Quartz.NET job for backup process
    /// </summary>
    internal class MakeBackupJobHandler : IJob
    {
        /// <summary>
        /// Execute method of the job
        /// </summary>
        /// <param name="context">Execution context</param>
        public void Execute(IJobExecutionContext context)
        {
            // Retrieving parameters from trigger
            var connectionString = context.Trigger.JobDataMap["ExportSqlConnectionString"].ToString();
            var databaseName = context.Trigger.JobDataMap["DatabaseName"].ToString();
            var storageConnection = context.Trigger.JobDataMap["StorageConnection"].ToString();
            var blobContainerName = context.Trigger.JobDataMap["BlobContainerName"].ToString();
            
            var tempFile = Path.GetTempFileName();
            var errorMessage = "";

            try
            {
                // Trying to create BACPAC package
                var service = new DacServices(connectionString);
                service.ExportBacpac(tempFile, databaseName);
            }
            catch (Exception ex)
            {
                // Something went wrong
                errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += Environment.NewLine + "Inner: " + ex.InnerException.Message;
            }

            // If bacpac file still doesn't exist - error
            if (!File.Exists(tempFile))
                errorMessage += Environment.NewLine + "BACPAC file was not created.";

            try
            {
                // Publish to Windows Azure Blob Storage
                var storageAccount = CloudStorageAccount.Parse(storageConnection);
                var client = storageAccount.CreateCloudBlobClient();

                var container = client.GetContainerReference(blobContainerName);
                container.CreateIfNotExist();

                // Blob has .bacpac extension if everything was good
                // and .error extension if there was an exception
                var blob = container.GetBlockBlobReference(
                    string.Format("{0}_{1}UTC{2}",
                                  databaseName,
                                  DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"),
                                  string.IsNullOrEmpty(errorMessage) ? ".bacpac" : ".error"));

                if (string.IsNullOrEmpty(errorMessage))
                {
                    // Upload file if exists
                    blob.UploadFile(tempFile);
                    File.Delete(tempFile);
                }
                else
                {
                    // Upload error message
                    blob.UploadText(errorMessage);
                }
            }
            catch { }
        }
    }
}

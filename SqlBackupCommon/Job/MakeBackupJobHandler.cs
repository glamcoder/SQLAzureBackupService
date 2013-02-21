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
            var properties = context.Trigger.JobDataMap["JobProperties"] as ServiceExecutionProperties;
            var tempFile = Path.GetTempFileName();
            var errorMessage = "";

            try
            {
                // Trying to create BACPAC package
                var service = new DacServices(properties.DbConnectionString);
                service.ExportBacpac(tempFile, properties.DatabaseName);
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
                var acc = new StorageCredentialsAccountAndKey(properties.StorageAccountName, properties.StorageKey);
                var storageAccount = new CloudStorageAccount(acc, true);
                var client = storageAccount.CreateCloudBlobClient();

                var container = client.GetContainerReference(properties.BlobContainerName);
                container.CreateIfNotExist();

                // Blob has .bacpac extension if everything was good
                // and .error extension if there was an exception
                var blob = container.GetBlockBlobReference(
                    string.Format("{0}_{1}UTC{2}",
                                  properties.DatabaseName,
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

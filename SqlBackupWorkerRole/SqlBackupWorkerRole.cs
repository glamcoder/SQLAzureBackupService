using System.Collections.Generic;
using Microsoft.WindowsAzure.ServiceRuntime;
using SqlBackupCommon;
using System.Configuration;
using System.Net;

namespace SqlBackupWorkerRole
{
    public class SqlBackupWorkerRole : RoleEntryPoint
    {
        private ServiceExecutionHandler _serviceExecutionHandler;

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Initialize and schedule backup process
            _serviceExecutionHandler = new ServiceExecutionHandler();
            MakeBackup();

            return base.OnStart();
        }

        private void MakeBackup()
        {
            _serviceExecutionHandler.ScheduleBackup(new ServiceExecutionProperties
            {
                TriggerName = ConfigurationManager.AppSettings["BackupTriggerName"],
                CronExpression = ConfigurationManager.AppSettings["BackupCronExpression"],
                DbConnectionString = ConfigurationManager.AppSettings["ExportSqlConnectionString"],
                DatabaseName = ConfigurationManager.AppSettings["DatabaseName"],
                StorageAccountName = ConfigurationManager.AppSettings["StorageAccountName"],
                StorageKey = ConfigurationManager.AppSettings["StorageKey"],
                BlobContainerName = ConfigurationManager.AppSettings["BlobContainerName"]
            });
        }
    }
}

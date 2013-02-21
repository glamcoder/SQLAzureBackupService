using Microsoft.WindowsAzure.ServiceRuntime;
using SqlBackupCommon;
using System.Configuration;
using System.Net;

namespace SqlBackupWorkerRole
{
    public class SqlBackupWorkerRole : RoleEntryPoint
    {
        private ServiceExecutionHandler _serviceExecutionHandler;

        private void MakeBackup()
        {
            _serviceExecutionHandler.ScheduleBackup(ConfigurationManager.AppSettings["BackupTriggerName"],
                                                    ConfigurationManager.AppSettings["BackupCronExpression"],
                                                    ConfigurationManager.AppSettings["ExportSqlConnectionString"],
                                                    ConfigurationManager.AppSettings["DatabaseName"],
                                                    ConfigurationManager.AppSettings["StorageConnection"],
                                                    ConfigurationManager.AppSettings["BlobContainerName"]);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Initialize and schedule backup process
            _serviceExecutionHandler = new ServiceExecutionHandler();
            MakeBackup();
            
            return base.OnStart();
        }
    }
}

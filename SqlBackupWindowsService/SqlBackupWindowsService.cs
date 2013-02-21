using System.Collections.Generic;
using SqlBackupCommon;
using System.Configuration;
using System.ServiceProcess;

namespace SqlBackupWindowsService
{
    public partial class SqlBackupWindowsService : ServiceBase
    {
        private ServiceExecutionHandler _serviceExecutionHandler;

        public SqlBackupWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Initialize and schedule backup process
            _serviceExecutionHandler = new ServiceExecutionHandler();
            MakeBackup();
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

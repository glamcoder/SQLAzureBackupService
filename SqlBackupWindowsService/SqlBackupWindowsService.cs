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
            _serviceExecutionHandler.ScheduleBackup(ConfigurationManager.AppSettings["BackupTriggerName"],
                                                    ConfigurationManager.AppSettings["BackupCronExpression"],
                                                    ConfigurationManager.AppSettings["ExportSqlConnectionString"],
                                                    ConfigurationManager.AppSettings["DatabaseName"],
                                                    ConfigurationManager.AppSettings["StorageConnection"],
                                                    ConfigurationManager.AppSettings["BlobContainerName"]);
        }

        protected override void OnStop()
        {
        }
    }
}

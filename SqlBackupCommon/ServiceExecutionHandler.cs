using System;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using SqlBackupCommon.Job;

namespace SqlBackupCommon
{
    public class ServiceExecutionHandler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IScheduler _scheduler;

        public ServiceExecutionHandler()
        {
            _schedulerFactory = new StdSchedulerFactory();
            _scheduler = _schedulerFactory.GetScheduler();
            _scheduler.Start();
        }

        public void ScheduleBackup(string triggerName,
                                   string cronExpression,
                                   string connectionString,
                                   string databaseName,
                                   string storageConnection,
                                   string blobContainerName)
        {
            var jobDetail = new JobDetailImpl("MakeBackupJobHandler", null, typeof (MakeBackupJobHandler));

            var trigger = new CronTriggerImpl
                {
                    StartTimeUtc = DateTime.UtcNow,
                    Name = triggerName,
                    CronExpressionString = cronExpression
                };

            trigger.JobDataMap.Add("ExportSqlConnectionString", connectionString);
            trigger.JobDataMap.Add("DatabaseName", databaseName);
            trigger.JobDataMap.Add("StorageConnection", storageConnection);
            trigger.JobDataMap.Add("BlobContainerName", blobContainerName);

            _scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}

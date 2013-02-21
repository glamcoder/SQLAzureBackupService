using System;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using SqlBackupCommon.Job;

namespace SqlBackupCommon
{
    /// <summary>
    /// Common class for two types of services with backup scheduling
    /// </summary>
    public class ServiceExecutionHandler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IScheduler _scheduler;

        /// <summary>
        /// .ctor
        /// </summary>
        public ServiceExecutionHandler()
        {
            _schedulerFactory = new StdSchedulerFactory();
            _scheduler = _schedulerFactory.GetScheduler();
            _scheduler.Start();
        }

        /// <summary>
        /// Shedules backup process
        /// </summary>
        /// <param name="triggerName">Name for trigger</param>
        /// <param name="cronExpression">Croc-like expression for scheduling</param>
        /// <param name="connectionString">SQL Database connection string</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="storageConnection">Storage connection string</param>
        /// <param name="blobContainerName">Blob container name</param>
        public void ScheduleBackup(string triggerName,
                                   string cronExpression,
                                   string connectionString,
                                   string databaseName,
                                   string storageConnection,
                                   string blobContainerName)
        {
            // Configure Quartz.NET job and trigger
            var jobDetail = new JobDetailImpl("MakeBackupJobHandler", null, typeof (MakeBackupJobHandler));
            
            var trigger = new CronTriggerImpl
                {
                    StartTimeUtc = DateTime.UtcNow,
                    Name = triggerName,
                    CronExpressionString = cronExpression
                };

            // Pass parameters to job
            trigger.JobDataMap.Add("ExportSqlConnectionString", connectionString);
            trigger.JobDataMap.Add("DatabaseName", databaseName);
            trigger.JobDataMap.Add("StorageConnection", storageConnection);
            trigger.JobDataMap.Add("BlobContainerName", blobContainerName);

            // Schedule job
            _scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}

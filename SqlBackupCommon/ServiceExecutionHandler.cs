using System;
using System.Collections.Generic;
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
        /// <param name="properties">Backup properties</param>
        public void ScheduleBackup(ServiceExecutionProperties properties)
        {
            // Configure Quartz.NET job and trigger
            var jobDetail = new JobDetailImpl("MakeBackupJobHandler-" + properties.DatabaseName, null,
                                              typeof(MakeBackupJobHandler));

            var trigger = new CronTriggerImpl
            {
                StartTimeUtc = DateTime.UtcNow,
                Name = properties.TriggerName,
                CronExpressionString = properties.CronExpression
            };

            // Pass parameters to job
            trigger.JobDataMap.Add("JobProperties", properties);

            // Schedule job
            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        /// <summary>
        /// Shedules backup process
        /// </summary>
        /// <param name="properties">Collection of all backups' properties</param>
        public void ScheduleBackup(IEnumerable<ServiceExecutionProperties> properties)
        {
            foreach (var prop in properties)
               ScheduleBackup(prop);
        }
    }
}

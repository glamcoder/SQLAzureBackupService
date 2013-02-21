namespace SqlBackupCommon
{
    /// <summary>
    /// Properties needed for job execution
    /// </summary>
    public class ServiceExecutionProperties
    {
        /// <summary>
        /// Trigger name
        /// </summary>
        public string TriggerName { get; set; }
        /// <summary>
        /// Cron-like scheduler expression
        /// </summary>
        public string CronExpression { get; set; }
        /// <summary>
        /// SQL database connection string
        /// </summary>
        public string DbConnectionString { get; set; }
        /// <summary>
        /// SQL database name
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Blob storage account name
        /// </summary>
        public string StorageAccountName { get; set; }
        /// <summary>
        /// Blob storage primary key
        /// </summary>
        public string StorageKey { get; set; }
        /// <summary>
        /// Blob container name
        /// </summary>
        public string BlobContainerName { get; set; }
    }
}

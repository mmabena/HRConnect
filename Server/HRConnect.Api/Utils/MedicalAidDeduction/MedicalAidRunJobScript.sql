-- Check the database platform
DECLARE @platform NVARCHAR(50);
SET @platform = (
    SELECT SERVERPROPERTY('ProductVersion')
);

-- Create the job on Azure SQL
IF @platform LIKE '%AZURE%'
BEGIN
    -- Create a scheduled job that runs daily on Azure SQL
    CREATE JOB myJob
    ENABLED = ON
    ON SCHEDULE = N'Every 24 hours'
    START_DATE = GETDATE()
    REPEAT_TIME = NULL
    WITH
    (
        -- Use the dbo user to execute the job on Azure SQL
        EXECUTE AS USER = 'dbo'
        BEGIN
            UPDATE myTable
            SET myField = 'New Value'
            WHERE effectiveDate = CONVERT(DATE, GETDATE());
        END
    );
END

-- Create the job on SQL Server
ELSE IF @platform LIKE '%SQL%'
BEGIN
    -- Create a scheduled job that runs daily on SQL Server
EXECUTE AS USER = 'sa'
BEGIN
        DECLARE @jobId UNIQUEIDENTIFIER;
        DECLARE @jobName NVARCHAR(100);

        -- Create the job
        CREATE JOB @jobId = 'myJob'
        ON Schedule
        (
            Frequency = TYPE 
            {
                Type = WEEKLY
                Interval = 1
                Day = 0
                FrequencySubtype = WEEKDAY
            },
            Enabled = 1,
            StartDate = GETDATE(),
            EndDate = NULL
        )
        WITH
        (
            -- Use the sa user to execute the job on SQL Server
            Step = N'SQLCommand'
            ,Command = N'UPDATE myTable SET myField = ''New Value'' WHERE effectiveDate = CONVERT(DATE, GETDATE())'
            ,TargetServerName = NULL
        );

        -- Get the job ID
        SET @jobName = 'myJob';
SELECT @jobId = [job_id] FROM msdb.dbo.sysjobs WHERE name = @jobName;

-- Enable the job
EXEC msdb.dbo.sp_update_job
        @job_id = @jobId,
        @enabled = 1;
END
END
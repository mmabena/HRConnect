-- Check the database platform
DECLARE @platform NVARCHAR(50);
DECLARE @isAzure BIT = 0;
DECLARE @isSqlServer BIT = 0;

-- Better platform detection
SET @platform = CAST(SERVERPROPERTY('EngineEdition') AS NVARCHAR(50));

-- Engine Edition values:
-- 1 = Personal or Desktop Engine
-- 2 = Standard
-- 3 = Enterprise
-- 4 = Express
-- 5 = SQL Database (Azure SQL)
-- 6 = SQL Data Warehouse
-- 8 = Managed Instance (Azure)

IF @platform = '5' OR @platform = '6' OR @platform = '8'
    BEGIN
        SET @isAzure = 1;
    END
ELSE
    BEGIN
        SET @isSqlServer = 1;
    END

-- Create the job on Azure SQL
IF @isAzure = 1
    BEGIN
        PRINT 'Creating job for Azure SQL...';

        -- Azure SQL uses Elastic Job Agent
        -- Note: This requires an Elastic Job Agent to be set up first
        BEGIN TRY
            -- Add the job
            EXEC jobs.sp_add_job
                 @job_name = N'MedicalAidDeductionActivatorJob',
                 @description = N'Activates medical aid deductions daily based on effective date',
                 @enabled = 1;

            -- Add the job step
            EXEC jobs.sp_add_jobstep
                 @job_name = N'MedicalAidDeductionActivatorJob',
                 @step_name = N'ActivateMedicalAidDeductions',
                 @command = N'UPDATE HRConnect.dbo.MedicalAidDeductions
                         SET IsActive = ''1''
                         WHERE EffectiveDate = CONVERT(DATE, GETDATE());',
                 @credential_name = N'job_credential', -- You need to create this credential
                 @target_group_name = N'DefaultTargetGroup'; -- Or your target group

            -- Add the schedule
            EXEC jobs.sp_add_schedule
                 @schedule_name = N'DailyActivationSchedule',
                 @freq_type = 4, -- Daily
                 @freq_interval = 1, -- Every day
                 @active_start_time = 010000; -- 1:00 AM

            -- Attach schedule to job
            EXEC jobs.sp_attach_schedule
                 @job_name = N'MedicalAidDeductionActivatorJob',
                 @schedule_name = N'DailyActivationSchedule';

            PRINT 'Azure SQL job created successfully.';
        END TRY
        BEGIN CATCH
            PRINT 'Error creating Azure SQL job: ' + ERROR_MESSAGE();
            PRINT 'Note: Ensure Elastic Job Agent is properly configured.';
        END CATCH
    END

-- Create the job on SQL Server
ELSE IF @isSqlServer = 1
    BEGIN
        PRINT 'Creating job for SQL Server...';

        BEGIN TRY
            DECLARE @jobId BINARY(16);
            DECLARE @jobName NVARCHAR(100) = N'MedicalAidDeductionActivatorJob';
            DECLARE @scheduleId INT;

            -- Create the job
            EXEC msdb.dbo.sp_add_job
                 @job_name = @jobName,
                 @description = N'Activates medical aid deductions daily based on effective date',
                 @enabled = 1,
                 @job_id = @jobId OUTPUT;

            -- Add the job step
            EXEC msdb.dbo.sp_add_jobstep
                 @job_id = @jobId,
                 @step_name = N'ActivateMedicalAidDeductions',
                 @subsystem = N'TSQL',
                 @command = N'UPDATE HRConnect.dbo.MedicalAidDeductions
                         SET IsActive = ''1''
                         WHERE EffectiveDate = CONVERT(DATE, GETDATE());',
                 @database_name = N'HRConnect',
                 @on_success_action = 1, -- Quit with success
                 @on_fail_action = 2; -- Quit with failure

            -- Create the schedule
            EXEC msdb.dbo.sp_add_schedule
                 @schedule_name = N'DailyActivationSchedule',
                 @freq_type = 4, -- Daily
                 @freq_interval = 1, -- Every day
                 @active_start_time = 010000; -- 1:00 AM
            @schedule_id = @scheduleId OUTPUT;

            -- Attach schedule to job
            EXEC msdb.dbo.sp_attach_schedule
                 @job_id = @jobId,
                 @schedule_name = N'DailyActivationSchedule';

            -- Add job to server
            EXEC msdb.dbo.sp_add_jobserver
                 @job_id = @jobId;

            PRINT 'SQL Server job created successfully.';
        END TRY
        BEGIN CATCH
            PRINT 'Error creating SQL Server job: ' + ERROR_MESSAGE();
            PRINT 'Note: Ensure SQL Server Agent is running and you have permissions.';
        END CATCH
    END

-- If platform detection failed
ELSE
    BEGIN
        PRINT 'Unable to determine database platform. Engine Edition: ' + @platform;
        PRINT 'Manual job creation may be required.';
    END
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime.CredentialManagement;
using Avt.Agents.Services.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avt.Agents.Services.Services
{
    public class DaemonService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly TaskRunnerBase _task;

        public DaemonService(
            ILogger<DaemonService> logger, 
            Simulator simulator, 
            Scheduler scheduler)
        {
            _logger = logger;
           

            if (!string.IsNullOrEmpty(Shared.Configuration["aws_access_key_id"]) &&
                !string.IsNullOrEmpty(Shared.Configuration["aws_secret_access_key"]))
            {
                var credentialFile = new NetSDKCredentialsFile();
                var option = new CredentialProfileOptions
                {
                    AccessKey = Shared.Configuration["aws_access_key_id"],
                    SecretKey = Shared.Configuration["aws_secret_access_key"]
                };
                var profile = new CredentialProfile("default", option)
                {
                    Region = Amazon.RegionEndpoint.EUWest1                    
                };
                credentialFile.RegisterProfile(profile);
            }

            var serviceName = Shared.Configuration["ServiceName"];
            if ("simulator".Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                _task = simulator;
            else
                _task = scheduler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _task.StartAsync(cancellationToken);
            _logger.LogDebug($"{DateTime.Now:yyy-MM-dd HH:mm:ss.ttt}: deamon [{_task.TaskName}] started ...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _task.StopAsync(cancellationToken);
            _logger.LogDebug($"{DateTime.Now:yyy-MM-dd HH:mm:ss.ttt}: deamon [{_task.TaskName}] stopped ...");
        }

        public void Dispose()
        {
            _task.Dispose();
        }
    }
}
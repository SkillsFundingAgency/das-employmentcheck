using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using StructureMap;
using SubmissionEventWorkerRole.DependencyResolution;

namespace SubmissionEventWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private IContainer _container;

        private ISubmissionEventManager _submissionManager;

        public override void Run()
        {
            Trace.TraceInformation("SubmissionEventWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            _container = ConfigureIocContainer();
            _submissionManager = _container.GetInstance<ISubmissionEventManager>();
            bool result = base.OnStart();

            Trace.TraceInformation("SubmissionEventWorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("SubmissionEventWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("SubmissionEventWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await _submissionManager.DetermineProcessingStartingPoint();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await _submissionManager.PollSubmissionEvents();
                await Task.Delay(300000, cancellationToken);
            }
        }

        private IContainer ConfigureIocContainer()
        {
            var container = new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
            });
            return container;
        }
    }
}

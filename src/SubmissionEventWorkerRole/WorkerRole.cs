using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using SFA.DAS.EmploymentCheck.Application.Commands.InitiateEmploymentCheckForChangedNationalInsuranceNumbers;
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
        private IMediator _mediator;

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
            ServicePointManager.DefaultConnectionLimit = 12;

            _container = ConfigureIocContainer();
            _mediator = _container.GetInstance<IMediator>();
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
            while (!cancellationToken.IsCancellationRequested)
            {
                await _mediator.PublishAsync(new InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest());
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

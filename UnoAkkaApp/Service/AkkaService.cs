using Akka.Actor;
using Akka.Routing;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using UnoAkkaApp.Actors;

namespace UnoAkkaApp.Service
{
    public sealed class AkkaService : IHostedService
    {        
        private ActorSystem AkkaSystem;        

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;        

        public AkkaService(IServiceProvider sp, ILogger<AkkaService> logger)
        {
            _serviceProvider = sp;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // start ActorSystem                        
            AkkaSystem = ActorSystem.Create("AkkaSystem");

            var unoActor = AkkaSystem.ActorOf(Props.Create(() => new UnoActor()), "unoActor");

            List<string> workActors = new List<string>();

            for (int i = 0; i < 9; i++)
            {
                string actorName = $"workActor{i + 1}";
                var curWorkActor = AkkaSystem.ActorOf(Props.Create(() => new WorkActor(unoActor, i + 1)), actorName);
                curWorkActor.Tell(new BatchData());
                workActors.Add($"/user/{actorName}");
            }

            // 참고 : https://getakka.net/articles/actors/routers.html            
            // RandomGroup / RoundRobinGroup
            var router = AkkaSystem.ActorOf(Props.Empty.WithRouter(new RandomGroup(workActors)), "routerGroup");

            // 밸브 Work : 초당 작업량을 조절                
            int timeSec = 1;
            int elemntPerSec = 2;
            var throttleWork = AkkaSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork");
            // 밸브 작업자를 지정
            throttleWork.Tell(new SetTarget(router));

            //분산처리할 100개의 샘플데이터생성
            List<object> batchDatas = new List<object>();
            for (int i = 0; i < 100; i++)
            {
                batchDatas.Add(new BatchData() { Data="SomeData" });
            }
            BatchList batchList = new BatchList(batchDatas.ToImmutableList());

            //100개의 데이터전송(벌크)
            throttleWork.Tell(batchList);

            _logger.LogInformation("Start AkkaSystem...");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // strictly speaking this may not be necessary - terminating the ActorSystem would also work
            // but this call guarantees that the shutdown of the cluster is graceful regardless
            _logger.LogInformation("Stop AkkaSystem...");
            await CoordinatedShutdown.Get(AkkaSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }
    }
}

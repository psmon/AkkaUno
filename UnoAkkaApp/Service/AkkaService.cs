using Akka.Actor;
using Akka.Routing;
using AkkaDotModule.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnoAkkaApp.Actors;
using UnoAkkaApp.Model;
using ThrottleWork = UnoAkkaApp.Actors.ThrottleWork;

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

            var arduSerialPort = new SerialPort();
            arduSerialPort.PortName = "COM4";   //아두이노가 연결된 시리얼 포트 번호 지정
            arduSerialPort.BaudRate = 9600;     //시리얼 통신 속도 지정
            arduSerialPort.Open();              //포트 오픈

            var unoActor = AkkaSystem.ActorOf(Props.Create(() => new UnoSendActor(arduSerialPort)), "unoActor");

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
            var router = AkkaSystem.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup(workActors)), "routerGroup");

            // 밸브 Work : 초당 작업량을 조절                
            int timeSec = 1;
            int elemntPerSec = 3;
            var throttleWork = AkkaSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork");
            // 밸브 작업자를 지정
            throttleWork.Tell(new SetTarget(router));

            var unoReadActor = AkkaSystem.ActorOf(Props.Create(() => new UnoReadActor(arduSerialPort, throttleWork)), "unoReadActor");
            unoReadActor.Tell(new SerialRead());

            //분산처리할 10개의 샘플데이터생성
            List<object> batchDatas = new List<object>();
            for (int i = 0; i < 10; i++)
            {
                batchDatas.Add(new BatchData() { Data="SomeData" });
            }
            BatchList batchList = new BatchList(batchDatas.ToImmutableList());

            //매5초마다 batchList 데이터 처리
            AkkaSystem
            .Scheduler
            .ScheduleTellRepeatedly(
                 TimeSpan.FromSeconds(1),    //1초뒤부터 작동
                 TimeSpan.FromSeconds(5),    //5초마다 이벤트발생
                 throttleWork, batchList, ActorRefs.NoSender
             );

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

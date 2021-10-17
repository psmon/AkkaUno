using System;
using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaDotModule.Models;

namespace UnoAkkaApp.Actors
{
    public class ThrottleWork : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private IActorRef consumer;

        private int countPerSec;

        public ThrottleWork(int element, int maxBust)
        {
            countPerSec = element;

            ReceiveAsync<SetTarget>(async target =>
            {
                consumer = target.Ref;
            });

            ReceiveAsync<int>(async count =>
            {
                countPerSec = count;

                logger.Info($"ThrottleWork Spped:{countPerSec}");
            });

            ReceiveAsync<BatchList>(async batchMessage =>
            {
                int Count = batchMessage.Obj.Count;
                Source<object, NotUsed> source = Source.From(batchMessage.Obj);

                using (var materializer = Context.Materializer())
                {
                    var factorials = source;
                    factorials
                         .Throttle(countPerSec, TimeSpan.FromSeconds(1), maxBust, ThrottleMode.Shaping)
                         .RunForeach(obj => {
                             var nowstr = DateTime.Now.ToString("mm:ss");
                             if (obj is BatchData batchData)
                             {
                                 if (consumer != null) consumer.Tell(batchData);
                             }
                         }, materializer)
                         .Wait();
                }
            });
        }
    }
}

using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Models;

namespace UnoAkkaApp.Actors
{
    public class WorkActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private readonly IActorRef _unoActor;

        public WorkActor(IActorRef unoActor,int nodeNum)
        {
            _unoActor = unoActor;

            int _nodeNo = nodeNum;

            ReceiveAsync<BatchData>(async command =>
            {
                logger.Info($"Receive Data..");
                unoActor.Tell(nodeNum.ToString());
            });
        }
    }
}

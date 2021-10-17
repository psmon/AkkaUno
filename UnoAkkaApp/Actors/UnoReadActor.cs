using Akka.Actor;
using Akka.Event;
using System;
using System.IO.Ports;
using UnoAkkaApp.Model;

namespace UnoAkkaApp.Actors
{
    public class UnoReadActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        // 참고 : 리눅스의 경우 SerialPortStream 사용(현재 윈도우 지원)
        private readonly SerialPort arduSerialPort;

        private readonly IActorRef throttleWork;

        public UnoReadActor(SerialPort serialPort, IActorRef _throttleWork)
        {
            arduSerialPort = serialPort;

            throttleWork = _throttleWork;

            ReceiveAsync<SerialRead>(async command =>
            {
                SerialRead();                
            });

        }

        protected void SerialRead()
        {
            string readString = arduSerialPort.ReadLine();

            if (readString.Contains("Speed"))
            {
                var speed = int.Parse(readString.Split(":")[1]);

                int countPerSec = (speed / 10) + 3;

                throttleWork.Tell(countPerSec);
            }

            Self.Tell(new SerialRead());
        }

    }
}

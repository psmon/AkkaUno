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

        public UnoReadActor(SerialPort serialPort)
        {
            arduSerialPort = serialPort;            

            ReceiveAsync<SerialRead>(async command =>
            {
                SerialRead();                
            });

        }

        protected void SerialRead()
        {
            try
            {
                string readString = arduSerialPort.ReadLine();

                if (readString.Contains("Speed"))
                {
                    logger.Info($"SerialRead : {readString}");
                }

                this.Self.Tell(new SerialRead());
            }
            catch(Exception e)
            {

            }
        }

    }
}

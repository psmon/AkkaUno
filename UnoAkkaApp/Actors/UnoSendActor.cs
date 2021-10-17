using Akka.Actor;
using Akka.Event;
using System;
using System.IO.Ports;
using UnoAkkaApp.Model;

namespace UnoAkkaApp.Actors
{
    public class UnoSendActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        // 참고 : 리눅스의 경우 SerialPortStream 사용(현재 윈도우 지원)
        private readonly SerialPort arduSerialPort;        

        public UnoSendActor(SerialPort serialPort)
        {
            arduSerialPort = serialPort;                        

            ReceiveAsync<string>(async command =>
            {
                logger.Info($"SerialWrite : {command}");
                arduSerialPort.Write(command);                
            });

            ReceiveAsync<SerialRead>(async command =>
            {
                //SerialRead();
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
            }
            catch(Exception e)
            {

            }
        }

    }
}

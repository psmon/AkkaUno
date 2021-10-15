using Akka.Actor;
using Akka.Event;
using System.IO.Ports;

namespace UnoAkkaApp.Actors
{
    public class UnoActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        // 참고 : 리눅스의 경우 SerialPortStream 사용(현재 윈도우 지원)
        private readonly SerialPort arduSerialPort;        

        public UnoActor()
        {
            arduSerialPort = new SerialPort();
            arduSerialPort.PortName = "COM3";   //아두이노가 연결된 시리얼 포트 번호 지정
            arduSerialPort.BaudRate = 9600;     //시리얼 통신 속도 지정
            arduSerialPort.Open();              //포트 오픈

            ReceiveAsync<string>(async command =>
            {
                logger.Info($"SerialWrite : {command}");
                arduSerialPort.Write(command);
            });
        }

        protected override void PostStop()
        {
            arduSerialPort.Close();
            logger.Info("UnoActor Stop");
        }

    }
}

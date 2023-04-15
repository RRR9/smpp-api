using System.Net;
using Inetlab.SMPP;
using Inetlab.SMPP.Common;
using Inetlab.SMPP.PDU;

namespace SmppServer
{
    public static class Smpp
    {
        public static readonly string RemoteAddress;
        public static readonly int RemotePort;
        private static readonly string _login;
        private static readonly string _password;
        private static SmppClient _smppClient = null;
        private static BindResp _bindResp = null;

        public static bool TryToConnect()
        {
            bool connect = false;
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(RemoteAddress), RemotePort);
            _smppClient = new SmppClient()
            {
                EnquireInterval = 60,
                Timeout = 60000,
                NeedEnquireLink = true,
                AddrNpi = 0,
                AddrTon = 0,
                WorkerThreads = 1,
                SystemType = ""
            };
            _smppClient.LocalEndPoint = new IPEndPoint(IPAddress.Any, 12332);
            if (!_smppClient.Connect(ipEndPoint))
            {
                return connect;
            }
            _bindResp = _smppClient.Bind(_login, _password, ConnectionMode.Transceiver);
            if (_bindResp.Status == CommandStatus.ESME_ROK)
            {
                LogManager.Debug("SmppClient " + _bindResp.SystemId + " with status " + _bindResp.Status.ToString());
                connect = true;
                return connect;
            }
            else
            {
                LogManager.Debug("Binding SMSC Client Failed  : " + _bindResp.Command.ToString() + " with status " + _bindResp.Status.ToString());
                return connect;
            }
        }

        public static bool TryToSend(string message, string to, string from)
        {
            bool sended = false;
            var submitSmBuilder = SMS.ForSubmit().From(from).To(to).Coding(DataCodings.UCS2).Text(message);
            foreach (var submitSm in submitSmBuilder.Create(_smppClient))
            {
                submitSm.Sequence = _smppClient.SequenceGenerator.NextSequenceNumber();
                SubmitSmResp smResponse = _smppClient.Submit(submitSm);
                if (smResponse.Status == CommandStatus.ESME_ROK)
                {
                    sended = true;
                }
            }
            return sended;
        }

        public static bool CheckStatusConnection()
        {
            if (_smppClient != null && _smppClient.Connected == true)
            {
                if (_bindResp != null && _bindResp.Status == CommandStatus.ESME_ROK)
                {
                    return true;
                }
            }
            return false;
        }

    }
}

namespace SmppServer
{
    public static class SmppMethods
    {
        public static bool SendSms(string message, string to, string from)
        {
            bool sendMessage = false;
            if (!Smpp.CheckStatusConnection())
            {
                if (!Smpp.TryToConnect())
                {
                    LogManager.Debug($"Connecting to {Smpp.RemoteAddress}:{Smpp.RemotePort} failed!");
                    return sendMessage;
                }
            }

            if (!Smpp.TryToSend(message, to, from))
            {
                LogManager.Debug($"Sending message to {Smpp.RemoteAddress}:{Smpp.RemotePort} failed!");
                return sendMessage;
            }
            else
            {
                LogManager.Debug("Sending message:" + message + "\n to: " + to + "\n from " + from);
            }

            sendMessage = true;
            return sendMessage;
        }
    }

}
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace SmppServer
{
    public partial class Sms : System.Web.UI.Page
    {
        private string _password;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.TrySkipIisCustomErrors = true;
            if (Request.QueryString.Count != 1 || Request.RequestType != "POST")
            {
                string errorMessage;
                if (Request.QueryString.Count != 1)
                {
                    errorMessage = "Неверное количество параметров в Url";
                }
                else
                {
                    errorMessage = $"Неверный метод запроса {Request.RequestType}";
                }
                SendResponse(errorMessage, HttpStatusCode.BadRequest);
                return;
            }
            string body;
            using(StreamReader streamReader = new StreamReader(Request.InputStream))
            {
                body = streamReader.ReadToEnd();
            }

            dynamic request = null;
            try
            {
                request = JsonConvert.DeserializeObject(body);
            }
            catch
            {
                SendResponse("Невозможно десериализовать JSON", HttpStatusCode.BadRequest);
                return;
            }

            string message = request.message;
            string to = request.to;
            string from = request.from;

            if (Md5(_password + message + to + from) != Request.QueryString["key"])
            {
                string errorMessage = "Неправильное значение key";
                if (message is null || to is null || from is null)
                {
                    errorMessage = "Параметры JSON заданы неверно";
                }
                SendResponse(errorMessage, HttpStatusCode.BadRequest);
                return;
            }

            try
            {
                message = DecryptString(message);
            }
            catch
            {
                SendResponse("Поле 'message' зашифровано неправильно", HttpStatusCode.BadRequest);
                return;
            }

            if(SmppMethods.SendSms(message, to, from))
            {
                SendResponse("СМС сообщение отправлено успешно!", HttpStatusCode.OK);
            }
            else
            {
                SendResponse("СМС сообщение не отправлено!", HttpStatusCode.BadRequest);
            }

        }

        private void SendResponse(string str, HttpStatusCode httpStatusCode)
        {
            JObject jsonFormation = new JObject();
            Response.ContentType = "application/json";
            if (httpStatusCode == HttpStatusCode.OK)
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                jsonFormation["success"] = str;
                Response.Write(jsonFormation.ToString());
            }
            else if (httpStatusCode == HttpStatusCode.BadRequest)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                jsonFormation["error"] = str;
                Response.Write(jsonFormation.ToString());
            }
        }

        private string DecryptString(string encryptedString)
        {
            string decryptMessage = "";
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                string privateKey = GetPrivateKey();
                rsa.FromXmlString(privateKey);
                byte[] encryptedByteMessage = Convert.FromBase64String(encryptedString);
                byte[] decryptedByteMessage = rsa.Decrypt(encryptedByteMessage, false);
                decryptMessage = Encoding.UTF8.GetString(decryptedByteMessage);
            }
            return decryptMessage;
        }

        private string GetPrivateKey()
        {
            using (StreamReader streamReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "PrivateKey.txt")))
            {
                return streamReader.ReadToEnd();
            }
        }

        private string GetPublicKey()
        {
            using (StreamReader streamReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "PublicKey.txt")))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static string Md5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}

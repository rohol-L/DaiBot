using DaiBot.Core;
using DaiBot.Core.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace DaiBot.Plugin.Main.MessageSource
{
    public class WebMessageSource : IMessageSource
    {
        readonly HttpListener listener = new();
        readonly string host;
        readonly ushort port;
        bool stop = false;
        Action<MessageContext>? send;

        public WebMessageSource(IBotConfig config)
        {
            host = config["http.host"] ?? "localhost";
            port = config.Value<ushort>("http.port");
            if (port < 1024)
            {
                port = 8067;
            }
            listener.Prefixes.Add($"http://{host}:{port}/");
        }

        public void Start(Action<MessageContext> send)
        {
            listener.Start();
            listener.BeginGetContext(Result, null);
            this.send = send;
            Console.WriteLine($"http监听:{host}:{port}");
        }

        public void Stop()
        {
            stop = true;
            listener.Stop();
            listener.Close();
            Console.WriteLine($"http停止监听:{host}:{port}");
        }

        private void Result(IAsyncResult result)
        {
            if (stop)
            {
                return;
            }
            //获得context对象
            var context = listener.EndGetContext(result);
            var request = context.Request;
            var response = context.Response;

            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.AppendHeader("Access-Control-Allow-Headers", "*");
            context.Response.AppendHeader("Access-Control-Allow-Method", "*");
            context.Response.ContentType = "text/plain;charset=UTF-8";
            context.Response.AddHeader("Content-type", "text/plain");
            context.Response.ContentEncoding = Encoding.UTF8;

            response.StatusCode = 200;

            string? message = request.QueryString["cmd"] ?? string.Empty;
            object? payload = null;

            if (request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                var content = reader.ReadToEnd();
                payload = content;
            }

            var ev = new MessageContext("http", message, message, rsp =>
            {
                using var sw = new StreamWriter(response.OutputStream, Encoding.UTF8);
                sw.Write(JsonConvert.SerializeObject(new { rsp }));
                response.Close();
            })
            {
                GroupID = 0,
                UserID = 0,
            };
            ev.Authorization.Add("http");
            ev.Authorization.Add("http" + request.HttpMethod.ToLower());
            ev.OtherPayload = payload;
            try
            {
                send?.Invoke(ev);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error]:" + ex.Message);
            }
            finally
            {
                response.Close();
            }

            listener.BeginGetContext(Result, null);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core
{
    public class Response
    {
        public List<string> Rsps { get; set; }

        public static Response Empty => new Response();

        public Response(string? rsp)
        {
            Rsps = new();
            if (rsp != null)
            {
                Rsps.Add(rsp);
            }
        }

        public Response(params string[] rsp)
        {
            Rsps = rsp.ToList();
        }

        public Response(List<string> rsp)
        {
            Rsps = rsp;
        }

        public static implicit operator Response(string? rsp)
        {
            return new Response(rsp);
        }

        public static implicit operator Response(List<string> rsp)
        {
            return new Response(rsp);
        }
    }
}

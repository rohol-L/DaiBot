using DaiBot.Core.Delegates;
using System.Collections.Concurrent;
using System.Text;

namespace DaiBot.Core.Utils
{
    public static class MyConsole
    {
        private static bool startLoop = false;
        private static bool cmdMode = false;
        private static readonly StringBuilder sb = new();

        public readonly static List<OnMessageDelegate> Receiver = new();
        public readonly static ConcurrentQueue<(string name, OnMessageDelegate call)> WaitList = new();

        public static void WriteLine(string? value)
        {
            if (cmdMode)
            {
                sb.AppendLine(value);
            }
            else
            {
                Console.WriteLine(value);
            }
        }

        public static void ReadLine(string name, OnMessageDelegate callback)
        {
            if (WaitList.IsEmpty)
            {
                Console.Write($"请输入{name}：");
            }
            WaitList.Enqueue((name, callback));
        }

        private static string ReadLine()
        {
            if (!cmdMode)
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) ;
                Console.WriteLine("[!]命令模式已开启");
                Console.WriteLine();
                cmdMode = true;
            }
            Console.Write(">>>");
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("[!]命令模式已关闭");
                cmdMode = false;
                if (sb.Length > 0)
                {
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                }
                return ReadLine();
            }
            else
            {
                return line;
            }
        }

        public static void StartLoop()
        {
            if (startLoop)
            {
                return;
            }
            Console.WriteLine("[!]按回车进入命令模式");
            startLoop = true;
            new Thread(() =>
            {
                while (true)
                {
                    string message = ReadLine();
                    if (WaitList.TryDequeue(out var item))
                    {
                        item.call.Invoke(message);
                        if (WaitList.TryPeek(out var item2))
                        {
                            Console.WriteLine($"请输入{item2.name}:");
                        }
                        continue;
                    }
                    for (int i = 0; i < Receiver.Count; i++)
                    {
                        Receiver[i].Invoke(message);
                    };
                }
            }).Start();
        }
    }
}

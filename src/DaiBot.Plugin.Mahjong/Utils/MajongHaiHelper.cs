using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using DaiBot.Plugin.Nanikiru.Model;
using System.Reflection;

namespace DaiBot.Plugin.Nanikiru.Utils
{
    internal static class MajongHaiHelper
    {
        public static string GetDoraPointer(string hai)
        {
            char[] tokens = hai.ToCharArray();
            int start = 0;
            int max = 9;
            if (tokens[0] == 'r')
            {
                start = 1;
            }
            if (tokens[start + 1] == 'z')
            {
                max = 7;
            }
            char num = (char)((hai[start] - '2' + max) % max + '1');
            return new string(new char[] { num, hai[start + 1] });
        }

        public static List<string?> GetHais(string haisi)
        {
            List<string?> hais = new();
            int last = 0;
            for (int i = 0; i < haisi.Length; i++)
            {
                char c1 = haisi[i];
                if (c1 == ',')
                {
                    hais.Add(null);
                    last = i + 1;
                    continue;
                }
                if (!"mpsz".Contains(c1))
                {
                    continue;
                }
                for (int j = last; j < i; j++)
                {
                    char c2 = haisi[j];
                    if (c2 == 'r')
                    {
                        hais.Add("r5" + c1);
                        j++;
                    }
                    else
                    {
                        hais.Add(string.Concat(haisi[j], c1));
                    }
                }
                last = i + 1;
            }
            return hais;
        }

        public static List<string> GetKans(List<string?> haisi)
        {
            List<string> kans = new();
            Dictionary<string, int> counts = new();
            bool furu = false;
            for (int i = 0; i < haisi.Count; i++)
            {
                string? current = haisi[i]?.Trim('r');
                if (current == null)
                {
                    furu = true;
                    continue;
                }
                if (counts.TryGetValue(current, out int value))
                {
                    if (value == 3)
                    {
                        kans.Add(current);
                    }
                    counts[current]++;
                }
                else if (!furu)
                {
                    counts.Add(current, 1);
                }
            }
            return kans;
        }

        public struct HaiDrawInfo
        {
            public string Hai;
            public int X;
            public bool Trans;
        }

        public static List<HaiDrawInfo> GetHaiDrawInfos(List<string?> haisi, int tsumoIndex, int width, int height, int span)
        {
            int lastx = 0;
            int furuIndex = 0;
            bool startFuru = false;
            string? tsumoHai = null;
            List<HaiDrawInfo> hdis = new();
            for (int i = 0; i < haisi.Count; i++)
            {
                string? hai = haisi[i];
                if (hai == null)
                {
                    furuIndex = 0;
                    if (!startFuru)
                    {
                        //first null
                        if (tsumoHai == null)
                        {
                            throw new Exception("tsumoHai is null");
                        }
                        lastx += span;
                        hdis.Add(new()
                        {
                            Hai = tsumoHai,
                            X = lastx,
                            Trans = false,
                        });
                        lastx += width;
                        startFuru = true;
                    }
                    lastx += span;
                    continue;
                }
                if (i == tsumoIndex)
                {
                    tsumoHai = hai;
                    continue;
                }
                bool trans = false;
                if (startFuru)
                {
                    char num = GetHaiNumber(hai);
                    if (furuIndex == 0)
                    {
                        char next = GetHaiNumber(haisi[i + 1]);
                        if (next - num == 1)
                        {
                            trans = true;
                        }
                    }
                    else if (furuIndex == 1)
                    {
                        char prev = GetHaiNumber(haisi[i - 1]);
                        if (prev == num)
                        {
                            trans = true;
                        }
                    }
                    furuIndex++;
                }
                hdis.Add(new()
                {
                    Hai = hai,
                    X = lastx,
                    Trans = trans,
                });
                lastx += trans ? height : width;
            }
            if (!startFuru && tsumoHai != null)
            {
                hdis.Add(new()
                {
                    Hai = tsumoHai,
                    X = lastx + span,
                    Trans = false,
                });
            }
            return hdis;
        }

        public static char GetHaiNumber(string? hai)
        {
            if (hai == null) return '0';
            return hai[0] == 'r' ? '5' : hai[0];
        }

        static readonly char[] kazes = new char[] { '东', '南', '西', '北' };

        public static Image<Rgba32> CreatePic(NanikiruProblem problem, string resourcePath)
        {
            if (!Directory.Exists("cache"))
            {
                Directory.CreateDirectory("cache");
            }
            if (problem.Dora == null)
            {
                throw new Exception("NanikiruProblem " + problem.ProblemNumber + " Dora is null");
            }
            if (problem.Kaze == null)
            {
                throw new Exception("NanikiruProblem " + problem.ProblemNumber + " Kaze is null");
            }
            if (problem.Haisi == null)
            {
                throw new Exception("NanikiruProblem " + problem.ProblemNumber + " Haisi is null");
            }

            string doraPointer = GetDoraPointer(problem.Dora);
            string? tsumo = problem.Tumo;
            List<string?> haisiList = GetHais(problem.Haisi);
            int haisiCount = haisiList.IndexOf(null);
            if (haisiCount < 0)
            {
                haisiCount = 14;
            }

            string info = $"东1局 {kazes[problem.Kaze[0] - '1']}家 {problem.Junme}巡目";

            int tsumoIdx = -1;
            if (tsumo?.Length > 0)
            {
                tsumoIdx = haisiList.IndexOf(problem.Tumo);
            }
            if (tsumoIdx < 0)
            {
                tsumoIdx = new Random().Next(haisiCount);
            }

            List<string> kanList = GetKans(haisiList);

            var fonts = new FontCollection();
            var fontFamily = fonts.Add(Path.Combine(resourcePath, "Resource/shangshouzhuqueti.ttf"));
            var f = new Font(fontFamily, 50);
            var f_tip = new Font(fontFamily, 40);
            int haiWidth = 66;
            int haiHeight = 90;
            int span = 20;
            var hdis = GetHaiDrawInfos(haisiList, tsumoIdx, haiWidth, haiHeight, 20);
            int picWidth = haiWidth * 14 + span * 5 + (14 - haisiCount) / 3 * (haiHeight - haiWidth + span);

            Image<Rgba32> image = new(picWidth, span * 5 + haiHeight * 2 + (kanList.Count > 0 ? 110 : 0));
            image.Mutate(context =>
            {
                context.Fill(Color.White);
                context.DrawText(info, f, Color.Black, new PointF((picWidth - span * 4 - haiWidth * 7) / 2 - 200 + span, span * 2 + 10));

                for (int i = 0; i < 7; i++)
                {
                    string hai = i == 2 ? doraPointer : "5z";
                    using Image img = Image.Load(Path.Combine(resourcePath, @"Resource\hai\" + hai + ".png"));
                    context.DrawImage(img, new Point(picWidth - span * 2 - (7 - i) * haiWidth, span * 2), 1);
                }

                foreach (var hdi in hdis)
                {
                    int y = span * 3 + haiHeight;
                    using Image img = Image.Load(Path.Combine(resourcePath, @"Resource\hai\" + hdi.Hai + ".png"));
                    if (hdi.Trans)
                    {
                        img.Mutate(x => x.RotateFlip(RotateMode.Rotate270, FlipMode.None));
                        y += haiHeight - haiWidth;
                    }
                    context.DrawImage(img, new Point(span * 2 + hdi.X, y), 1);
                }

                if (kanList.Count > 0)
                {
                    int y = span * 4 + haiHeight * 2 + 10;
                    context.DrawText("其他选项：", f_tip, Color.Black, new PointF(span * 2, y + 26));
                    for (int i = 0; i < kanList.Count; i++)
                    {
                        string hai = kanList[i];
                        context.DrawText("杠", f, Color.Black, new PointF(span * 2 + 190 + 300 * i, y + 20));
                        using Image img = Image.Load(Path.Combine(resourcePath, @"Resource\hai\" + hai + ".png"));
                        context.DrawImage(img, new Point(span * 2 + 250 + i * 300, y), 1);
                    }
                }

            });
            return image;
        }


        /*public static string hai2Emoji(string hai)
        {
            Dictionary<char, byte> codeOffset = new()
            {
                { 'm', 135 },
                { 's', 144 },
                { 'p', 153 },
                { 'z', 128 },
            };
        }*/
    }

}

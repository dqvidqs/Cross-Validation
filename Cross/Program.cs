using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Cross
{
    class Program
    {
        private static MetaData Meta;
        private static Dictionary<string, Word> Data;
        private static Dictionary<string, Word> Valid;
        private static Dictionary<string, Word> Values;
        private static double Prefix = 0.4;
        private static double Neuatral = 0.5;
        private static int LexQuantity = 20; 
        private static int CountSpamLex;
        private static int CountHamLex;

        static void Main(string[] args)
        {
            Setup();
            Options();
        }
        static private void Setup()
        {
            Meta = new MetaData(Directory.GetCurrentDirectory());
            Meta.SetFiles(Directory.GetFiles(Meta.GetDirHam()), Directory.GetFiles(Meta.GetDirSpam()));
        }
        static private void Options()
        {
            string[] Command = Console.ReadLine().Split(' ');
            switch (Command[0].ToLower())
            {
                case "cross":
                    Cross(Command);
                    break;
                case "help":
                    Console.WriteLine(Help.Cross());
                    break;
                case "setcross":
                    Change(Command);
                    break;
                default:
                    Console.WriteLine("Help?");
                    Options();
                    break;
            }
            Options();
        }
        static private void Change(string[] Command)
        {
            Meta.SetNumber(Convert.ToInt32(Command[1]));
            Console.WriteLine("Cross Number set to = {0}", Meta.N);
        }
        static private double SpamAsHam;
        static private double SpamAsSpam;
        static private double HamAsHam;
        static private double HamAsSpam;
        static private void Cross(string[] Command)
        {
            if (Command.Length == 4)
            {
                Neuatral = Convert.ToDouble(Command[3]);
            }
            if (Command.Length >= 3)
            {
                Prefix = Convert.ToDouble(Command[2]);
            }
            if (Command.Length >= 2)
            {
                LexQuantity = Convert.ToInt32(Command[1]);
            }
            
            double SumHam = 0;
            double SumSpam = 0;

            for (int i = 0; i < Meta.N; i++)
            {
                CountHamLex = 0;
                CountSpamLex = 0;
                SpamAsHam = 0;
                SpamAsSpam = 0;
                HamAsHam = 0;
                HamAsSpam = 0;
                Data = new Dictionary<string, Word>();
                Valid = new Dictionary<string, Word>();
                Train(i);
                Calculate();
                ValidCross(i);
                SumHam += HamAsHam / (HamAsHam + HamAsSpam) * 100;
                SumSpam += SpamAsSpam / (SpamAsHam + SpamAsSpam) * 100;
                Console.WriteLine("Iteration = {0}", i + 1);
                Console.WriteLine("     True Positive:");
                Console.WriteLine("         HAM correctly identified as HAM:        {0:0.00} %", HamAsHam / (HamAsHam + HamAsSpam) * 100);
                Console.WriteLine("         SPAM correctly identified as SPAM:      {0:0.00} %", SpamAsSpam / (SpamAsSpam + SpamAsHam) * 100);
                Console.WriteLine("     False Positive:");
                Console.WriteLine("         HAM incorrectly identified as SPAM:     {0:0.00} %", HamAsSpam / (HamAsHam + HamAsSpam) * 100);
                Console.WriteLine("         SPAM incorrectly identified as HAM:     {0:0.00} %", SpamAsHam / (SpamAsSpam + SpamAsHam) * 100);
                Console.WriteLine();
            }
            Console.WriteLine("N = {0}, Value = {1}, Neutral = {2}\n", LexQuantity, Prefix, Neuatral);
            Console.WriteLine("Average True Positive:");
            Console.WriteLine("     HAM correctly identified as HAM:        {0:0.00} %", SumHam / Meta.N);
            Console.WriteLine("     SPAM correctly identified as SPAM:      {0:0.00} %", SumSpam / Meta.N);
            Console.WriteLine("     Average:                                {0:0.00} %", (SumSpam + SumHam) / Meta.N / 2);
            Console.WriteLine("Average False Positive:");
            Console.WriteLine("     HAM incorrectly identified as SPAM:     {0:0.00} %", 100 - (SumHam / Meta.N));
            Console.WriteLine("     SPAM incorrectly identified as HAM:     {0:0.00} %", 100 - (SumSpam / Meta.N));
            Console.WriteLine("     Average:                                {0:0.00} %", 100 - (SumSpam + SumHam) / Meta.N / 2);
            Console.WriteLine();
        }
        static private void Train(int index)
        {
            int LengthSpam = Meta.GetSpamFileLength();
            int LengthHam = Meta.GetHamFileLength();
            int StartIgnoreSpam = Meta.GetSpamIndex(index);
            int EndIgnoreSpam = Meta.GetSpamIndex(index + 1);
            int StartIgnoreHam = Meta.GetHamIndex(index);
            int EndIgnoreHam = Meta.GetHamIndex(index + 1);
            for (int i = 0; i < LengthSpam; i++)
            {
                if (!(i >= StartIgnoreSpam && i < EndIgnoreSpam))
                {
                    string Text = System.IO.File.ReadAllText(Meta.GetSpamFile(i));
                    var Lexes = TextToLex(Text);                    
                    foreach (var Lex in Lexes)
                    {
                        CountSpamLex++;
                        string lex = Lex.ToString().ToLower();
                        if (Data.ContainsKey(lex))
                        {
                            Data[lex].IncreaseSpam();
                        }
                        else
                        {
                            Data.Add(lex, new Word(lex, 1, 0));
                        }
                    }
                }
                else
                {
                    i = EndIgnoreSpam;
                }
            }
            
            for (int i = 0; i < LengthHam; i++)
            {
                if (!(i >= StartIgnoreHam && i < EndIgnoreHam))
                {
                    string Text = System.IO.File.ReadAllText(Meta.GetHamFile(i));
                    var Lexes = TextToLex(Text);
                    foreach (var Lex in Lexes)
                    {
                        string lex = Lex.ToString().ToLower();
                        CountHamLex++;
                        if (Data.ContainsKey(lex))
                        {
                            Data[lex].IncreaseHam();
                        }
                        else
                        {
                            Data.Add(lex, new Word(lex, 0, 1));
                        }
                    }
                }
                else
                {
                    i = EndIgnoreHam;
                }
            }
        }
        
        static private void ValidCross(int index)
        {
            double Probobility;
            int LengthSpam = Meta.GetSpamFileLength();
            int LengthHam = Meta.GetHamFileLength();
            int StartSpam = Meta.GetSpamIndex(index);
            int EndSpam = Meta.GetSpamIndex(index + 1);
            int StartHam = Meta.GetHamIndex(index);
            int EndHam = Meta.GetHamIndex(index + 1);
            for (int i = StartSpam; i < EndSpam; i++)
            {
                Valid = new Dictionary<string, Word>();
                string Text = System.IO.File.ReadAllText(Meta.GetSpamFile(i));
                var Lexes = TextToLex(Text);
                foreach(var Lex in Lexes)
                {
                    string lex = Lex.ToString().ToLower();
                    if (Data.ContainsKey(lex))
                    {
                        if (!Valid.ContainsKey(lex))
                        {
                            Valid.Add(lex, Data[lex]);
                        }
                    }
                    else
                    {
                        if (!Valid.ContainsKey(lex))
                        {
                            Valid.Add(lex, new Word(lex, 0, 0));
                            Valid[lex].SetProbability(Prefix);
                        }
                    }
                }
                Values = GetMaxValues();
                Probobility = GetProbability(Values);
                if(Probobility > 0.75)
                {
                    SpamAsSpam++;
                }
                else
                {
                    SpamAsHam++;
                }
            }
            
            for (int i = StartHam; i < EndHam; i++)
            {
                Valid = new Dictionary<string, Word>();
                string Text = System.IO.File.ReadAllText(Meta.GetHamFile(i));
                var Lexes = TextToLex(Text);
                foreach (var Lex in Lexes)
                {
                    string lex = Lex.ToString().ToLower();
                    if (Data.ContainsKey(lex))
                    {
                        if (!Valid.ContainsKey(lex))
                        {
                            Valid.Add(lex, Data[lex]);
                        }
                    }
                    else
                    {
                        if (!Valid.ContainsKey(lex))
                        {
                            Valid.Add(lex, new Word(lex, 0, 0));
                            Valid[lex].SetProbability(Prefix);
                        }
                    }
                }
                Values = GetMaxValues();
                Probobility = GetProbability(Values);
                if (Probobility > 0.75)
                {
                    HamAsSpam++;
                }
                else
                {
                    HamAsHam++;
                }
            }
        }
        static private double GetProbability(Dictionary<string, Word> Temp)
        {
            double a = 1;
            double b = 1;
            foreach(var lex in Temp)
            {
                a *= lex.Value.GetProbability();
                b *= (1 - lex.Value.GetProbability());
            }
            return a / (a + b);
        }
        static private Dictionary<string, Word> GetMaxValues()
        {
            double max;
            int N = LexQuantity;
            string lex = null;
            Dictionary<string, Word> Temp = new Dictionary<string, Word>();
            List<string> Val = new List<string>();
            if(Valid.Count < LexQuantity)
            {
                N = Valid.Count;
            }
            for (int i = 0; i < N; i++)
            {
                max = 0;
                lex = null;
                foreach (var Lex in Valid)
                {
                    
                    if (max < Math.Abs(Lex.Value.GetProbability() - Neuatral))
                    {
                        max = Math.Abs(Lex.Value.GetProbability() - Neuatral);
                        lex = Valid[Lex.Key].GetWord();
                    }
                }
                Temp.Add(lex, Valid[lex]);
                Valid.Remove(lex);
            }
            return Temp;
        }
        static private MatchCollection TextToLex(string Text)
        {
            Regex Pattern = new Regex(@"([A-z]|[0-9]|'|""|$)+");
            var Lexes = Pattern.Matches(Text);
            return Lexes;
        }
        static private void Calculate()
        {
            foreach (var Lex in Data)
            {
                if(Lex.Value.GetSpam() == 0)
                {
                    Data[Lex.Key].SetProbability(0.01);
                    continue;
                }
                if(Lex.Value.GetHam() == 0)
                {
                    Data[Lex.Key].SetProbability(0.99);
                    continue;
                }
                double PWS = (double)(Lex.Value.GetSpam()) / (double)CountSpamLex;
                double PWH = (double)(Lex.Value.GetHam()) / (double)CountHamLex;
                double PSW = PWS / (PWS + PWH);
                Data[Lex.Key].SetProbability(PSW);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross
{
    class Word
    {
        string Lexema;
        int Spam;
        int Ham;
        double Probability;
        public Word() { }
        public Word(string Lexema, int Spam, int Ham)
        {
            this.Lexema = Lexema;
            this.Spam = Spam;
            this.Ham = Ham;
        }
        public double GetProbability()
        {
            return Probability;
        }
        public void SetProbability(double Probability)
        {
            this.Probability = Probability;
        }
        public void IncreaseSpam()
        {
            Spam++;
        }
        public void IncreaseHam()
        {
            Ham++;
        }
        public int GetSpam()
        {
            return Spam;
        }
        public int GetHam()
        {
            return Ham;
        }
        public string GetWord()
        {
            return Lexema;
        }
    }
}

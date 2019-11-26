using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross
{
    class MetaData
    {
        private string _path;
        private string[] _fileSpam;
        private string[] _fileHam;
        private string[] _dataPath = { "//spam//Spamas", "//ham//Ne spamas" };
        private int _crossNumber = 10;
        private int[] _SIndexes;
        private int[] _HIndexes;
        public MetaData(string Path)
        {
            _path = Path;
        }
        public string GetDirSpam()
        {
            return _path + _dataPath[0];
        }
        public string GetDirHam()
        {
            return _path + _dataPath[1];
        }
        public void SetNumber(int Number)
        {
            _crossNumber = Number;
            SetFiles(Directory.GetFiles(GetDirHam()), Directory.GetFiles(GetDirSpam()));
        }
        public void SetFiles(string[] HamFiles, string[] SpamFiles)
        {
            Array.Sort(HamFiles);
            Array.Sort(SpamFiles);
            _fileHam = HamFiles;
            _fileSpam = SpamFiles;
            _SIndexes = GetIndexes(_crossNumber, _fileSpam.Length);
            _HIndexes = GetIndexes(_crossNumber, _fileHam.Length);
        }
        public string GetHamFile(int index)
        {
            return _fileHam[index];
        }
        public string GetSpamFile(int index)
        {
            return _fileSpam[index];
        }
        public int GetHamFileLength()
        {
            return _fileHam.Length;
        }
        public int GetSpamFileLength()
        {
            return _fileSpam.Length;
        }
        private int[] GetIndexes(int Parts, int Quantity)
        {
            int[] indexes = new int[Parts + 1];
            int current = Quantity / Parts;
            int part = current;
            for (int i = 1; i < indexes.Length - 1; i++)
            {
                indexes[i] = current;
                current += part;
            }
            indexes[indexes.Length - 1] = Quantity;
            return indexes;
        }
        public int N { get { return _crossNumber; } }
        public int GetHamIndex(int index)
        {
            return _HIndexes[index];
        }
        public int GetSpamIndex(int index)
        {
            return _SIndexes[index];
        }
    }
}

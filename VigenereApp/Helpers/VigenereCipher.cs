using System;
using System.Text;

namespace VigenereSimple
{
    public static class VigenereCipher
    {
        private const string Eng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Rus = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        public static string Process(string text, string key, bool isEncrypt)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (string.IsNullOrEmpty(key)) return text;

            var result = new StringBuilder();
            int keyIndex = 0;
            string upperKey = key.ToUpper();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    string alphabet = char.IsAscii(c) ? Eng : Rus;
                    char baseChar = char.IsUpper(c) 
                        ? (char.IsAscii(c) ? 'A' : 'А') 
                        : (char.IsAscii(c) ? 'a' : 'а');
                    
                    int alphaLen = alphabet.Length;
                    int charIdx = alphabet.IndexOf(char.ToUpper(c));

                    if (charIdx == -1) 
                    { 
                        result.Append(c); 
                        continue; 
                    }
                    char kChar = upperKey[keyIndex % upperKey.Length];
                    string keyAlphabet = char.IsAscii(kChar) ? Eng : Rus;
                    int keyIdx = keyAlphabet.IndexOf(char.ToUpper(kChar));
                    
                    if (keyIdx == -1)
                    {
                         result.Append(c);
                         continue;
                    }

                    int newIdx;
                    if (isEncrypt)
                        newIdx = (charIdx + keyIdx) % alphaLen;
                    else
                        newIdx = (charIdx - keyIdx + alphaLen) % alphaLen;

                    char newChar = alphabet[newIdx];
                    result.Append(char.IsUpper(c) ? newChar : char.ToLower(newChar));
                    
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }
    }
}
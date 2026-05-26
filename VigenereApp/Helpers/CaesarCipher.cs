using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VigenereSimple
{
    public static class CaesarCipher
    {
        private const string Eng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // Исправлено: убрана Ё, теперь 32 буквы как в классическом шифре Цезаря
        private const string Rus = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        // Эталонные частоты для английского (26 букв)
        private static readonly double[] EngFreq =
        [
            0.0817, 0.0149, 0.0278, 0.0425, 0.1270, 0.0223, 0.0202,
            0.0609, 0.0697, 0.0015, 0.0077, 0.0403, 0.0241, 0.0675,
            0.0751, 0.0193, 0.0009, 0.0599, 0.0633, 0.0906, 0.0276,
            0.0098, 0.0236, 0.0015, 0.0197, 0.0007
        ];

        // Исправлено: убрано значение для Ё (было 0.0004 на 7-й позиции), теперь 32 значения
        private static readonly double[] RusFreq =
        [
            0.0780, 0.0164, 0.0450, 0.0185, 0.0309, 0.0842,  // А-Е, Ж
            0.0100, 0.0169, 0.0696, 0.0130, 0.0330, 0.0427,  // З-Л
            0.0317, 0.0678, 0.1118, 0.0266, 0.0491, 0.0534,  // М-Т
            0.0646, 0.0242, 0.0026, 0.0102, 0.0055, 0.0150,  // У-Ь
            0.0085, 0.0047, 0.0004, 0.0209, 0.0203, 0.0039,  // Э-Ю
            0.0076, 0.0224                                    // Я
        ];

        /// <summary>
        /// Определяет алфавит по символу (без учёта Ё)
        /// </summary>
        private static string GetAlphabet(char c)
        {
            bool isRussian = (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я');
            return isRussian ? Rus : Eng;
        }

        /// <summary>
        /// Шифрование текста
        /// </summary>
        public static string Encrypt(string text, int shift)
        {
            return Process(text, shift);
        }

        /// <summary>
        /// Дешифрование текста
        /// </summary>
        public static string Decrypt(string text, int shift)
        {
            return Process(text, -shift);
        }

        /// <summary>
        /// Универсальный метод для сдвига (положительный = вправо, отрицательный = влево)
        /// </summary>
        private static string Process(string text, int shift)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var result = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    string alphabet = GetAlphabet(c);
                    int alphaLen = alphabet.Length;

                    // Приводим к верхнему регистру для поиска
                    char upperC = char.ToUpperInvariant(c);
                    int charIdx = alphabet.IndexOf(upperC);

                    if (charIdx == -1)
                    {
                        result.Append(c);
                        continue;
                    }

                    // Вычисляем эффективный сдвиг
                    int effectiveShift = shift % alphaLen;
                    if (effectiveShift < 0) effectiveShift += alphaLen;

                    int newIdx = (charIdx + effectiveShift) % alphaLen;
                    char newChar = alphabet[newIdx];

                    // Сохраняем исходный регистр
                    result.Append(char.IsUpper(c) ? newChar : char.ToLowerInvariant(newChar));
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Автоматический взлом: возвращает наиболее вероятный открытый текст
        /// </summary>
        public static string Crack(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var variants = GetAllDecryptions(text);
            return variants.OrderBy(v => v.Score).First().Text;
        }

        /// <summary>
        /// Возвращает все варианты дешифровки с оценкой качества
        /// </summary>
        public static List<(int Shift, string Text, double Score)> GetAllDecryptions(string text)
        {
            // Исправлено: проверка на русский без учёта Ё
            bool hasRussian = text.Any(c => (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я'));
            string alphabet = hasRussian ? Rus : Eng;
            double[] expectedFreq = hasRussian ? RusFreq : EngFreq;

            var results = new List<(int, string, double)>();
            int alphaLen = alphabet.Length;

            for (int shift = 0; shift < alphaLen; shift++)
            {
                string decrypted = Decrypt(text, shift);
                double score = CalculateChiSquared(decrypted, alphabet, expectedFreq);
                results.Add((shift, decrypted, score));
            }
            return results;
        }

        /// <summary>
        /// Вычисляет Chi-Squared (меньше = лучше)
        /// </summary>
        private static double CalculateChiSquared(string text, string alphabet, double[] expectedFreq)
        {
            int alphaLen = alphabet.Length;
            int[] counts = new int[alphaLen];
            int total = 0;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char upperC = char.ToUpperInvariant(c);
                    int idx = alphabet.IndexOf(upperC);
                    if (idx >= 0)
                    {
                        counts[idx]++;
                        total++;
                    }
                }
            }

            if (total == 0) return double.MaxValue;

            double chiSq = 0;
            for (int i = 0; i < alphaLen; i++)
            {
                double expectedCount = total * expectedFreq[i];
                if (expectedCount > 0)
                {
                    chiSq += Math.Pow(counts[i] - expectedCount, 2) / expectedCount;
                }
            }
            return chiSq;
        }
    }
}
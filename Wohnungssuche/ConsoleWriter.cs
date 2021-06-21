using System;
using System.Collections.Generic;
using System.Text;

namespace Wohnungssuche
{
    /// <summary>
    /// Erweiterung für die Konsole. 
    /// </summary>
    public static class ConsoleWriter
    {
        private static readonly object ConsoleWriterLock = new();

        /// <summary>
        /// Schreibt threadsicher eine farbige Zeile in die Console.
        /// </summary>
        public static void WriteLine(string category, string text, ConsoleColor categoryColor = ConsoleColor.White, ConsoleColor textColor = ConsoleColor.Gray)
        {
            if (category is null)
                throw new ArgumentNullException(nameof(category));

            if (text is null)
                throw new ArgumentNullException(nameof(text));

            lock (ConsoleWriterLock)
            {
                Console.ForegroundColor = categoryColor;
                Console.Write(category);
                Console.ForegroundColor = textColor;
                Console.WriteLine($" {text}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Schreibt threadsicher eine farbige Zeile in die Console.
        /// </summary>
        public static void WriteLine(string text, ConsoleColor textColor = ConsoleColor.Gray)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            lock (ConsoleWriterLock)
            {
                Console.ForegroundColor = textColor;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
    }
}

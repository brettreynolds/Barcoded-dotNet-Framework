using System;
using System.Collections.Generic;

namespace Barcoded
{
    internal class LinearEncoding
    {

        //public Dictionary<int, int> Symbols { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, LinearSymbol> Symbols { get; } = new Dictionary<int, LinearSymbol>();

        public int MinimumWidth { get; private set; }

        internal void Add(string character, int characterType, LinearPattern pattern)
        {
            int position = Symbols.Count;
            Add(position, character, characterType, pattern);
        }

        internal void Add(int position, string character, int characterType, LinearPattern pattern)
        {
            int width = pattern.GetWidth();
            LinearSymbol symbol = new LinearSymbol(character, characterType, pattern, width);
            Symbols.Add(position, symbol);
            MinimumWidth += width;
        }

        public void Clear()
        {
            MinimumWidth = 0;
            Symbols.Clear();
        }

        /// <summary>
        /// Gets the greatest width of all encoded symbols 
        /// </summary>
        /// <returns>Widest symbol width</returns>
        public int GetWidestSymbol()
        {
            int widestSymbol = 0;

            for (int symbol = 0; symbol <= Symbols.Count - 1; symbol++)
            {
                widestSymbol = Math.Max(Symbols[symbol].Width, widestSymbol);
            }
            return widestSymbol;
        }
    }
}

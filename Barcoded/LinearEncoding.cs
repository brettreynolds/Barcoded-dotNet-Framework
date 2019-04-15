using System;
using System.Collections.Generic;

namespace Barcoded
{
    internal class LinearEncoding
    {

        //public Dictionary<int, int> Symbols { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, LinearSymbol> Symbols { get; private set; } = new Dictionary<int, LinearSymbol>();

        public int MinimumWidth { get; private set; }

        public void Add(string character, int characterType, LinearPattern pattern)
        {
            int Position = this.Symbols.Count;
            this.Add(Position, character, characterType, pattern);
        }

        public void Add(int position, string character, int characterType, LinearPattern pattern)
        {
            int width = pattern.GetWidth();
            LinearSymbol Symbol = new LinearSymbol(character, characterType, pattern, width);
            this.Symbols.Add(position, Symbol);
            MinimumWidth += width;
        }

        public void Clear()
        {
            this.MinimumWidth = 0;
            this.Symbols.Clear();
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

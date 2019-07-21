namespace Barcoded
{
    /// <summary>
    /// Barcode encoding symbol for a given character or function
    /// </summary>
    internal class LinearSymbol
    {
        public string Character { get; set; }
        public int CharacterType { get; set; }
        public LinearPattern Pattern { get; set; }
        public int Width { get; set; }

        internal LinearSymbol(string character, int characterType, LinearPattern pattern, int width)
        {
            Character = character;
            CharacterType = characterType;
            Pattern = pattern;
            Width = width;
        }
    }
}

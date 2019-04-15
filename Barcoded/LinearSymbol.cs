namespace Barcoded
{
    class LinearSymbol
    {
        public string Character { get; set; }
        public int CharacterType { get; set; }
        public LinearPattern Pattern { get; set; }
        public int Width { get; set; }

        internal LinearSymbol(string character, int characterType, LinearPattern pattern, int width)
        {
            this.Character = character;
            this.CharacterType = characterType;
            this.Pattern = pattern;
            this.Width = width;
        }
    }
}

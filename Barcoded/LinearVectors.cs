using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Vectored version of the barcode
    /// </summary>
    public class LinearVectors
    {
        internal LinearVectors()
        {
            // Empty Constructor
            Width = 0;
        }

        internal LinearVectors(LinearEncoder encoder)
        {
            Data.Clear();
            Width = encoder.LinearEncoding.MinimumWidth * encoder.XDimension;

            foreach (KeyValuePair<int, LinearSymbol> symbol in encoder.LinearEncoding.Symbols)
            {
                foreach (KeyValuePair<int, LinearModule> module in symbol.Value.Pattern)
                {
                    LinearModule newModule = new LinearModule(module.Value.ModuleType, module.Value.Width * encoder.XDimension);
                    Data.Add(Data.Count, newModule);
                }
            }
        }

        /// <summary>
        /// The combined bar and space modules, ordered list that represents the full barcode.
        /// </summary>
        public Dictionary<int, LinearModule> Data { get; } = new Dictionary<int, LinearModule>();

        /// <summary>
        /// Total point width of the vector data
        /// </summary>
        public int Width { get; }
    }
}

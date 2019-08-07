using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Barcode pattern of bars and spaces for a given symbol.
    /// </summary>
    internal class LinearPattern : Dictionary<int, LinearModule>
    {
        /// <summary>
        /// Creates a linear pattern using a string of int values for each module width.
        /// </summary>
        /// <param name="intPattern"></param>
        /// <param name="firstModule"></param>
        internal LinearPattern(string intPattern, ModuleType firstModule)
        {
            ModuleType moduleType = firstModule;
            for (int position = 0; position <= intPattern.Length - 1; position++)
            {
                Add(position, new LinearModule(moduleType, Convert.ToInt32(intPattern.Substring(position, 1))));

                switch (moduleType)
                {
                    case ModuleType.Bar:
                        moduleType = ModuleType.Space;
                        break;
                    case ModuleType.Space:
                        moduleType = ModuleType.Bar;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a linear pattern using a (N)arrow (W)ide string and narrow to wide ratio for calculating each module width.
        /// </summary>
        /// <param name="narrowWidePattern"></param>
        /// <param name="firstModule"></param>
        /// <param name="wideRatio"></param>
        internal LinearPattern(string narrowWidePattern, ModuleType firstModule, int wideRatio)
        {
            ModuleType moduleType = firstModule;
            for (int position = 0; position <= narrowWidePattern.Length - 1; position++)
            {
                int moduleWidth = narrowWidePattern.Substring(position, 1).ToUpper() == "N" ? 1 : wideRatio;
                Add(position, new LinearModule(moduleType, moduleWidth));

                switch (moduleType)
                {
                    case ModuleType.Bar:
                        moduleType = ModuleType.Space;
                        break;
                    case ModuleType.Space:
                        moduleType = ModuleType.Bar;
                        break;
                }
            }
        }

        /// <summary>
        /// Get the pattern total point width.
        /// </summary>
        /// <returns>Returns the sum of the module widths within the pattern.</returns>
        public int GetWidth()
        {
            int symbolWidth = 0;

            for (int module = 0; module <= Count - 1; module++)
            {
                symbolWidth += this[module].Width;
            }

            return symbolWidth;
        }
    }
}

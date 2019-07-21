using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Barcode pattern of bars and spaces for a given symbol.
    /// </summary>
    internal class LinearPattern : Dictionary<int, LinearModule>
    {
        internal LinearPattern(string pattern, ModuleType firstModule)
        {
            ModuleType moduleType = firstModule;
            for (int position = 0; position <= pattern.Length - 1; position++)
            {
                Add(position, new LinearModule(moduleType, Convert.ToInt32(pattern.Substring(position, 1))));

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

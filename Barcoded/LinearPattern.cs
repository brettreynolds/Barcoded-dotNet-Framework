using System;
using System.Collections.Generic;
using System.Linq;

namespace Barcoded
{
    class LinearPattern : Dictionary<int, LinearModule>
    {
        public LinearPattern(string pattern, ModuleType firstModule)
        {
            ModuleType moduleType = firstModule;
            for (int position = 0; position <= pattern.Length - 1; position++)
            {
                this.Add(position, new LinearModule(moduleType, Convert.ToInt32(pattern.Substring(position, 1))));

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

        public int GetWidth()
        {
            int symbolWidth = 0;

            for (int module = 0; module <= this.Count() - 1; module++)
            {
                symbolWidth += this[module].Width;
            }

            return symbolWidth;
        }
    }
}

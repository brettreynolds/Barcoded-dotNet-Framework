namespace Barcoded
{
    /// <summary>
    /// The bar or space of element that makes up a symbol pattern.
    /// </summary>
    public class LinearModule
    {
        /// <summary>
        /// Module type (bar or space).
        /// </summary>
        public ModuleType ModuleType { get; set; }

        /// <summary>
        /// Module point width.
        /// </summary>
        public int Width { get; set; }

        internal LinearModule(ModuleType moduleType, int width)
        {
            this.ModuleType = moduleType;
            this.Width = width;
        }
    }
}

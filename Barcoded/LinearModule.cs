namespace Barcoded
{
    class LinearModule
    {
        public ModuleType ModuleType { get; set; }
        public int Width { get; set; }

        public LinearModule(ModuleType moduleType, int width)
        {
            this.ModuleType = moduleType;
            this.Width = width;
        }
    }
}

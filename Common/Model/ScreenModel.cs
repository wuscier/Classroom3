namespace Common.Model
{
    public class ScreenModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// 1：主显示屏，2：扩展显示屏
        /// </summary>
        public int Type { get; set; }
    }
}

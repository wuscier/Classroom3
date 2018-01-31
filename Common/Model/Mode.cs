namespace Common.Model
{
    public class Mode
    {
        /// <summary>
        /// 模式名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否可接收命令
        /// </summary>
        public bool ReceiveCommand { get; set; }
    }
}

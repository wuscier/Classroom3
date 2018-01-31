using Common.Helper;

namespace Classroom.SwichModel
{
    class CommandController : CommandSender
    {
        private static CommandController _instance;
        public static CommandController Instance => _instance ?? (_instance = new CommandController());

        protected override void OnSendCommand(string commandName)
        {
            if (GlobalData.Instance.CurrentMode.ReceiveCommand)
            {
                base.OnSendCommand(commandName);
            }
        }

        public void ExecuteCommand(string commandName)
        {
            OnSendCommand(commandName);
        }
    }
}

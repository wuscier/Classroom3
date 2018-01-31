namespace Classroom.SwichModel
{
    class CommandSender
    {
        private static readonly string EventJoinMeeting = "0";
        private static readonly string EventExitMeeting = "1";

        private static readonly string EventSetSpeakerMode = "2";
        private static readonly string EventSetCourseMode = "3";
        private static readonly string EventSetInteractionMode = "4";
        private static readonly string EventOpenDocument = "5";
        private static readonly string EventCloseDocument = "6";

        private static readonly string EventSetAutoLayout = "7";
        private static readonly string EventSetFlatLayout = "8";
        private static readonly string EventSetPictureLayout = "9";
        private static readonly string EventSetFeatureLayout = "10";

        public delegate void CommandHandler();

        public event CommandHandler JoinMeeting
        {
            add
            {
                Events.AddHandler(EventJoinMeeting, value);
            }
            remove
            {
                Events.RemoveHandler(EventJoinMeeting, value);
            }
        }
        public event CommandHandler ExitMeeting
        {
            add
            {
                Events.AddHandler(EventExitMeeting, value);
            }
            remove
            {
                Events.RemoveHandler(EventExitMeeting, value);
            }
        }

        public event CommandHandler SetSpeakerMode
        {
            add
            {
                Events.AddHandler(EventSetSpeakerMode, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetSpeakerMode, value);
            }
        }
        public event CommandHandler SetCourseMode
        {
            add
            {
                Events.AddHandler(EventSetCourseMode, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetCourseMode, value);
            }
        }
        public event CommandHandler SetInteractionMode
        {
            add
            {
                Events.AddHandler(EventSetInteractionMode, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetInteractionMode, value);
            }
        }
        public event CommandHandler OpenDocument
        {
            add
            {
                Events.AddHandler(EventOpenDocument, value);
            }
            remove
            {
                Events.RemoveHandler(EventOpenDocument, value);
            }
        }
        public event CommandHandler CloseDocument
        {
            add
            {
                Events.AddHandler(EventCloseDocument, value);
            }
            remove
            {
                Events.RemoveHandler(EventCloseDocument, value);
            }
        }

        public event CommandHandler SetAutoLayout
        {
            add
            {
                Events.AddHandler(EventSetAutoLayout, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetAutoLayout, value);
            }
        }
        public event CommandHandler SetFlatLayout
        {
            add
            {
                Events.AddHandler(EventSetFlatLayout, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetFlatLayout, value);
            }
        }
        public event CommandHandler SetPictureLayout
        {
            add
            {
                Events.AddHandler(EventSetPictureLayout, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetPictureLayout, value);
            }
        }
        public event CommandHandler SetFeatureLayout
        {
            add
            {
                Events.AddHandler(EventSetFeatureLayout, value);
            }
            remove
            {
                Events.RemoveHandler(EventSetFeatureLayout, value);
            }
        }

        private CommandHandlerList events;
        protected CommandHandlerList Events
        {
            get
            {
                if (events == null)
                {
                    events = new CommandHandlerList();
                }
                return events;
            }
        }

        protected virtual void OnSendCommand(string commandName)
        {
            CommandHandler handler = (CommandHandler)Events[commandName];
            if (handler != null)
            {
                handler();
            }
        }
    }
}

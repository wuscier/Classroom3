using System;

namespace Classroom.SwichModel
{
    public sealed class CommandHandlerList
    {
        ListEntry head;

        public Delegate this[string key]
        {
            get
            {
                ListEntry e = null;
                e = Find(key);
                if (e != null)
                {
                    return e.handler;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                ListEntry e = Find(key);
                if (e != null)
                {
                    e.handler = value;
                }
                else
                {
                    head = new ListEntry(key, value, head);
                }
            }
        }

        private ListEntry Find(string key)
        {
            ListEntry found = head;

            while (found != null)
            {
                if (found.key == key)
                {
                    break;
                }
                found = found.next;
            }
            return found;
        }

        public void AddHandler(string key, Delegate value)
        {
            ListEntry e = Find(key);

            if (e != null)
            {
                e.handler = Delegate.Combine(e.handler, value);
            }
            else
            {
                head = new ListEntry(key, value, head);
            }
        }

        public void RemoveHandler(string key, Delegate value)
        {
            ListEntry e = Find(key);
            if (e != null)
            {
                e.handler = Delegate.Remove(e.handler, value);
            }
        }

        private sealed class ListEntry
        {
            internal ListEntry next;
            internal string key;
            internal Delegate handler;

            public ListEntry(string key, Delegate handler, ListEntry next)
            {
                this.next = next;
                this.key = key;
                this.handler = handler;
            }
        }
    }
}

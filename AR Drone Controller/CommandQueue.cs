using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR_Drone_Controller
{
    class CommandQueue
    {
        public const int MaxMessageLength = 1024;

        private readonly Queue<string> _commands = new Queue<string>();
        private readonly object _syncLock = new object();

        internal virtual string Flush()
        {
            var message = AppendCommands();

            if (message.Length > 0)
            {
                return message.ToString();
            }

            return null;
        }

        private StringBuilder AppendCommands()
        {
            var message = new StringBuilder();
            lock (_syncLock)
            {
                while (_commands.Any() && message.Length + _commands.Peek().Length <= MaxMessageLength)
                {
                    message.Append(_commands.Dequeue());
                }
            }

            return message;
        }

        internal virtual void Enqueue(string command)
        {
            if (command.Length > MaxMessageLength)
            {
                throw new CommandTooLongException(command);
            }

            lock (_syncLock)
            {
                _commands.Enqueue(command);
            }
        }

        public class CommandTooLongException : Exception
        {
            private const string CommandKey = "Command";
            public CommandTooLongException(string command) : base("Command exceeded max command length.")
            {
                Data.Add(CommandKey, command);
            }

            public string Command { get { return (string)Data[CommandKey]; } }
        }
    }
}

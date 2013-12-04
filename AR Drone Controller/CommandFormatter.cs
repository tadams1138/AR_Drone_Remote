namespace AR_Drone_Controller
{
    class CommandFormatter
    {
        private int _seq = 1;
        
        internal virtual string CreateCommand(string type)
        {
            string command = string.Format("AT*{0}={1}\r", type, _seq++);
            return command;
        }
        
        internal virtual string CreateCommand(string type, string message)
        {
            string command = string.Format("AT*{0}={1},{2}\r", type, _seq++, message);
            return command;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Library3d.Error
{
    public delegate void MessageDelegate(string message, int level);
    public delegate void ClearDelegate();

    public class ErrorMessages
    {
        public static ErrorMessages Global = new ErrorMessages();
        List<string> Messages = new List<string>();

        public event MessageDelegate OnMessage;
        public event ClearDelegate OnClear;

        public void AddErrorMessage(string message, int level)
        {
            this.Messages.Add(message);
            if (OnMessage != null)
                OnMessage(message, level);

        }

        public void Clear()
        {
            this.Messages.Clear();
            if (OnClear != null)
                OnClear();

        }
    }
}

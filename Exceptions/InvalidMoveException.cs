using System;

namespace Chess.Exceptions
{
    class InvalidMoveException : Exception
    {
        public InvalidMoveException() : base()
        {
        }

        public InvalidMoveException(string message) : base(message)
        {
        }

        public InvalidMoveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

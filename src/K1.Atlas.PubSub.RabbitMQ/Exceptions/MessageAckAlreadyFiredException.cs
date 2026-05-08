namespace K1.Atlas.PubSub.Rabbit.Exceptions
{
    public class MessageAckAlreadyFiredException : Exception
    {
        public MessageAckAlreadyFiredException()
        {
        }

        public MessageAckAlreadyFiredException(string message) : base(message)
        {
        }
    }
}

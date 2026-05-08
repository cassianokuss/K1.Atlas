namespace K1.Atlas.PubSub.Rabbit.Exceptions
{
    public class InvalidExchangeException : Exception
    {
        public InvalidExchangeException()
            : base("You must specify the exchange using the DefaultExchange config or the options argument")
        {
        }
    }
}

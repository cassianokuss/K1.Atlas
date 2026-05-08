namespace K1.Atlas.PubSub.Consumer;

public class SubscriptionOptions
{
    public bool? AutoAck { get; set; }
    public string? Exchange { get; set; }
    public string? Queue { get; set; }
    public IEnumerable<string>? RoutingKeys { get; set; }
}

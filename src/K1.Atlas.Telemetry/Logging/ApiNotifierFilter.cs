using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace K1.Atlas.Telemetry.Logging;

public class ApiNotifierFilter : IActionFilter
{
    private readonly INotifier _notifier;

    public ApiNotifierFilter(INotifier notifier)
    {
        _notifier = notifier;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (_notifier.HasFailNotification)
        {
            context.Result = new ObjectResult(_notifier.GetNotifications().Select(e =>
            {
                var message = e.Message;
                for (int i = 0; i < e.Args.Length; i++)
                {
                    var idx0 = message.IndexOf("{", 0);
                    var idx1 = message.IndexOf("}", idx0);
                    message = message.Remove(idx0, idx1 - idx0 + 1);
                    message = message.Insert(idx0, e.Args[i]!.ToString()!);
                }

                return message;
            }))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}

using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Api.Library;

/// <summary>
/// 
/// </summary>
public class TokenFilter : IActionFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Do something before the action executes.
        if (context.HttpContext.Request.Headers.Authorization.Count == 0 ||
            context.HttpContext.Request.Headers.Authorization.First().StartsWith("Basic "))
            return;

        //Logger.Info(JsonConvert.SerializeObject(context.HttpContext.Request.Headers.Authorization));
        var ticket = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (ticket != null)
        {
            ticket = ticket.Replace("Bearer ", "");
            if (ticket.Length > 10)
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(ticket);
                if (jwtToken.Payload.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata",
                        out var userData))
                    TokenManager.UserDate = (string)userData;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Do something after the action executes.
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Rewrite;

namespace samplesignalr
{
  public class HttpsRewrite : IRule
  {
    /// <summary>
    /// A path that we should ignore because App Engine hits it multiple
    /// times per second, and it doesn't need to be https.
    /// </summary>
    static PathString s_healthCheckPathString =
        new PathString("/_ah/health");

    /// <summary>
    /// Https requests that arrived via App Engine look like http
    /// (no ssl) requests.  Rewrite them so they look like https requests.
    /// </summary>
    /// <returns>
    /// True if the request was secure.
    /// </returns>
    public static bool Rewrite(HttpRequest request)
    {
      if (request.Scheme == "https")
      {
        return true;  // Already https.
      }
      string proto = request.Headers["X-Forwarded-Proto"]
          .FirstOrDefault();
      if (proto == "https")
      {
        // This request was sent via https from the browser to the
        // App Engine load balancer.  So it's good, but we need to
        // modify the request so that Controllers know it
        // was sent via https.
        request.IsHttps = true;
        request.Scheme = "https";
        return true;
      }
      if (request.Path.StartsWithSegments(s_healthCheckPathString))
      {
        // Accept health checks from non-ssl connections.
        return true;
      }

      return false;
    }

    void IRule.ApplyRule(RewriteContext context)
    {
      var request = context.HttpContext.Request;
      bool wasSecure = Rewrite(request);
      if (!wasSecure)
      {
        // Redirect to https.
        var newUrl = string.Concat(
                            "https://",
                            request.Host.ToUriComponent(),
                            request.PathBase.ToUriComponent(),
                            request.Path.ToUriComponent(),
                            request.QueryString.ToUriComponent());
        var action = new RedirectResult(newUrl);
        // Execute the redirect.
        ActionContext actionContext = new ActionContext()
        {
          HttpContext = context.HttpContext
        };
        action.ExecuteResult(actionContext);
        context.Result = RuleResult.EndResponse;
      }
    }
  }
}
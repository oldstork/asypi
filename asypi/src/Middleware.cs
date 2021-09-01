
namespace Asypi {
    /// <summary>
    /// When attached to routes,
    /// runs after routes are resolved
    /// but before the responder.
    /// <br />
    /// If response chain should continue, middleware should return true.
    /// Otherwise, middleware should return false.
    /// <br />
    /// For example, an authentication middleware may want to,
    /// in the case of an unauthenticated user,
    /// set a redirect URL and response code and
    /// return false to prevent unauthorized user access. 
    /// </summary>
    public delegate bool Middleware(HttpRequest req, HttpResponse res);
}

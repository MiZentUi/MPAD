namespace UI.Extensions;

public static class HttpRequestExtensions
{
    private const string RequestedWithHeader = "x-requested-with";
    private const string XmlHttpRequestValue = "xmlhttprequest";

    public static bool IsAjaxRequest(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return string.Equals(
            request.Headers[RequestedWithHeader].ToString(),
            XmlHttpRequestValue,
            StringComparison.OrdinalIgnoreCase);
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Data;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Network
{
    [DataModelType]
    public enum RequestType
    {
        GET,
        POST,
        PUT,
        HEAD,
        DELETE,
        PATCH,
        OPTIONS,
        TRACE
    }

    [NodeCategory("Obsidian/Network")]
    public class AsyncHttpRequestNode : AsyncActionNode<FrooxEngineContext>
    {
        public ObjectInput<string> RequestUri;
        public ValueInput<RequestType> RequestMethod;
        public ObjectInput<string> RequestHeaders;
        public ObjectInput<string> RequestBody;

        public ObjectOutput<string> ResponseHeaders;
        public ObjectOutput<string> ResponseBody;
        public ValueOutput<HttpStatusCode> StatusCode;

        public AsyncCall OnRequestStart;
        public Continuation OnResponseReceived;
        public Continuation OnError;

        private static readonly HttpClient client = new HttpClient();

        public AsyncHttpRequestNode()
        {
            ResponseHeaders = new ObjectOutput<string>(this);
            ResponseBody = new ObjectOutput<string>(this);
            StatusCode = new ValueOutput<HttpStatusCode>(this);
        }

        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            try
            {
                string requestUri = RequestUri.Evaluate(context);
                RequestType requestMethod = RequestMethod.Evaluate(context);
                string requestHeaders = RequestHeaders.Evaluate(context);
                string requestBody = RequestBody.Evaluate(context);

                UniLog.Log($"Request URI: {requestUri}");
                UniLog.Log($"Request Method: {requestMethod}");
                UniLog.Log($"Request Headers: {requestHeaders}");
                UniLog.Log($"Request Body: {requestBody}");

                if (string.IsNullOrWhiteSpace(requestUri) || (!requestUri.StartsWith("http://") && !requestUri.StartsWith("https://")))
                {
                    ResponseBody.Write("Error in Uri.", context);
                    UniLog.Error("Error in Uri.");
                    return OnError.Target;
                }

                await OnRequestStart.ExecuteAsync(context);

                HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod(requestMethod.ToString()), requestUri);

                if (!string.IsNullOrWhiteSpace(requestHeaders))
                {
                    var headerList = FormatHeaders(requestHeaders);
                    foreach (var header in headerList)
                    {
                        if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value))
                        {
                            if (httpRequest.Content == null)
                            {
                                httpRequest.Content = new System.Net.Http.StringContent(requestBody ?? "", Encoding.UTF8, "application/json");
                            }
                            httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                if (RequestContainsBody(requestMethod))
                {
                    httpRequest.Content = new System.Net.Http.StringContent(requestBody ?? "", Encoding.UTF8, "application/json");
                }

                UniLog.Log("Sending HTTP request...");
                HttpResponseMessage responseMessage = await client.SendAsync(httpRequest);
                UniLog.Log("HTTP request sent successfully.");

                ResponseBody.Write(await responseMessage.Content.ReadAsStringAsync(), context);
                ResponseHeaders.Write(StringifyHeaders(responseMessage.Headers), context);
                StatusCode.Write(responseMessage.StatusCode, context);
                UniLog.Log($"Response Status Code: {responseMessage.StatusCode}");
                UniLog.Log($"Response Headers: {StringifyHeaders(responseMessage.Headers)}");
                UniLog.Log($"Response Body: {await responseMessage.Content.ReadAsStringAsync()}");

                return OnResponseReceived.Target;
            }
            catch (Exception ex)
            {
                ResponseBody.Write(ex.ToString(), context);
                UniLog.Error(ex.ToString());
                return OnError.Target;
            }
        }

        private Dictionary<string, string> FormatHeaders(string headers)
        {
            var headerList = new Dictionary<string, string>();
            var lines = headers.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    headerList[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return headerList;
        }

        private string StringifyHeaders(HttpHeaders headers)
        {
            var sb = new StringBuilder();
            foreach (var header in headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            return sb.ToString();
        }

        private bool RequestContainsBody(RequestType method)
        {
            switch (method)
            {
                case RequestType.POST:
                case RequestType.PUT:
                case RequestType.DELETE:
                case RequestType.PATCH:
                    return true;
                default:
                    return false;
            }
        }
    }
}

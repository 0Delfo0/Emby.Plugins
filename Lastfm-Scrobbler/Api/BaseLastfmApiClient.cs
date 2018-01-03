using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lastfm.Api.Model.Requests;
using Lastfm.Api.Model.Responses;
using Lastfm.Resources;
using Lastfm.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace Lastfm.Api
{
    public class BaseLastfmApiClient
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;

        protected BaseLastfmApiClient(IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }

        protected async Task<TResponse> Post<TRequest, TResponse>(TRequest request) where TRequest : BaseAuthedRequest where TResponse : BaseResponse
        {
            var data = request.ToDictionary();
            //Append the signature
            Helpers.AppendSignature(ref data);

            var options = new HttpRequestOptions
            {
                Url = BuildPostUrl(request.Secure),
                ResourcePool = Plugin.LastfmResourcePool,
                RequestContentType = "application/json",
                CancellationToken = CancellationToken.None,
                TimeoutMs = 120000,
                LogErrorResponseBody = false,
                LogRequest = true,
                BufferContent = false,
                EnableHttpCompression = false,
                EnableKeepAlive = false
            };

            options.SetPostData(data);

            using(var httpResponseInfo = await _httpClient.Post(options))
            {
                try
                {
                    var result = _jsonSerializer.DeserializeFromStream<TResponse>(httpResponseInfo.Content);

                    //Lets Log the error here to ensure all errors are logged
                    if(result.IsError())
                    {
                        Plugin.Logger.Error(result.message);
                    }

                    return result;
                }
                catch(Exception e)
                {
                    Plugin.Logger.Debug(e.Message);
                }

                return null;
            }
        }

        protected async Task<TResponse> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            return await Get<TRequest, TResponse>(request, CancellationToken.None);
        }

        protected async Task<TResponse> Get<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            using(var stream = await _httpClient.Get(new HttpRequestOptions
            {
                Url = BuildGetUrl(request.ToDictionary()),
                ResourcePool = Plugin.LastfmResourcePool,
                CancellationToken = cancellationToken,
                EnableHttpCompression = false
            }))
            {
                try
                {
                    var result = _jsonSerializer.DeserializeFromStream<TResponse>(stream);

                    //Lets Log the error here to ensure all errors are logged
                    if(result.IsError())
                    {
                        Plugin.Logger.Error(result.message);
                    }

                    return result;
                }
                catch(Exception e)
                {
                    Plugin.Logger.Debug(e.Message);
                }

                return null;
            }
        }

        private static string BuildGetUrl(Dictionary<string, string> requestData)
        {
            return string.Format("http://{0}/{1}/?format=json&{2}",
                PluginConst.LasfmApi.LastfmBaseUrl,
                PluginConst.LasfmApi.ApiVersion,
                Helpers.DictionaryToQueryString(requestData)
            );
        }

        private static string BuildPostUrl(bool secure = false)
        {
            return string.Format("{0}://{1}/{2}/?format=json",
                secure ? "https" : "http",
                PluginConst.LasfmApi.LastfmBaseUrl,
                PluginConst.LasfmApi.ApiVersion
            );
        }
    }
}
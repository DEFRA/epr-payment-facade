﻿using EPR.Payment.Facade.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EPR.Payment.Facade.Common.RESTServices
{
    public abstract class BaseHttpService
    {
        protected readonly string _baseUrl;
        protected readonly HttpClient _httpClient;
        protected IHttpContextAccessor _httpContextAccessor;

        protected BaseHttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, string baseUrl, string endPointName)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(endPointName))
            {
                throw new ArgumentNullException(nameof(endPointName));
            }

            _baseUrl = baseUrl.EndsWith('/') ? baseUrl.TrimEnd('/') : baseUrl;

            _baseUrl = $"{_baseUrl}/{endPointName}";

            // Add common headers
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        protected void SetBearerToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Performs an Http GET returning the specified object
        /// </summary>
        protected virtual async Task<T> Get<T>(string url, CancellationToken cancellationToken, bool includeTrailingSlash = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = _baseUrl;
            }
            else
            {
                if (includeTrailingSlash)
                {
                    url = $"{_baseUrl}/{url}/";
                }
                else
                {
                    url = $"{_baseUrl}/{url}";
                }
            }

            return await Send<T>(CreateMessage(url, null, HttpMethod.Get), cancellationToken);
        }

        private string ReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            return $"{_baseUrl}/{url}/";
        }

        /// <summary>
        /// Performs an Http POST returning the specified object
        /// </summary>
        protected virtual async Task<T> Post<T>(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            return await Send<T>(CreateMessage(url, payload, HttpMethod.Post), cancellationToken);
        }

        /// <summary>
        /// Performs an Http POST without returning any data
        /// </summary>
        protected async Task Post(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            await Send(CreateMessage(url, payload, HttpMethod.Post), cancellationToken);
        }

        /// <summary>
        /// Performs an Http PUT returning the specified object
        /// </summary>
        protected async Task<T> Put<T>(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            return await Send<T>(CreateMessage(url, payload, HttpMethod.Put), cancellationToken);
        }

        /// <summary>
        /// Performs an Http PUT without returning any data
        /// </summary>
        protected async Task Put(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            await Send(CreateMessage(url, payload, HttpMethod.Put), cancellationToken);
        }

        /// <summary>
        /// Performs an Http DELETE returning the specified object
        /// </summary>
        protected async Task<T> Delete<T>(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            return await Send<T>(CreateMessage(url, payload, HttpMethod.Delete), cancellationToken);
        }

        /// <summary>
        /// Performs an Http DELETE without returning any data
        /// </summary>
        protected async Task Delete(string url, object? payload, CancellationToken cancellationToken)
        {
            url = ReturnUrl(url);
            await Send(CreateMessage(url, payload, HttpMethod.Delete), cancellationToken);
        }

        private static HttpRequestMessage CreateMessage(
            string url,
            object? payload,
            HttpMethod httpMethod)
        {
            var msg = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = httpMethod
            };

            if (payload != null)
            {
                msg.Content = JsonContent.Create(payload);
            }

            return msg;
        }

        private async Task<T> Send<T>(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var streamReader = new StreamReader(responseStream);
                var content = await streamReader.ReadToEndAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                    return default!;

                return ReturnValue<T>(content);
            }
            else
            {
                // get any message from the response
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var content = default(string);

                if (responseStream.Length > 0)
                {
                    using var streamReader = new StreamReader(responseStream);
                    content = await streamReader.ReadToEndAsync(cancellationToken);
                }

                // set the response status code and throw the exception for the middleware to handle
                throw new ResponseCodeException(response.StatusCode, content!);
            }
        }

        private async Task Send(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)response.StatusCode;
                // for now we don't know how we're going to handle errors specifically,
                // so we'll just throw an error with the error code
                throw new ServiceException($"Error occurred calling API with error code: {response.StatusCode}. Message: {response.ReasonPhrase}");
            }
        }

        private static T ReturnValue<T>(string value)
        {
            if (IsValidJson(value))
                return JsonConvert.DeserializeObject<T>(value)!;
            else
                return (T)Convert.ChangeType(value, typeof(T));
        }

        private static bool IsValidJson(string stringValue)
        {
            try
            {
                JToken.Parse(stringValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

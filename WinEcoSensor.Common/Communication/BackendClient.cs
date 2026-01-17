// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Communication
{
    /// <summary>
    /// HTTP client for communicating with the backend server.
    /// Posts CloudEvent messages to the configured endpoint.
    /// </summary>
    public class BackendClient : IDisposable
    {
        // Default timeout in seconds
        private const int DefaultTimeoutSeconds = 30;

        // CloudEvents content type (JSON for FastAPI compatibility)
        private const string CloudEventsContentType = "application/json";

        // Event endpoint path
        private const string EventEndpoint = "/event";

        // Retry settings
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        private string _backendUrl;
        private int _timeoutSeconds;
        private bool _disposed;
        private DateTime _lastSuccessfulSend;
        private int _consecutiveFailures;

        /// <summary>
        /// Backend server URL
        /// </summary>
        public string BackendUrl
        {
            get => _backendUrl;
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    _backendUrl = value;
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    _backendUrl = value.TrimEnd('/');
                }
                else
                {
                    _backendUrl = value;
                }
            }
        }

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set => _timeoutSeconds = value > 0 ? value : DefaultTimeoutSeconds;
        }

        /// <summary>
        /// Whether the backend is available (based on recent communication)
        /// </summary>
        public bool IsBackendAvailable => _consecutiveFailures < 5;

        /// <summary>
        /// Last successful send time
        /// </summary>
        public DateTime LastSuccessfulSend => _lastSuccessfulSend;

        /// <summary>
        /// Number of consecutive failures
        /// </summary>
        public int ConsecutiveFailures => _consecutiveFailures;

        /// <summary>
        /// Create backend client with default settings
        /// </summary>
        public BackendClient()
        {
            _timeoutSeconds = DefaultTimeoutSeconds;
            _consecutiveFailures = 0;
            
            // Enable TLS 1.2 for .NET 4.0+
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch
            {
                // TLS 1.2 might not be available on older systems
            }
        }

        /// <summary>
        /// Create backend client with URL
        /// </summary>
        /// <param name="backendUrl">Backend server URL</param>
        public BackendClient(string backendUrl) : this()
        {
            BackendUrl = backendUrl;
        }

        /// <summary>
        /// Send CloudEvent message to backend
        /// </summary>
        /// <param name="message">CloudEvent message</param>
        /// <returns>True if sent successfully</returns>
        public bool SendEvent(CloudEventMessage message)
        {
            if (_disposed)
            {
                Logger.Warning("Cannot send event: BackendClient has been disposed");
                return false;
            }

            if (message == null)
            {
                Logger.Warning("Cannot send null message");
                return false;
            }

            if (string.IsNullOrEmpty(_backendUrl))
            {
                Logger.Warning("Backend URL not configured");
                return false;
            }

            string json = SerializeMessage(message);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            // Append /event endpoint to backend URL
            string eventUrl = _backendUrl + EventEndpoint;
            return SendWithRetry(eventUrl, json);
        }

        /// <summary>
        /// Send CloudEvent message to backend asynchronously
        /// </summary>
        /// <param name="message">CloudEvent message</param>
        /// <returns>True if sent successfully</returns>
        public Task<bool> SendEventAsync(CloudEventMessage message)
        {
            return Task.Run(() => SendEvent(message));
        }

        /// <summary>
        /// Send JSON payload with retry logic
        /// </summary>
        private bool SendWithRetry(string url, string json)
        {
            int attempt = 0;
            while (attempt < MaxRetries)
            {
                try
                {
                    var request = CreateRequest(url);
                    WriteRequestBody(request, json);

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.Accepted ||
                            response.StatusCode == HttpStatusCode.Created ||
                            response.StatusCode == HttpStatusCode.NoContent)
                        {
                            _lastSuccessfulSend = DateTime.UtcNow;
                            _consecutiveFailures = 0;
                            Logger.Debug($"Event sent successfully to {url}");
                            return true;
                        }

                        Logger.Warning($"Unexpected response status: {response.StatusCode}");
                    }
                }
                catch (WebException ex)
                {
                    HandleWebException(ex, attempt);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error sending event (attempt {attempt + 1}/{MaxRetries})", ex);
                }

                attempt++;
                if (attempt < MaxRetries)
                {
                    Thread.Sleep(RetryDelayMs * attempt);
                }
            }

            _consecutiveFailures++;
            Logger.Warning($"Failed to send event after {MaxRetries} attempts. Consecutive failures: {_consecutiveFailures}");
            return false;
        }

        /// <summary>
        /// Create HTTP request
        /// </summary>
        private HttpWebRequest CreateRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = CloudEventsContentType;
            request.Accept = "application/json";
            request.Timeout = _timeoutSeconds * 1000;
            request.ReadWriteTimeout = _timeoutSeconds * 1000;
            request.UserAgent = "WinEcoSensor/1.0";

            return request;
        }

        /// <summary>
        /// Write request body
        /// </summary>
        private void WriteRequestBody(HttpWebRequest request, string json)
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Handle WebException with detailed logging
        /// </summary>
        private void HandleWebException(WebException ex, int attempt)
        {
            if (ex.Response is HttpWebResponse response)
            {
                Logger.Warning($"HTTP error {(int)response.StatusCode} (attempt {attempt + 1}/{MaxRetries}): {response.StatusDescription}");
                
                // Try to read error body
                try
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string errorBody = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(errorBody))
                        {
                            Logger.Debug($"Error response: {errorBody}");
                        }
                    }
                }
                catch { }
            }
            else
            {
                Logger.Warning($"Network error (attempt {attempt + 1}/{MaxRetries}): {ex.Status} - {ex.Message}");
            }
        }

        /// <summary>
        /// Serialize CloudEvent message to JSON
        /// </summary>
        private string SerializeMessage(CloudEventMessage message)
        {
            try
            {
                // Simple JSON serialization without external dependencies
                var sb = new StringBuilder();
                sb.Append("{");
                sb.AppendFormat("\"specversion\":\"{0}\",", EscapeJson(message.SpecVersion));
                sb.AppendFormat("\"type\":\"{0}\",", EscapeJson(message.Type));
                sb.AppendFormat("\"source\":\"{0}\",", EscapeJson(message.Source));
                sb.AppendFormat("\"id\":\"{0}\",", EscapeJson(message.Id));
                sb.AppendFormat("\"time\":\"{0}\",", EscapeJson(message.Time));
                sb.AppendFormat("\"datacontenttype\":\"{0}\"", EscapeJson(message.DataContentType));

                if (!string.IsNullOrEmpty(message.Subject))
                {
                    sb.AppendFormat(",\"subject\":\"{0}\"", EscapeJson(message.Subject));
                }

                if (message.Data != null)
                {
                    sb.Append(",\"data\":");
                    if (message.Data is CloudEventData cloudData)
                    {
                        SerializeData(sb, cloudData);
                    }
                    else
                    {
                        sb.Append("{}");
                    }
                }

                sb.Append("}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error("Error serializing message", ex);
                return null;
            }
        }

        /// <summary>
        /// Serialize CloudEventData to JSON
        /// </summary>
        private void SerializeData(StringBuilder sb, CloudEventData data)
        {
            sb.Append("{");
            bool first = true;

            if (!string.IsNullOrEmpty(data.HardwareId))
            {
                AppendJsonProperty(sb, ref first, "hardwareId", data.HardwareId);
            }

            AppendJsonProperty(sb, ref first, "timestamp", data.Timestamp.ToString("o"));

            if (data.Hardware != null)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"hardware\":");
                SerializeDictionary(sb, data.Hardware);
            }

            if (data.UserActivity != null)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"userActivity\":");
                SerializeDictionary(sb, data.UserActivity);
            }

            if (data.Energy != null)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"energy\":");
                SerializeDictionary(sb, data.Energy);
            }

            if (data.DisplayTime != null)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"displayTime\":");
                SerializeDictionary(sb, data.DisplayTime);
            }

            if (!string.IsNullOrEmpty(data.ServiceVersion))
            {
                AppendJsonProperty(sb, ref first, "serviceVersion", data.ServiceVersion);
            }

            if (!string.IsNullOrEmpty(data.SummaryDate))
            {
                AppendJsonProperty(sb, ref first, "summaryDate", data.SummaryDate);
            }

            sb.Append("}");
        }

        /// <summary>
        /// Serialize dictionary to JSON
        /// </summary>
        private void SerializeDictionary(StringBuilder sb, System.Collections.Generic.Dictionary<string, object> dict)
        {
            sb.Append("{");
            bool first = true;

            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",");
                first = false;

                sb.AppendFormat("\"{0}\":", EscapeJson(kvp.Key));
                SerializeValue(sb, kvp.Value);
            }

            sb.Append("}");
        }

        /// <summary>
        /// Serialize a value to JSON
        /// </summary>
        private void SerializeValue(StringBuilder sb, object value)
        {
            if (value == null)
            {
                sb.Append("null");
            }
            else if (value is string s)
            {
                sb.AppendFormat("\"{0}\"", EscapeJson(s));
            }
            else if (value is bool b)
            {
                sb.Append(b ? "true" : "false");
            }
            else if (value is int || value is long || value is short)
            {
                sb.Append(value);
            }
            else if (value is double d)
            {
                sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is float f)
            {
                sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is decimal dec)
            {
                sb.Append(dec.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is System.Collections.Generic.Dictionary<string, object> dict)
            {
                SerializeDictionary(sb, dict);
            }
            else if (value is System.Collections.IList list)
            {
                sb.Append("[");
                bool first = true;
                foreach (var item in list)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    SerializeValue(sb, item);
                }
                sb.Append("]");
            }
            else
            {
                sb.AppendFormat("\"{0}\"", EscapeJson(value.ToString()));
            }
        }

        /// <summary>
        /// Append JSON property with proper comma handling
        /// </summary>
        private void AppendJsonProperty(StringBuilder sb, ref bool first, string name, string value)
        {
            if (!first) sb.Append(",");
            first = false;
            sb.AppendFormat("\"{0}\":\"{1}\"", EscapeJson(name), EscapeJson(value));
        }

        /// <summary>
        /// Escape string for JSON
        /// </summary>
        private string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            var sb = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Test connection to backend using /health endpoint
        /// </summary>
        /// <returns>Connection test result</returns>
        public ConnectionTestResult TestConnection()
        {
            var result = new ConnectionTestResult
            {
                Url = _backendUrl,
                TestedAt = DateTime.UtcNow
            };

            if (_disposed)
            {
                result.Success = false;
                result.ErrorMessage = "BackendClient has been disposed";
                return result;
            }

            if (string.IsNullOrEmpty(_backendUrl))
            {
                result.Success = false;
                result.ErrorMessage = "Backend URL not configured";
                return result;
            }

            try
            {
                // Use /health endpoint for connection test
                string healthUrl = _backendUrl + "/health";
                var request = (HttpWebRequest)WebRequest.Create(healthUrl);
                request.Method = "GET";
                request.Accept = "application/json";
                request.Timeout = 10000;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    stopwatch.Stop();
                    result.Success = true;
                    result.StatusCode = (int)response.StatusCode;
                    result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

                    // Read health response
                    try
                    {
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string body = reader.ReadToEnd();
                            Logger.Debug($"Health check response: {body}");
                        }
                    }
                    catch { }
                }
            }
            catch (WebException ex)
            {
                result.Success = false;
                if (ex.Response is HttpWebResponse response)
                {
                    result.StatusCode = (int)response.StatusCode;
                    result.ErrorMessage = $"HTTP {result.StatusCode}: {response.StatusDescription}";
                }
                else
                {
                    result.ErrorMessage = $"{ex.Status}: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Reset failure counter (e.g., after configuration change)
        /// </summary>
        public void ResetFailureCounter()
        {
            _consecutiveFailures = 0;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Result of a connection test
    /// </summary>
    public class ConnectionTestResult
    {
        /// <summary>
        /// URL that was tested
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether the test was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code (if applicable)
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public long ResponseTimeMs { get; set; }

        /// <summary>
        /// Error message (if failed)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// When the test was performed
        /// </summary>
        public DateTime TestedAt { get; set; }
    }
}

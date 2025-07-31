/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Godot;
using Newtonsoft.Json;
using LllmNpcConversationSystem.Services.Types;

namespace LllmNpcConversationSystem.Services
{
    /// <summary>
    /// Service class for interacting with the Ollama API for AI chat responses.
    /// Implements a thread-safe singleton pattern.
    /// </summary>
    public class OllamaService
    {
        // Singleton instance of OllamaService.
        private static OllamaService _instance;

        // Lock object for thread-safe singleton initialization.
        private static readonly object _lock = new object();

        // HttpClient used for sending requests to Ollama.
        private System.Net.Http.HttpClient _httpClient;

        // Host URL for the Ollama API.
        private string _ollamaHost = "";

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// Initializes the HttpClient with custom settings.
        /// </summary>
        private OllamaService()
        {
            _httpClient = new System.Net.Http.HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        /// <summary>
        /// Gets the singleton instance of OllamaService.
        /// </summary>
        public static OllamaService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new OllamaService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sets the host URL for the Ollama API.
        /// </summary>
        /// <param name="host">The base URL of the Ollama API.</param>
        public void SetHost(string host)
        {
            _ollamaHost = host;
        }

        /// <summary>
        /// Sends a chat request to the Ollama API and streams the AI response.
        /// </summary>
        /// <param name="messages">List of chat messages to send.</param>
        /// <param name="model">Model name to use for the chat.</param>
        /// <param name="responseFormat">Optional response format specification.</param>
        /// <returns>Async stream of FetchAiResponse objects containing reply data.</returns>
        public async IAsyncEnumerable<FetchAiResponse> FetchAIResponseAsync(List<Message> messages, string model, object responseFormat = null)
        {
            var request = new ChatRequest
            {
                Model = model,
                Messages = messages,
                Format = responseFormat,
                Stream = true,
                KeepAlive = "60m"
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            GD.Print("OllamaService: Sending request to Ollama...");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_ollamaHost}/api/chat")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                GD.PrintErr($"OllamaService: HTTP Error - {response.StatusCode}: {errorContent}");

                yield return new FetchAiResponse
                {
                    Success = false,
                    Error = $"HTTP {response.StatusCode}: {errorContent}"
                };
                yield break;
            }

            var fullReply = "";
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(line);

                if (chatResponse?.Message != null)
                {
                    var newContent = chatResponse.Message.Content;
                    fullReply += newContent;

                    StructuredResponse structuredData = null;

                    if (chatResponse.Done && !string.IsNullOrWhiteSpace(fullReply))
                    {
                        try
                        {
                            structuredData = JsonConvert.DeserializeObject<StructuredResponse>(fullReply);
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr($"Failed to parse structured response: {ex.Message}");
                            GD.PrintErr($"Raw response: {fullReply}");
                        }
                    }

                    yield return new FetchAiResponse
                    {
                        Success = true,
                        Reply = fullReply,
                        Final = chatResponse.Done,
                        StructuredData = structuredData
                    };

                    if (chatResponse.Done)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Aborts any pending HTTP requests to the Ollama API.
        /// </summary>
        public void AbortCurrentRequest()
        {
            _httpClient?.CancelPendingRequests();
        }

        /// <summary>
        /// Disposes the HttpClient and releases resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
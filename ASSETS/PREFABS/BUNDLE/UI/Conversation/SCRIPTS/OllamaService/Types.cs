/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LllmNpcConversationSystem.Services.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageType
    {
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "system")]
        System,
        [EnumMember(Value = "assistant")]
        Assistant
    }
    public class Message
    {
        [JsonProperty("role")]
        public MessageType Role { get; set; }
        
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class FetchAiResponse
    {
        public bool Success { get; set; }
        public string Reply { get; set; }
        public bool Final { get; set; }
        public string Error { get; set; }
        public StructuredResponse StructuredData { get; set; }
    }

    public class ChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }

        [JsonProperty("format")]
        public object Format { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; } = true;

        [JsonProperty("keep_alive")]
        public string KeepAlive { get; set; } = "60m";
    }

    public class ChatResponse
    {
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }
    }

    public class StructuredResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("signal")]
        public SignalData Signal { get; set; }
    }

    public class SignalData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
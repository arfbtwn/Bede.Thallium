using System;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Handlers
{
    public static class HandlerBuilder
    {
        public static ThrowOnFail ThrowOnFail(this HttpMessageHandler @this)
        {
            return new ThrowOnFail(@this);
        }

        public static ThrowOnFail ThrowOnFail(this ThrowOnFail @this)
        {
            return @this;
        }

        public static RecordingHandler Record(this HttpMessageHandler @this)
        {
            return new RecordingHandler(@this);
        }

        public static RecordingHandler Record(this RecordingHandler @this)
        {
            return @this;
        }

        public static RecordingHandler OnRequest(this RecordingHandler @this, EventHandler<HttpRequestMessage> handler)
        {
            @this.Request += handler;

            return @this;
        }

        public static RecordingHandler OnResponse(this RecordingHandler @this, EventHandler<HttpResponseMessage> handler)
        {
            @this.Response += handler;

            return @this;
        }
    }
}

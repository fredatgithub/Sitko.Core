using System;
using System.Threading.Tasks;
using Sitko.Core.Queue.Exceptions;
using Sitko.Core.Queue.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Sitko.Core.Queue.Nats.Tests
{
    public class
        BasicQueueTests : BasicQueueTests<NatsQueueTestScope, NatsQueueModule, NatsQueue, NatsQueueModuleConfig>
    {
        public BasicQueueTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        
        [Fact]
        public async Task RequestResponseTimeout()
        {
            var scope = GetScope();

            var queue = scope.Get<IQueue>();

            var msg = new TestMessage();
            var timeout = TimeSpan.FromMilliseconds(1);
            var subResult = await queue.ReplyAsync<TestMessage, TestResponse>((message, context) =>
                Task.FromResult(new TestResponse {Id = message.Id}));
            Assert.True(subResult.IsSuccess);
            var ex = await Assert.ThrowsAsync<QueueRequestTimeoutException>(() =>
                queue.RequestAsync<TestMessage, TestResponse>(msg, null, timeout));

            Assert.Equal(timeout, ex.Timeout);
        }
    }
}
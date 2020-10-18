using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Domain;
using Archimedes.Library.Extensions;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Archimedes.Service.Repository.Tests
{
    [TestFixture]
    public class PriceClientTests
    {
        //https://stackoverflow.com/questions/36425008/mocking-httpclient-in-unit-tests
        [Test]
        public void Should_NotThrowException_WhenPostingPrices()
        {
            var subject = GetSubjectUnderTest(HttpStatusCode.Accepted);

            Assert.DoesNotThrow(() => subject.Post(MockPriceResponse()));
        }

        private static IMessageClient GetSubjectUnderTest(HttpStatusCode statusCode)
        {
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<MessageClient>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var mockConfig = new Mock<IOptions<Config>>();
            var mockConfigItem = new Config() {ApiRepositoryUrl = "https://www.something"};

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new JsonContent(MockPriceResponse())
                });

            mockConfig.Setup(a => a.Value).Returns(mockConfigItem);

            var client = new System.Net.Http.HttpClient(mockHttpMessageHandler.Object);
            mockFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(client);

            return new MessageClient(mockConfig.Object, client, mockLogger.Object);
        }

        private static PriceMessage MockPriceResponse()
        {
            var priceResponse = new PriceMessage()
            {

                Text = "TestText",
                Prices = new List<PriceDto>() { new PriceDto() { Market = "TestGBPUSD" } }
            };

            return priceResponse;
        }
    }
}
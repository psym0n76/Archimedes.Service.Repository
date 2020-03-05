using System.Collections.Generic;
using System.Net.Http;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Archimedes.Service.Repository.Tests
{
    [TestFixture]
    public class HttpClientRequestTests
    {
        [Test]
        public void Should_NotThrowException_WhenPostingPrices()
        {
            //var mockConfig = new Mock<IOptions<Config>>();
            //var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            //var mockLogger = new Mock<ILogger<HttpClientRequest>>();

            //var priceResponse = new PriceResponse()
            //{
            //    Status = "TestStatus", Text = "TestText",
            //    Payload = new List<PriceDto>() {new PriceDto() {Market = "TestGBPUSD"}}
            //};

            //mockHttpClientFactory.Setup(m => m.CreateClient());

            //var subject = new HttpClientRequest(mockConfig.Object, mockHttpClientFactory.Object,mockLogger.Object);

            //subject.PostPrice(priceResponse);

            //Assert.DoesNotThrow(()=>subject.PostPrice(priceResponse));
        }
    }
}
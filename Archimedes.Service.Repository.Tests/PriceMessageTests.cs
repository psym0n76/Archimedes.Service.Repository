using System.Collections.Generic;
using Archimedes.Library.Domain;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Archimedes.Service.Repository.Tests
{
    [TestFixture]
    public class PriceMessageTests
    {
        [Test]
        public void Should_InvokePostPrice_WhenPriceMessageIsReceived()
        {
            var mockHttpClientRequest = new Mock<IClient>();
            var mockLogger = new Mock<ILogger>();

            var config = new Config();
            var subject = new PriceMessageProcessor();
            var priceResponse = new PriceMessage()
            {
                Text = "TestTest",
                Prices = new List<PriceDto>(){new PriceDto(){Market = "GBP/USD"}}
            };

            mockHttpClientRequest.Setup(m => m.Post(priceResponse));

            try
            {
                subject.Process(priceResponse, mockHttpClientRequest.Object, mockLogger.Object, config);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        [Test]
        public void Should_NotThrowException_WhenPriceMessageIsReceived()
        {
            var mockHttpClientRequest = new Mock<IClient>();
            var mockLogger = new Mock<ILogger>();
            var config = new Config();
            var subject = new PriceMessageProcessor();

            var priceResponse = new PriceMessage()
            {
                Text = "TestText",
                Prices = new List<PriceDto>(){new PriceDto(){Market = "GBP/USD"}}
            };

            mockHttpClientRequest.Setup(m => m.Post(priceResponse));

            Assert.DoesNotThrow(() =>
                {
                    subject.Process(priceResponse, mockHttpClientRequest.Object, mockLogger.Object, config);
                });

        }
    }
}
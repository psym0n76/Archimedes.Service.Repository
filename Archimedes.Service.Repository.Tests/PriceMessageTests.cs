﻿using System;
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
            var mockHttpClientRequest = new Mock<IHttpClientRequest>();
            var mockLogger = new Mock<ILogger>();

            var config = new Config();
            var subject = new PriceMessage();
            var priceResponse = new PriceResponse()
            {
                Status = "TestStatus", Text = "TestText",
                Payload = new List<PriceDto>() {new PriceDto() {Market = "TestGBPUSD"}}
            };

            mockHttpClientRequest.Setup(m => m.PostPrice(priceResponse));

            try
            {
                subject.Process(priceResponse, mockHttpClientRequest.Object, mockLogger.Object, config);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false);
            }
        }

        [Test]
        public void Should_NotThrowException_WhenPriceMessageIsReceived()
        {
            var mockHttpClientRequest = new Mock<IHttpClientRequest>();
            var mockLogger = new Mock<ILogger>();
            var config = new Config();
            var subject = new PriceMessage();

            var priceResponse = new PriceResponse()
            {
                Status = "TestStatus", Text = "TestText",
                Payload = new List<PriceDto>() {new PriceDto() {Market = "TestGBPUSD"}}
            };

            mockHttpClientRequest.Setup(m => m.PostPrice(priceResponse));

            Assert.DoesNotThrow(() =>
                {
                    subject.Process(priceResponse, mockHttpClientRequest.Object, mockLogger.Object, config);
                });

        }
    }
}
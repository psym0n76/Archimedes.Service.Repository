﻿using Archimedes.Library.Message;
using NUnit.Framework;

namespace Archimedes.Service.Repository.Tests
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void Should_ReturnPriceMessage_WhenInputPriceResponseReceived()
        {
            var message = new PriceResponse();
            var subject = MessageHandlerFactory.Get(message);

            Assert.IsNotNull(subject);
            Assert.IsInstanceOf<PriceMessage>(subject);
        }

        [Test]
        public void Should_ReturnNull_WhenInputUnknownTypeReceived()
        {
            var message = string.Empty;
            var subject = MessageHandlerFactory.Get(message);
            Assert.IsNull(subject);
        }
    }
}
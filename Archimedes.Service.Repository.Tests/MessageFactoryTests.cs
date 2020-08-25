using Archimedes.Library.Message;
using NUnit.Framework;

namespace Archimedes.Service.Repository.Tests
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void Should_ReturnPriceMessage_WhenInputPriceResponseReceived()
        {
            var message = new PriceMessage();
            var subject = MessageHandlerFactory.Get(message);

            Assert.IsNotNull(subject);
            Assert.IsInstanceOf<PriceMessageProcessor>(subject);
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
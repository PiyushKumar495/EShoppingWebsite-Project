using NUnit.Framework;
using EShop.Dtos;
using EShop.Models;
using System.Collections.Generic;

namespace EShop.Tests
{
    [TestFixture]
    public class DtoTests
    {
        [Test]
        public void OrderItemResponseDto_Properties_SetCorrectly()
        {
            var dto = new OrderItemResponseDto
            {
                OrderItemId = 1,
                ProductId = 100,
                ProductName = "Test Product",
                Quantity = 5,
                Price = 99.99m
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.OrderItemId, Is.EqualTo(1));
                Assert.That(dto.ProductId, Is.EqualTo(100));
                Assert.That(dto.ProductName, Is.EqualTo("Test Product"));
                Assert.That(dto.Quantity, Is.EqualTo(5));
                Assert.That(dto.Price, Is.EqualTo(99.99m));
            });
        }

        [Test]
        public void OrderResponseDto_Properties_SetCorrectly()
        {
            var items = new List<OrderItemResponseDto>
            {
                new OrderItemResponseDto { OrderItemId = 1, ProductId = 1, Quantity = 2, Price = 50 }
            };
            var payment = new PaymentResponseDto { PaymentId = 1, Amount = 100 };

            var dto = new OrderResponseDto
            {
                OrderId = 1,
                OrderDate = System.DateTime.Now,
                TotalAmount = 100.50m,
                Status = "Pending",
                ShippingAddress = "123 Test St",
                UserName = "Test User",
                Items = items,
                PaymentMethod = PaymentMethod.COD,
                Payment = payment
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.OrderId, Is.EqualTo(1));
                Assert.That(dto.TotalAmount, Is.EqualTo(100.50m));
                Assert.That(dto.Status, Is.EqualTo("Pending"));
                Assert.That(dto.ShippingAddress, Is.EqualTo("123 Test St"));
                Assert.That(dto.UserName, Is.EqualTo("Test User"));
                Assert.That(dto.Items, Is.EqualTo(items));
                Assert.That(dto.PaymentMethod, Is.EqualTo(PaymentMethod.COD));
                Assert.That(dto.Payment, Is.EqualTo(payment));
            });
        }

        [Test]
        public void OrderResponseDto_CanHandleNullValues()
        {
            var dto = new OrderResponseDto
            {
                OrderId = 1,
                Status = null,
                UserName = null,
                Items = null,
                Payment = null
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Status, Is.Null);
                Assert.That(dto.UserName, Is.Null);
                Assert.That(dto.Items, Is.Null);
                Assert.That(dto.Payment, Is.Null);
            });
        }
    }
}
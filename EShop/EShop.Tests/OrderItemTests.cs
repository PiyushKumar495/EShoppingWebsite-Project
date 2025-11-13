using NUnit.Framework;
using EShop.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace EShop.Tests
{
    [TestFixture]
    public class OrderItemTests
    {
        [Test]
        public void OrderItem_Properties_SetCorrectly()
        {
            var orderItem = new OrderItem
            {
                OrderItemId = 1,
                OrderId = 100,
                ProductId = 200,
                Quantity = 5,
                Price = 99.99m
            };

            Assert.Multiple(() =>
            {
                Assert.That(orderItem.OrderItemId, Is.EqualTo(1));
                Assert.That(orderItem.OrderId, Is.EqualTo(100));
                Assert.That(orderItem.ProductId, Is.EqualTo(200));
                Assert.That(orderItem.Quantity, Is.EqualTo(5));
                Assert.That(orderItem.Price, Is.EqualTo(99.99m));
            });
        }

        [Test]
        public void OrderItem_ValidationAttributes_Work()
        {
            var orderItem = new OrderItem
            {
                OrderId = 0,
                ProductId = 0,
                Quantity = 0,
                Price = 0
            };

            var context = new ValidationContext(orderItem);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(orderItem, context, results, true);

            Assert.Multiple(() =>
            {
                Assert.That(isValid, Is.False);
                Assert.That(results, Has.Count.GreaterThan(0));
            });
        }

        [Test]
        public void OrderItem_NavigationProperties_CanBeSet()
        {
            var order = new Order { OrderId = 1 };
            var product = new Product { ProductId = 1, Name = "Test" };
            var orderItem = new OrderItem
            {
                Order = order,
                Product = product
            };

            Assert.Multiple(() =>
            {
                Assert.That(orderItem.Order, Is.EqualTo(order));
                Assert.That(orderItem.Product, Is.EqualTo(product));
            });
        }
    }
}
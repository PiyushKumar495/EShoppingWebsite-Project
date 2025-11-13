using NUnit.Framework;
using EShop.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EShop.Tests
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void Cart_Properties_SetCorrectly()
        {
            var cart = new Cart
            {
                CartId = 1,
                UserId = 100
            };

            Assert.Multiple(() =>
            {
                Assert.That(cart.CartId, Is.EqualTo(1));
                Assert.That(cart.UserId, Is.EqualTo(100));
                Assert.That(cart.CartItems, Is.Not.Null);
            });
        }

        [Test]
        public void Cart_NavigationProperties_Work()
        {
            var user = new User { UserId = 1, FullName = "Test" };
            var cartItem = new CartItem { CartItemId = 1 };
            var cart = new Cart
            {
                User = user,
                CartItems = [cartItem]
            };

            Assert.Multiple(() =>
            {
                Assert.That(cart.User, Is.EqualTo(user));
                Assert.That(cart.CartItems, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void Order_Properties_SetCorrectly()
        {
            var order = new Order
            {
                OrderId = 1,
                UserId = 100,
                TotalAmount = 99.99m,
                Status = OrderStatus.Pending,
                ShippingAddress = "123 Test St",
                PaymentMethod = PaymentMethod.COD
            };

            Assert.Multiple(() =>
            {
                Assert.That(order.OrderId, Is.EqualTo(1));
                Assert.That(order.UserId, Is.EqualTo(100));
                Assert.That(order.TotalAmount, Is.EqualTo(99.99m));
                Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
                Assert.That(order.ShippingAddress, Is.EqualTo("123 Test St"));
                Assert.That(order.PaymentMethod, Is.EqualTo(PaymentMethod.COD));
                Assert.That(order.Items, Is.Not.Null);
            });
        }

        [Test]
        public void Order_DefaultOrderDate_IsSet()
        {
            var order = new Order();
            Assert.That(order.OrderDate, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public void Order_NavigationProperties_Work()
        {
            var user = new User { UserId = 1, FullName = "Test" };
            var payment = new Payment { PaymentId = 1 };
            var orderItem = new OrderItem { OrderItemId = 1 };
            var order = new Order
            {
                User = user,
                Payment = payment,
                Items = [orderItem]
            };

            Assert.Multiple(() =>
            {
                Assert.That(order.User, Is.EqualTo(user));
                Assert.That(order.Payment, Is.EqualTo(payment));
                Assert.That(order.Items, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void OrderStatus_EnumValues_HaveCorrectCount()
        {
            var values = System.Enum.GetValues<OrderStatus>();
            Assert.That(values, Has.Length.EqualTo(4));
        }

        [Test]
        public void PaymentMethod_EnumValues_HaveCorrectCount()
        {
            var values = System.Enum.GetValues<PaymentMethod>();
            Assert.That(values, Has.Length.EqualTo(2));
        }
    }
}
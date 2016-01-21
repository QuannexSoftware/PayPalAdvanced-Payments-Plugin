/*
    PayPalAdvanced Payments Plugin
    Copyright(C) 2016  Quan Luu

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 3 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see http://www.gnu.org/licenses.
*/
using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.PayPalAdvanced
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            // Payment Canceled
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.PaymentCanceled",
                 "ppa/cancel",
                 new { controller = "PaymentPayPalAdvanced", action = "PaymentCanceled" },
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            // Pay Order
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.PayOrder",
                 "ppa/payment/{orderId}",
                 new { controller = "PaymentPayPalAdvanced", action = "PayOrder" },                 
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            // Paypal Data Transfer Handler
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.PDTHandler",
                 "ppa/return",
                 new { controller = "PaymentPayPalAdvanced", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            // Payment Confirmation
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.PaymentConfirmation",
                 "ppa/confirmation/{orderId}/{resultCode}",
                 new { controller = "PaymentPayPalAdvanced", action = "PaymentConfirmation" },                 
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            // Payment Error Handler
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.PaymentErrorHandler",
                 "ppa/error",
                 new { controller = "PaymentPayPalAdvanced", action = "PaymentErrorHandler" },
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.PayPalAdvanced.AddLicenseKey",
                "Plugins/PayPalAdvanced/AddLicenseKey",
                new { controller = "PaymentPayPalAdvanced", action = "AddLicenseKey" },
                new string[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.PayPalAdvanced.DeleteLicenseKey",
                "Plugins/PayPalAdvanced/DeleteLicenseKey",
                new { controller = "PaymentPayPalAdvanced", action = "DeleteLicenseKey" }, 
                new string[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

            // License info
            routes.MapRoute("Plugin.Payments.PayPalAdvanced.License",
                 "ppa/license",
                 new { controller = "PaymentPayPalAdvanced", action = "ShowLicenseInfo" },
                 new[] { "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }
            );

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}

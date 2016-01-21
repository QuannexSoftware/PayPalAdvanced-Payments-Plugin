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
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.PayPalAdvanced.Controllers;
using Nop.Plugin.Payments.PayPalAdvanced.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Nop.Plugin.Payments.PayPalAdvanced.Data;

namespace Nop.Plugin.Payments.PayPalAdvanced
{
    /// <summary>
    /// PayPalAdvanced payment processor
    /// </summary>
    public class PayPalAdvancedPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayPalAdvancedPaymentSettings _payPalAdvancedPaymentSettings;
        private readonly ISettingService _settingService;        
        private readonly ICustomerService _customerService;        
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly HttpContextBase _httpContext;
        private readonly ILicenseService _licenseService;
        private readonly PayPalAdvancedObjectContext _objectContext;
        private readonly IEncryptionService _encryptionService;        
        private readonly PayPalHelper _payPalHelper;
        
        #endregion

        #region Ctor

        public PayPalAdvancedPaymentProcessor(PayPalAdvancedPaymentSettings payPalAdvancedPaymentSettings,
            ISettingService settingService,
            ICustomerService customerService,
            IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService,
            HttpContextBase httpContext,
            IOrderService orderService,
            ILicenseService licenseService,
            PayPalAdvancedObjectContext objectContext,
            IEncryptionService encryptionService,
            PayPalHelper payPalHelper)
        {
            this._payPalAdvancedPaymentSettings = payPalAdvancedPaymentSettings;
            this._settingService = settingService;            
            this._customerService = customerService;            
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContext = httpContext;
            this._licenseService = licenseService;
            this._objectContext = objectContext;
            this._encryptionService = encryptionService;
            this._payPalHelper = payPalHelper;               
        }

        #endregion
                
        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;                        
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            bool isSecure = _webHelper.IsCurrentConnectionSecured();
            string redirectUrl = _webHelper.GetStoreLocation(isSecure).Trim() + "ppa/payment/" + postProcessPaymentRequest.Order.Id;

            // Check license
            bool isLicensed = this._licenseService.IsLicensed(HttpContext.Current.Request.Url.Host);            
            if (!isLicensed && postProcessPaymentRequest.Order.OrderTotal > 5.00M)
            {
                redirectUrl = _webHelper.GetStoreLocation(isSecure).Trim() + "ppa/license";
            }
                        
            _httpContext.Response.Redirect(redirectUrl);
        }                

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _payPalAdvancedPaymentSettings.AdditionalFee, _payPalAdvancedPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            // Check license
            bool isLicensed = this._licenseService.IsLicensed(HttpContext.Current.Request.Url.Host);
            if (!isLicensed && capturePaymentRequest.Order.OrderTotal > 5.00M)
            {
                result.AddError("The trial license can be used to submit order of $5.00 or less. Please purchase a full license at our website.");
                return result;
            }

            string authorizationId = capturePaymentRequest.Order.AuthorizationTransactionId;

            // Create the Payflow Data Objects.
            // Create the User data object with the required user details.
            UserInfo payflowUser = _payPalHelper.GetUserInfo();

            // Create the Payflow Connection data object with the required connection details.            
            PayflowConnectionData payflowConn = new PayflowConnectionData(_payPalHelper.GetPayflowProHost());
                        
            CaptureTransaction trans = new CaptureTransaction(authorizationId, payflowUser, payflowConn, PayflowUtility.RequestId);
            Response resp = trans.SubmitTransaction();
                        
            // Process the Payflow response.
            if (resp != null)
            {
                // Get the Transaction Response parameters.
                TransactionResponse trxResp = resp.TransactionResponse;
                if (trxResp != null)
                {
                    if (trxResp.Result == 0)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                        result.CaptureTransactionId = trxResp.Pnref;
                        result.CaptureTransactionResult = trxResp.RespMsg;
                    }
                    else
                    {
                        result.AddError(string.Format("Capture RESULT: {0}-{1}", trxResp.Result, trxResp.RespMsg));
                    }
                }
            }
                                    
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            
            // Check license
            bool isLicensed = this._licenseService.IsLicensed(HttpContext.Current.Request.Url.Host);
            if (!isLicensed && refundPaymentRequest.Order.OrderTotal > 5.00M)
            {
                result.AddError("The trial license can be used to submit order of $5.00 or less. Please purchase a full license at our website.");
                return result;
            }

            string transactionId = refundPaymentRequest.Order.CaptureTransactionId;

            // Create the Payflow Data Objects.
            // Create the User data object with the required user details.
            UserInfo payflowUser = _payPalHelper.GetUserInfo();

            // Create the Payflow Connection data object with the required connection details.                        
            PayflowConnectionData payflowConn = new PayflowConnectionData(_payPalHelper.GetPayflowProHost());

            // Create a new Invoice data object with the Amount, Billing Address etc. details.
            Invoice invoice = new Invoice();

            // Set Amount.
            PayPal.Payments.DataObjects.Currency refundAmount = new PayPal.Payments.DataObjects.Currency(refundPaymentRequest.AmountToRefund);
            invoice.Amt = refundAmount;
            invoice.PoNum = refundPaymentRequest.Order.Id.ToString();
            invoice.InvNum = refundPaymentRequest.Order.Id.ToString();

            CreditTransaction trans = new CreditTransaction(transactionId, payflowUser, payflowConn, invoice, PayflowUtility.RequestId);
            Response resp = trans.SubmitTransaction();
                                                
            // Process the Payflow response.
            if (resp != null)
            {
                // Get the Transaction Response parameters.
                TransactionResponse trxResp = resp.TransactionResponse;
                if (trxResp != null)
                {
                    if (trxResp.Result == 0)
                    {
                        if (refundPaymentRequest.IsPartialRefund)
                            result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                        else
                            result.NewPaymentStatus = PaymentStatus.Refunded;
                    }
                    else
                    {
                        result.AddError(string.Format("Refund RESULT: {0}-{1}", trxResp.Result, trxResp.RespMsg));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            // Check license
            bool isLicensed = this._licenseService.IsLicensed(HttpContext.Current.Request.Url.Host);
            if (!isLicensed && voidPaymentRequest.Order.OrderTotal > 5.00M)
            {
                result.AddError("The trial license can be used to submit order of $5.00 or less. Please purchase a full license at our website.");
                return result;
            }

            string transactionId = voidPaymentRequest.Order.AuthorizationTransactionId;
            if (String.IsNullOrEmpty(transactionId))
                transactionId = voidPaymentRequest.Order.CaptureTransactionId;

            // Create the Payflow Data Objects.
            // Create the User data object with the required user details.
            UserInfo payflowUser = _payPalHelper.GetUserInfo();

            // Create the Payflow Connection data object with the required connection details.                        
            PayflowConnectionData payflowConn = new PayflowConnectionData(_payPalHelper.GetPayflowProHost());

            // Create a new Void Transaction.
            // The ORIGID is the PNREF no. for a previous transaction.
            VoidTransaction trans = new VoidTransaction(transactionId, payflowUser, payflowConn, PayflowUtility.RequestId);

            // Submit the Transaction
            Response resp = trans.SubmitTransaction();

            // Process the Payflow response.
            if (resp != null)
            {
                // Get the Transaction Response parameters.
                TransactionResponse trxResp = resp.TransactionResponse;
                if (trxResp != null)
                {
                    if (trxResp.Result == 0)
                    {
                        result.NewPaymentStatus = PaymentStatus.Voided;
                    }
                    else
                    {
                        result.AddError(string.Format("Void RESULT: {0}-{1}", trxResp.Result, trxResp.RespMsg));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //OK to repost payment
            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPayPalAdvanced";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayPalAdvanced";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.PayPalAdvanced.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentPayPalAdvancedController);
        }

        public override void Install()
        {
            //settings
            var settings = new PayPalAdvancedPaymentSettings
            {
                TransactMode = TransactMode.AuthorizeAndCapture,
                UseSandbox = true,
                PFPartner = "PayPal",
                SkipPaymentInfo = false,
                EnableMobileOptimizedLayout = false
            };
            _settingService.SaveSetting(settings);

            //data
            this._objectContext.Install();

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.UseSandbox", "Use Sandbox (Test Mode)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.TransactMode", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.TransactMode.Hint", "Specify transaction mode.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPartner", "Payflow Partner");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPartner.Hint", "Specify Payflow Partner (ex. PayPal)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFMerchantLogin", "Payflow Merchant Login");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFMerchantLogin.Hint", "Your unique, case sensitive merchant login");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFUser", "Payflow User");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFUser.Hint", "The same merchant login (the default admin user)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPassword", "Payflow Account Password");            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPassword.Hint", "Specify Payflow account password.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.IsEncryptPassword", "Encrypt Password");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.IsEncryptPassword.Hint", "Check to encrypt your password.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.SkipPaymentInfo", "Skip Payment Info Page/Step");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.SkipPaymentInfo.Hint", "Indicates whether we should display the payment information page/step of the checkout process.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.EnableMobileOptimizedLayout", "Enable Mobile Optimized Layout");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.EnableMobileOptimizedLayout.Hint", "Enable PayPal’s hosted checkout pages to be mobile optimized for iPhone, iPod and Android devices");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.LicenseKey", "License Keys");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.LicenseKey.Hint", "When you purchase the license key, it will be emailed to you. The plugin for NopCommerce 3.70 and later is free.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PaymentInfoTip", "Payment information will be entered after confirming the order.");
            
            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PayPalAdvancedPaymentSettings>();

            //data
            _objectContext.Uninstall();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.TransactMode");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.TransactMode.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPartner");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPartner.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFMerchantLogin");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFMerchantLogin.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFUser");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFUser.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPassword");            
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PFPassword.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.IsEncryptPassword");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.IsEncryptPassword.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.SkipPaymentInfo");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.SkipPaymentInfo.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.EnableMobileOptimizedLayout");
            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.EnableMobileOptimizedLayout.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.PayPalAdvanced.Fields.PaymentInfoTip");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return _payPalAdvancedPaymentSettings.SkipPaymentInfo;
            }
        }

        #endregion
    }
}
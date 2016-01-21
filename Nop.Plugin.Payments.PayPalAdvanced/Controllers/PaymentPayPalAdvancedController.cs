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
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Payments.PayPalAdvanced.Domain;
using Nop.Plugin.Payments.PayPalAdvanced.Models;
using Nop.Plugin.Payments.PayPalAdvanced.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Nop.Plugin.Payments.PayPalAdvanced.Common;
using Nop.Services.Logging;

namespace Nop.Plugin.Payments.PayPalAdvanced.Controllers
{
    public class PaymentPayPalAdvancedController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;                
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;        
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ITaxService _taxService;                
        private readonly PayPalAdvancedPaymentSettings _payPalAdvancedPaymentSettings;                          
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;        
        private readonly ILicenseService _licenseService;
        private readonly IEncryptionService _encryptionService;
        private readonly PayPalHelper _payPalHelper;
        private readonly ILogger _logger;

        public PaymentPayPalAdvancedController(
            IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService _currencyService,
            IPriceFormatter priceFormatter,
            ITaxService taxService,
            PayPalAdvancedPaymentSettings payPalAdvancedPaymentSettings,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ILicenseService licenseService,
            IEncryptionService encryptionService,
            PayPalHelper payPalHelper,
            ILogger logger)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._currencyService = _currencyService;
            this._priceFormatter = priceFormatter;
            this._taxService = taxService;
            this._payPalAdvancedPaymentSettings = payPalAdvancedPaymentSettings;
            this._taxSettings = taxSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._licenseService = licenseService;
            this._encryptionService = encryptionService;
            this._payPalHelper = payPalHelper;
            this._logger = logger;
        }

        #region Utilities

        public bool IsMobileDevice()
        {
            String u = HttpContext.Request.ServerVariables["HTTP_USER_AGENT"];
            if (u != null)
            {                
                if (string.IsNullOrEmpty(u)) 
                    return false;
                Regex b = new Regex("android.+mobile|avantgo|bada\\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                Regex v = new Regex("1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\\-(n|u)|c55\\/|capi|ccwa|cdm\\-|cell|chtm|cldc|cmd\\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\\-s|devi|dica|dmob|do(c|p)o|ds(12|\\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\\-|_)|g1 u|g560|gene|gf\\-5|g\\-mo|go(\\.w|od)|gr(ad|un)|haie|hcit|hd\\-(m|p|t)|hei\\-|hi(pt|ta)|hp( i|ip)|hs\\-c|ht(c(\\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\\-(20|go|ma)|i230|iac( |\\-|\\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\\/)|klon|kpt |kwc\\-|kyo(c|k)|le(no|xi)|lg( g|\\/(k|l|u)|50|54|e\\-|e\\/|\\-[a-w])|libw|lynx|m1\\-w|m3ga|m50\\/|ma(te|ui|xo)|mc(01|21|ca)|m\\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\\-2|po(ck|rt|se)|prox|psio|pt\\-g|qa\\-a|qc(07|12|21|32|60|\\-[2-7]|i\\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\\-|oo|p\\-)|sdk\\/|se(c(\\-|0|1)|47|mc|nd|ri)|sgh\\-|shar|sie(\\-|m)|sk\\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\\-|v\\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\\-|tdg\\-|tel(i|m)|tim\\-|t\\-mo|to(pl|sh)|ts(70|m\\-|m3|m5)|tx\\-9|up(\\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|xda(\\-|2|g)|yas\\-|your|zeto|zte\\-", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                if (u.Length > 4)
                {
                    if (b.IsMatch(u) | v.IsMatch(u.Substring(0, 4)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalAdvancedPaymentSettings = _settingService.LoadSetting<PayPalAdvancedPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UseSandbox = payPalAdvancedPaymentSettings.UseSandbox;
            model.TransactModeId = Convert.ToInt32(payPalAdvancedPaymentSettings.TransactMode);
            model.PFPartner = payPalAdvancedPaymentSettings.PFPartner;
            model.PFMerchantLogin = payPalAdvancedPaymentSettings.PFMerchantLogin;
            model.PFUser = payPalAdvancedPaymentSettings.PFUser;
            model.PFPassword = payPalAdvancedPaymentSettings.PFPassword;
            model.IsEncryptPassword = payPalAdvancedPaymentSettings.IsEncryptPassword;
            model.AdditionalFee = payPalAdvancedPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = payPalAdvancedPaymentSettings.AdditionalFeePercentage;
            model.TransactModeValues = payPalAdvancedPaymentSettings.TransactMode.ToSelectList();
            model.SkipPaymentInfo = payPalAdvancedPaymentSettings.SkipPaymentInfo;
            model.EnableMobileOptimizedLayout = payPalAdvancedPaymentSettings.EnableMobileOptimizedLayout;
            model.LicenseKeys = (from k in this._licenseService.GetAll() select new LicenseModel { Id = k.Id, LicenseKey = k.LicenseKey, Host = this._licenseService.GetLicenseHost(k.LicenseKey), Type = this._licenseService.GetLicenseType(k.LicenseKey) }).ToList<LicenseModel>();
            
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.UseSandbox, storeScope);
                model.TransactModeId_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.TransactMode, storeScope);
                model.PFPartner_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.PFPartner, storeScope);
                model.PFMerchantLogin_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.PFMerchantLogin, storeScope);
                model.PFUser_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.PFUser, storeScope);
                model.PFPassword_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.PFPassword, storeScope);
                model.IsEncryptPassword_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.IsEncryptPassword, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.SkipPaymentInfo_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.SkipPaymentInfo, storeScope);
                model.EnableMobileOptimizedLayout_OverrideForStore = _settingService.SettingExists(payPalAdvancedPaymentSettings, x => x.EnableMobileOptimizedLayout, storeScope);                
            }

            return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var PayPalAdvancedPaymentSettings = _settingService.LoadSetting<PayPalAdvancedPaymentSettings>(storeScope);

            //save settings
            PayPalAdvancedPaymentSettings.UseSandbox = model.UseSandbox;
            PayPalAdvancedPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            PayPalAdvancedPaymentSettings.PFPartner = model.PFPartner;
            PayPalAdvancedPaymentSettings.PFMerchantLogin = model.PFMerchantLogin;
            PayPalAdvancedPaymentSettings.PFUser = model.PFUser;
            ModelState.Remove("PFPassword"); //ensure value is updated to view
            ModelState.Remove("IsEncryptPassword");
            if (string.IsNullOrEmpty(model.PFPassword))
            {
                PayPalAdvancedPaymentSettings.PFPassword = model.PFPassword;
                model.IsEncryptPassword = false;
                PayPalAdvancedPaymentSettings.IsEncryptPassword = model.IsEncryptPassword;
            }
            else if (model.IsEncryptPassword)
            {
                try
                {
                    // try decrypt first
                    string tempPwd = _encryptionService.DecryptText(model.PFPassword, SecurityHelper.GetPrivateKey());
                    // now encrypt it again
                    PayPalAdvancedPaymentSettings.PFPassword = _encryptionService.EncryptText(tempPwd, SecurityHelper.GetPrivateKey());
                }
                catch
                {
                    // if failed, now encrypt it
                    try
                    {
                        PayPalAdvancedPaymentSettings.PFPassword = _encryptionService.EncryptText(model.PFPassword, SecurityHelper.GetPrivateKey());
                    }
                    catch
                    {
                        // if failed again, set it as is
                        PayPalAdvancedPaymentSettings.PFPassword = model.PFPassword;
                    }
                }
                PayPalAdvancedPaymentSettings.IsEncryptPassword = model.IsEncryptPassword;
            }
            else
            {
                // decrypt it
                try
                {
                    PayPalAdvancedPaymentSettings.PFPassword = _encryptionService.DecryptText(model.PFPassword, SecurityHelper.GetPrivateKey());
                }
                catch
                {
                    // if failed again, set it as is
                    PayPalAdvancedPaymentSettings.PFPassword = model.PFPassword;
                }
                PayPalAdvancedPaymentSettings.IsEncryptPassword = model.IsEncryptPassword;
            }
            
            PayPalAdvancedPaymentSettings.AdditionalFee = model.AdditionalFee;
            PayPalAdvancedPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            PayPalAdvancedPaymentSettings.SkipPaymentInfo = model.SkipPaymentInfo;
            PayPalAdvancedPaymentSettings.EnableMobileOptimizedLayout = model.EnableMobileOptimizedLayout;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.UseSandbox_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.TransactModeId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.TransactMode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.TransactMode, storeScope);

            if (model.PFPartner_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.PFPartner, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.PFPartner, storeScope);

            if (model.PFMerchantLogin_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.PFMerchantLogin, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.PFMerchantLogin, storeScope);

            if (model.PFUser_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.PFUser, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.PFUser, storeScope);

            if (model.PFPassword_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.PFPassword, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.PFPassword, storeScope);

            if (model.IsEncryptPassword_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.IsEncryptPassword, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.IsEncryptPassword, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.SkipPaymentInfo_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.SkipPaymentInfo, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.SkipPaymentInfo, storeScope);

            if (model.EnableMobileOptimizedLayout_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(PayPalAdvancedPaymentSettings, x => x.EnableMobileOptimizedLayout, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(PayPalAdvancedPaymentSettings, x => x.EnableMobileOptimizedLayout, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }                      

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            return PartialView("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/PaymentInfo.cshtml", model);            
        }
                                
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
                                
        public ActionResult PaymentCanceled(FormCollection form)
        {            
            var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                .FirstOrDefault();
            if (order != null)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                //return RedirectToRoute("Plugin.Payments.PayPalAdvanced.PayOrder", new { orderId = order.OrderId.ToString() });                
            }            

            return RedirectToAction("Index", "Home", new { area = "" });
        }
                                
        public ActionResult PayOrder(int orderId)
        {                        
            var model = new PaymentFormModel();

            try
            {
                model.OrderId = orderId;

                var order = _orderService.GetOrderById(orderId);

                if (order == null || order.Deleted)
                    return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/PaymentForm.cshtml", model);

                if (_workContext.CurrentCustomer.Id != order.CustomerId)
                    return new HttpUnauthorizedResult();

                if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });

                                                                
                // Create a new Invoice data object with the Amount, Billing Address etc. details.
                Invoice inv = new Invoice();
                decimal orderTotal = 0.0M;

                try
                {                    
                    // Set Amount.
                    orderTotal = Math.Round(order.OrderTotal, 2);
                    PayPal.Payments.DataObjects.Currency amt = new PayPal.Payments.DataObjects.Currency(orderTotal);
                    inv.Amt = amt;                    
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create invoice amount: " + ex.Message);
                }

                inv.PoNum = order.Id.ToString();
                inv.InvNum = order.Id.ToString();

                try
                {
                    // Check license
                    bool isLicensed = this._licenseService.IsLicensed(HttpContext.Request.Url.Host);
                    if (!isLicensed && orderTotal > 5.00M)
                    {
                        return ShowLicenseInfo();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to verify plugin's license: " + ex.Message);
                }

                // Set the Billing Address details.
                BillTo billTo = new BillTo();

                // optional fields
                try
                {
                    billTo.FirstName = order.Customer.BillingAddress.FirstName;
                }
                catch { }
                try
                {
                    billTo.LastName = order.Customer.BillingAddress.LastName;
                }
                catch { }
                try
                {
                    billTo.Street = order.Customer.BillingAddress.Address1;
                }
                catch { }
                try
                {
                    billTo.City = order.Customer.BillingAddress.City;
                }
                catch { }
                try
                {
                    billTo.State = order.Customer.BillingAddress.StateProvince.Abbreviation;
                }
                catch { }
                try
                {
                    billTo.Zip = order.Customer.BillingAddress.ZipPostalCode;
                }
                catch { }
                try
                {
                    billTo.BillToCountry = order.Customer.BillingAddress.Country.NumericIsoCode.ToString();
                }
                catch { }

                inv.BillTo = billTo;

                // Set the Shipping Address details.
                //if (order.Customer.ShippingAddress != null)
                //{
                //    if (order.Customer.ShippingAddress.StateProvince != null && order.Customer.ShippingAddress.Country != null)
                //    {
                //        ShipTo shipTo = new ShipTo();
                //        shipTo.ShipToFirstName = order.Customer.ShippingAddress.FirstName;
                //        shipTo.ShipToLastName = order.Customer.ShippingAddress.LastName;
                //        shipTo.ShipToStreet = order.Customer.ShippingAddress.Address1;
                //        //shipTo.ShipToStreet2 = order.Customer.ShippingAddress.Address2;
                //        shipTo.ShipToCity = order.Customer.ShippingAddress.City;
                //        shipTo.ShipToState = order.Customer.ShippingAddress.StateProvince.Abbreviation;
                //        shipTo.ShipToZip = order.Customer.ShippingAddress.ZipPostalCode;
                //        shipTo.ShipToCountry = order.Customer.ShippingAddress.Country.NumericIsoCode.ToString();
                //        inv.ShipTo = shipTo;
                //    }
                //}


                // Create the Payflow Data Objects.
                // Create the User data object with the required user details.
                UserInfo payflowUser = null;
                try
                {
                    payflowUser = _payPalHelper.GetUserInfo();
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create Payflow User object, check the configuration: " + ex.Message);
                }

                // Create the Payflow Connection data object with the required connection details.                        
                PayflowConnectionData payflowConn = null;
                try
                {
                    payflowConn = new PayflowConnectionData(_payPalHelper.GetPayflowProHost());
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to create Payflow connection, check the web.config: " + ex.Message);
                }

                string payflowRequestId = PayflowUtility.RequestId;
                Response resp;

                if (_payPalAdvancedPaymentSettings.TransactMode == TransactMode.Authorize)
                {
                    // Create a new Auth Transaction.
                    AuthorizationTransaction trans = null;
                    try
                    {
                        trans = new AuthorizationTransaction(payflowUser, payflowConn, inv, null, payflowRequestId);

                        trans.AddToExtendData(new ExtendData("CREATESECURETOKEN", "Y"));
                        trans.AddToExtendData(new ExtendData("SECURETOKENID", payflowRequestId));
                        if (_payPalAdvancedPaymentSettings.EnableMobileOptimizedLayout && this.IsMobileDevice())
                            trans.AddToExtendData(new ExtendData("TEMPLATE", "MOBILE"));

                        // Submit the Transaction
                        resp = trans.SubmitTransaction();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error processing AuthorizationTransaction: " + ex.Message);
                    }
                }
                else
                {
                    // Create a new Sale Transaction.
                    SaleTransaction trans = null;
                    try
                    {
                        trans = new SaleTransaction(payflowUser, payflowConn, inv, null, payflowRequestId);

                        trans.AddToExtendData(new ExtendData("CREATESECURETOKEN", "Y"));
                        trans.AddToExtendData(new ExtendData("SECURETOKENID", payflowRequestId));
                        if (_payPalAdvancedPaymentSettings.EnableMobileOptimizedLayout && this.IsMobileDevice())
                            trans.AddToExtendData(new ExtendData("TEMPLATE", "MOBILE"));

                        // Submit the Transaction
                        resp = trans.SubmitTransaction();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error processing SaleTransaction: " + ex.Message);
                    }
                }

                string paypalSecureToken = string.Empty;
                string paypalContent = string.Empty;

                // Process the Payflow response.
                if (resp != null)
                {
                    // Get the Transaction Response parameters.
                    TransactionResponse trxResp = resp.TransactionResponse;
                    if (trxResp != null)
                    {
                        if (trxResp.Result == 0)
                        {
                            paypalSecureToken = (from ExtendData edEntry in resp.ExtendDataList
                                                 where edEntry.ParamName == "SECURETOKEN"
                                                 select edEntry.ParamValue).FirstOrDefault();

                            model.PayflowSecureToken = paypalSecureToken.Trim();
                            model.PayflowSecureTokenId = payflowRequestId.Trim();
                            model.PayflowMode = _payPalAdvancedPaymentSettings.UseSandbox ? "TEST" : "LIVE";
                            model.PayflowUrl = _payPalHelper.GetPayflowLinkHost();
                            model.Success = true;
                        }
                        else
                        {
                            // Show error msg
                            model.ErrorMsg = string.Format("Error: {0} - {1}", trxResp.Result, trxResp.RespMsg != null ? trxResp.RespMsg : "");

                            // Log resp error
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "Failed fetching PayPal Secure Token: " + model.ErrorMsg,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });

                            if (_orderService != null)
                            {
                                _orderService.UpdateOrder(order);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                model.ErrorMsg = "An error has occurred, please check System Log for more information.";
            }                       
            
            return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/PaymentForm.cshtml", model);
        }
                       
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var model = new PDTModel();

            // Parse response
            if (form != null && form.AllKeys != null)
            {
                model.ResultCode = form["RESULT"] != null ? form["RESULT"] : "";
                model.ResponseMessage = form["RESPMSG"] != null ? form["RESPMSG"] : "";
                model.OrderId = form["INVNUM"] != null ? int.Parse(form["INVNUM"]) : 0;
                model.AuthorizationCode = form["AUTHCODE"] != null ? form["AUTHCODE"] : "";
                model.TransactionId = form["PNREF"] != null ? form["PNREF"] : "";
                model.TransactionType = form["TYPE"] != null ? form["TYPE"].ToUpper() : "";
                model.Amount = form["AMT"] != null ? decimal.Parse(form["AMT"]) : 0.0M;
                
                if (model.OrderId > 0)
                {
                    var order = _orderService.GetOrderById(model.OrderId);
                    if (order != null)
                    {                        
                        var sb = new StringBuilder("PAYMENT RESULT: ");
                        sb.Append(string.Format("{0} = {1}, ", "RESULT", model.ResultCode));
                        sb.Append(string.Format("{0} = {1}, ", "RESPMSG", model.ResponseMessage));
                        sb.Append(string.Format("{0} = {1}, ", "AUTHCODE", model.AuthorizationCode));
                        sb.Append(string.Format("{0} = {1}, ", "PNREF", model.TransactionId));
                        sb.Append(string.Format("{0} = {1}, ", "TYPE", model.TransactionType));
                        sb.Append(string.Format("{0} = {1}", "AMT", model.Amount));

                        // order note
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = sb.ToString(),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);

                        // Log full Paypal's response
                        if (_payPalHelper.IsLogPaypalResponse)
                        {
                            sb = new StringBuilder("PAYPAL RESPONSE: ");
                            foreach (string key in form.AllKeys)
                            {
                                sb.Append(string.Format("{0}:{1}, ", key, form[key]));
                            }

                            //order note
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = sb.ToString(),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                        }

                        // 0 = Approved
                        if (model.ResultCode == "0")
                        {
                            // Set order payment properties
                            order.AuthorizationTransactionCode = model.AuthorizationCode;
                            if (model.TransactionType == "A")
                            {
                                order.AuthorizationTransactionId = model.TransactionId;
                                order.AuthorizationTransactionResult = model.ResponseMessage;
                                _orderService.UpdateOrder(order);

                                // mark order as authorized
                                if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                                {
                                    _orderProcessingService.MarkAsAuthorized(order);
                                }
                            }
                            else
                            {
                                order.CaptureTransactionId = model.TransactionId;
                                order.CaptureTransactionResult = model.ResponseMessage;
                                _orderService.UpdateOrder(order);

                                // mark order as paid
                                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                {
                                    _orderProcessingService.MarkOrderAsPaid(order);
                                }
                            }
                        }

                    }
                }
            }

            // redirect out of the iframe
            return Content(string.Format("<script>parent.location = \"/ppa/confirmation/{0}/{1}?q={2}\"</script>", model.OrderId, model.ResultCode, HttpUtility.UrlEncode(model.TransactionId)));
            
        }

        public ActionResult PaymentConfirmation(int orderId, string resultCode, string q)
        {
            var model = new PaymentConfirmationModel();
            
            var order = _orderService.GetOrderById(orderId);

            if (order == null)
                return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/PaymentConfirmation.cshtml", model);                    

            if (_workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();
                        
            model.OrderId = order.Id;
            model.ResultCode = resultCode.Trim();
            model.ConfirmationId = HttpUtility.UrlDecode(q);
 
            return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/PaymentConfirmation.cshtml", model);
        }
                
        [ValidateInput(false)]
        public ActionResult PaymentErrorHandler(FormCollection form)
        {
            var model = new PDTModel();

            // Parse response
            if (form != null && form.AllKeys != null)
            {
                model.ResultCode = form["RESULT"] != null ? form["RESULT"].Trim() : "";
                model.ResponseMessage = form["RESPMSG"] != null ? form["RESPMSG"] : "";
                model.OrderId = form["INVNUM"] != null ? int.Parse(form["INVNUM"]) : 0;
                model.AuthorizationCode = form["AUTHCODE"] != null ? form["AUTHCODE"] : "";
                model.TransactionId = form["PNREF"] != null ? form["PNREF"] : "";
                model.TransactionType = form["TYPE"] != null ? form["TYPE"].ToUpper() : "";
                model.Amount = form["AMT"] != null ? decimal.Parse(form["AMT"]) : 0.0M;

                if (model.OrderId > 0)
                {
                    var order = _orderService.GetOrderById(model.OrderId);
                    if (order != null)
                    {                        
                        var sb = new StringBuilder("PAYMENT RESULT: ");
                        sb.Append(string.Format("{0} = {1}, ", "RESULT", model.ResultCode));
                        sb.Append(string.Format("{0} = {1}, ", "RESPMSG", model.ResponseMessage));
                        sb.Append(string.Format("{0} = {1}, ", "AUTHCODE", model.AuthorizationCode));
                        sb.Append(string.Format("{0} = {1}, ", "PNREF", model.TransactionId));
                        sb.Append(string.Format("{0} = {1}, ", "TYPE", model.TransactionType));
                        sb.Append(string.Format("{0} = {1}", "AMT", model.Amount));

                        //order note
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = sb.ToString(),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });                                                
                        _orderService.UpdateOrder(order);

                        // Log full Paypal's response
                        if (_payPalHelper.IsLogPaypalResponse)
                        {
                            sb = new StringBuilder("PAYPAL RESPONSE: ");
                            foreach (string key in form.AllKeys)
                            {
                                sb.Append(string.Format("{0}:{1}, ", key, form[key]));
                            }

                            //order note
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = sb.ToString(),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                        }
                    }
                }
            }

            // redirect out of the iframe
            return Content(string.Format("<script>parent.location = \"/ppa/confirmation/{0}/{1}?q={2}\"</script>", model.OrderId, model.ResultCode, HttpUtility.UrlEncode(model.TransactionId)));
        }

        [HttpPost, AdminAuthorize]
        public string AddLicenseKey(string licenseKey)
        {
            LicenseRecord key = new LicenseRecord
            {
                LicenseKey = licenseKey
            };
            _licenseService.Insert(key);
            return ("<tr id=\"LicenseRow_" + key.Id.ToString() + "\"><td>" + _licenseService.GetLicenseType(licenseKey) + "</td><td>" + _licenseService.GetLicenseHost(licenseKey) + "</td><td>" + licenseKey + "</td><td class=\"delete\"><a class=\"delkey\" id=\"DeleteKey_" + key.Id.ToString() + "\" data-id=\"" + key.Id.ToString() + "\">Delete</a></td></tr>");
        }

        [HttpPost, AdminAuthorize]
        public int DeleteLicenseKey(int id)
        {
            LicenseRecord byId = _licenseService.GetById(id);
            _licenseService.Delete(byId);
            return id;
        }

        public ActionResult ShowLicenseInfo()
        {            
            return View("~/Plugins/Payments.PayPalAdvanced/Views/PaymentPayPalAdvanced/LicenseInfo.cshtml", null);
        }
  
    }
}
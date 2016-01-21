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
using PayPal.Payments.DataObjects;
using System.Configuration;
using Nop.Services.Security;
using Nop.Plugin.Payments.PayPalAdvanced.Common;
using System;

namespace Nop.Plugin.Payments.PayPalAdvanced
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class PayPalHelper
    {
        #region Fields

        private readonly PayPalAdvancedPaymentSettings _payPalAdvancedPaymentSettings;
        private readonly IEncryptionService _encryptionService;
                
        #endregion

        #region Ctor

        public PayPalHelper(PayPalAdvancedPaymentSettings payPalAdvancedPaymentSettings, IEncryptionService encryptionService)
        {
            this._payPalAdvancedPaymentSettings = payPalAdvancedPaymentSettings;
            this._encryptionService = encryptionService;      
        }

        #endregion

        public string GetPayflowProHost()
        {
            string host = "";
            if (_payPalAdvancedPaymentSettings.UseSandbox)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PayflowProSanbox"]))
                    host = ConfigurationManager.AppSettings["PayflowProSanbox"];
                else
                    host = "pilot-payflowpro.paypal.com";
            }
            else
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PayflowPro"]))
                    host = ConfigurationManager.AppSettings["PayflowPro"];
                else
                    host = "payflowpro.paypal.com";
            }
            return host;
        }

        public string GetPayflowLinkHost()
        {
            string host = "";
            if (_payPalAdvancedPaymentSettings.UseSandbox)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PayflowLinkSanbox"]))
                    host = ConfigurationManager.AppSettings["PayflowLinkSanbox"];
                else
                    host = "pilot-payflowlink.paypal.com";
            }
            else
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PayflowLink"]))
                    host = ConfigurationManager.AppSettings["PayflowLink"];
                else
                    host = "payflowlink.paypal.com";
            }
            return host.Trim();
        }
                
        public UserInfo GetUserInfo()
        {
            string password = _payPalAdvancedPaymentSettings.PFPassword;
            if (_payPalAdvancedPaymentSettings.IsEncryptPassword)
            {
                try
                {
                    password = _encryptionService.DecryptText(_payPalAdvancedPaymentSettings.PFPassword, SecurityHelper.GetPrivateKey());
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to decrypt password: " + ex.Message);
                }
            }

            return new UserInfo(
                _payPalAdvancedPaymentSettings.PFUser,
                _payPalAdvancedPaymentSettings.PFMerchantLogin,
                _payPalAdvancedPaymentSettings.PFPartner,
                password
                );
        }

        public bool IsLogPaypalResponse
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["LogPaypalResponse"])
                     && ConfigurationManager.AppSettings["LogPaypalResponse"].ToLower() == "true")
                    return true;
                else
                    return false;
            }
        }
    }
}


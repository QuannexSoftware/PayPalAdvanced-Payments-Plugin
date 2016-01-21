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
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.PayPalAdvanced.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public int TransactModeId { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.TransactMode")]
        public SelectList TransactModeValues { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.PFPartner")]
        public string PFPartner { get; set; }
        public bool PFPartner_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.PFMerchantLogin")]
        public string PFMerchantLogin { get; set; }
        public bool PFMerchantLogin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.PFUser")]
        public string PFUser { get; set; }
        public bool PFUser_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.PFPassword")]
        public string PFPassword { get; set; }
        public bool PFPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.IsEncryptPassword")]
        public bool IsEncryptPassword { get; set; }
        public bool IsEncryptPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.SkipPaymentInfo")]
        public bool SkipPaymentInfo { get; set; }
        public bool SkipPaymentInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.EnableMobileOptimizedLayout")]
        public bool EnableMobileOptimizedLayout { get; set; }
        public bool EnableMobileOptimizedLayout_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalAdvanced.Fields.LicenseKey")]
        public IList<LicenseModel> LicenseKeys { get; set; }        
    }
}
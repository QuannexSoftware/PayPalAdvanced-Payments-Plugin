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
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayPalAdvanced
{
    public class PayPalAdvancedPaymentSettings : ISettings
    {
        public TransactMode TransactMode { get; set; }
        public bool UseSandbox { get; set; }
        public string PFPartner { get; set; }
        public string PFMerchantLogin { get; set; }
        public string PFUser { get; set; }
        public string PFPassword { get; set; }
        public bool IsEncryptPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
        public bool SkipPaymentInfo { get; set; }
        public bool EnableMobileOptimizedLayout { get; set; }
    }
}

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
using Nop.Core.Data;
using Nop.Plugin.Payments.PayPalAdvanced.Domain;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Payments.PayPalAdvanced.Services
{
    public partial class LicenseService : ILicenseService
    {
        #region Fields

        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<LicenseRecord> _licRepository;
        private const string k = "Xg79YVSc2Blcjf0mtWtb5LeqcxAKCFR6VvWnmamwpp4EuvtiMRab2W14S7zb0kDjTE4FxQ00tEecpYDvnYTcCV5InP";

        #endregion

        #region Ctor

        public LicenseService(IRepository<LicenseRecord> licRepository, IEncryptionService encryptionService)
        {
            this._licRepository = licRepository;
            this._encryptionService = encryptionService;
        }

        #endregion
                
        #region Methods

        public virtual void Insert(LicenseRecord licenseRecord)
        {
            if (licenseRecord == null)
                throw new ArgumentNullException("licenseRecord");

            _licRepository.Insert(licenseRecord);
        }

        public virtual void Update(LicenseRecord licenseRecord)
        {
            if (licenseRecord == null)
                throw new ArgumentNullException("licenseRecord");

            _licRepository.Update(licenseRecord);
        }  

        public void Delete(LicenseRecord licenseRecord)
        {
            if (licenseRecord == null)
            {
                throw new ArgumentNullException("licenseRecord");
            }
            this._licRepository.Delete(licenseRecord);
        }

        public virtual IList<LicenseRecord> GetAll()
        {
            var query = from lic in _licRepository.Table
                        orderby lic.Id
                        select lic;
            var records = query.ToList();
            return records;
        }

        public virtual LicenseRecord GetById(int Id)
        {
            if (Id == 0)
                return null;

            return _licRepository.GetById(Id);
        }
                
        public string GetLicenseHost(string licKey)
        {
            string retStr = "";
            try
            {                                
                string plainStr = _encryptionService.DecryptText(licKey, k.Substring(k[69] - ' ', 0x10));
                if (plainStr.Length > 1)
                {
                    retStr = plainStr.Substring(1);
                }
            }
            catch
            {
                retStr = "Invalid/Error";
            }
            return retStr;
        }

        public string GetLicenseType(string licKey)
        {
            try
            {
                string plainStr = _encryptionService.DecryptText(licKey, k.Substring(k[69] - ' ', 0x10));
                if (plainStr.Length > 0)
                {
                    switch (plainStr[0])
                    {
                        case 'D':
                            return "Domain";
                        case 'U':
                            return "URL";
                        default:
                            return "N/A";
                    }                    
                }
                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
                
        public bool IsLicensed(string host)
        {
            return true; // no longer check license 1/20/2016

            host = host.ToLower().Trim();
            var keys = GetAll();

            foreach (var key in keys)
            {
                try
                {
                    string decryptedKey = _encryptionService.DecryptText(key.LicenseKey, k.Substring(k[69] - ' ', 0x10));
                    string licensedHost = decryptedKey.Substring(1).ToLower().Trim();
                    if (decryptedKey[0] == 'U')
                    {
                        if (host == licensedHost || host == "www." + licensedHost)
                            return true;
                    }
                    else if (decryptedKey[0] == 'D')
                    {
                        if (host.EndsWith(licensedHost))
                            return true;
                    }
                }
                catch { }
            }

            return false;
        }                             

        #endregion
    }
}

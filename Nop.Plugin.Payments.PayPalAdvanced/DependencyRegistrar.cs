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
using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.PayPalAdvanced.Data;
using Nop.Plugin.Payments.PayPalAdvanced.Domain;
using Nop.Plugin.Payments.PayPalAdvanced.Services;
using Nop.Web.Framework.Mvc;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayPalAdvanced
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<LicenseService>().As<ILicenseService>().InstancePerLifetimeScope();
            builder.RegisterType<PayPalHelper>().As<PayPalHelper>().InstancePerLifetimeScope();
                        
            //data context
            this.RegisterPluginDataContext<PayPalAdvancedObjectContext>(builder, "nop_object_context_paypaladvanced_license");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<LicenseRecord>>()
                .As<IRepository<LicenseRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_paypaladvanced_license"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}

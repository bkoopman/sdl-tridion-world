//-----------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//-----------------------------------------------------------------------------

using System.Web.Configuration;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.SecurityTokenService;

namespace LocalStsService
{
    /// <summary>
    /// A custom SecurityTokenServiceConfiguration implementation.
    /// </summary>
    public class WindowsSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {
        /// <summary>
        /// CustomSecurityTokenServiceConfiguration constructor.
        /// </summary>
        public WindowsSecurityTokenServiceConfiguration() : base(WebConfigurationManager.AppSettings["IssuerName"], new X509SigningCredentials(CertificateUtil.GetCertificate(WebConfigurationManager.AppSettings["Thumbprint"])))
        {
            SecurityTokenService = typeof(WindowsSecurityTokenService);
        }
    }
}

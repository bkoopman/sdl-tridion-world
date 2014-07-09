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

using System;
using System.Security.Cryptography.X509Certificates;

namespace LocalStsService
{
    /// <summary>
    /// A utility class which helps to retrieve an x509 certificate
    /// </summary>
    public class CertificateUtil
    {
        /// <summary>
        /// Gets the certificate in the My store on the local machine defined by the thumb print in the configuration.
        /// </summary>
        /// <param name="thumbprint">Find certificate by this thumb print.</param>
        /// <returns>A X509Certificate2 certificate.</returns>
        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Store store = null;
            X509Certificate2Collection allCertificates = null;

            try
            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                // Every time we call store.Certificates property, a new collection will be returned.
                allCertificates = store.Certificates;
                X509Certificate2Collection foundCertificates = allCertificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

                if (foundCertificates.Count == 0)
                {
                    throw new Exception("NoValidCertificateFound");
                }
                if (foundCertificates.Count > 1)
                {
                    throw new Exception("MultipleValidCertificatesFound");
                }

                return new X509Certificate2(foundCertificates[0]);
            }
            finally
            {
                if (allCertificates != null)
                {
                    foreach (X509Certificate2 cert in allCertificates)
                    {
                        cert.Reset();
                    }
                }

                if (store != null)
                {
                    store.Close();
                }
            }
        }


        public static X509Certificate2 GetCertificate(StoreName name, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(name, location);
            X509Certificate2Collection certificates = null;
            store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2 result = null;

                //
                // Every time we call store.Certificates property, a new collection will be returned.
                //
                certificates = store.Certificates;

                foreach (X509Certificate2 cert in certificates)
                {
                    if (cert.SubjectName.Name.ToLower() == subjectName.ToLower())
                    {
                        if (result != null)
                        {
                            throw new ApplicationException(string.Format("There are multiple certificate for subject Name {0}", subjectName));
                        }

                        result = new X509Certificate2(cert);
                    }
                }

                if (result == null)
                {
                    throw new ApplicationException(string.Format("No certificate was found for subject Name {0}", subjectName));
                }

                return result;
            }
            finally
            {
                if (certificates != null)
                {
                    foreach (X509Certificate2 cert in certificates)
                    {
                        cert.Reset();
                    }
                }

                store.Close();
            }
        }
    }
}

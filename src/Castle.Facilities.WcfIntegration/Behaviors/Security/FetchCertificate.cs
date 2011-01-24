// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.WcfIntegration.Behaviors.Security
{
	using System.Security.Cryptography.X509Certificates;

	public class FetchCertificate
	{
		public FetchCertificate(X509FindType criteria, object match)
		{
			Criteria = criteria;
			MatchesValue = match;
			StoreLocation = StoreLocation.LocalMachine;
			StoreName = StoreName.My;
		}

		public X509FindType Criteria { get; private set; }

		public object MatchesValue { get; private set; }

		public StoreLocation StoreLocation { get; set; }

		public StoreName StoreName { get; set; }

		public static implicit operator AbstractCredentials(FetchCertificate finder)
		{
			return new CertificateCredentials(finder);
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using Auth0.ManagementApi.Models;

namespace WhichClientWhichRules.ScriptParser
{
    public interface IApplicableClient
    {
	    bool TryGetMatchingClient(ICollection<Client> clients, out Client client);

	    string GetHumanReadableId();
    }

	public class ApplicableByName : IApplicableClient
	{
		private readonly string _clientName;

		public ApplicableByName(string clientName)
		{
			_clientName = clientName;
		}

		public bool TryGetMatchingClient(ICollection<Client> clients, out Client client)
		{
			client = clients.FirstOrDefault(c => c.Name == _clientName);
			return client != null;
		}

		public string GetHumanReadableId()
		{
			return $"named '{_clientName}'";
		}
	}

	public class ApplicableById : IApplicableClient
	{
		private readonly string _clientId;

		public ApplicableById(string clientId)
		{
			_clientId = clientId;
		}

		public bool TryGetMatchingClient(ICollection<Client> clients, out Client client)
		{
			client = clients.FirstOrDefault(c => c.ClientId == _clientId);
			return client != null;
		}

		public string GetHumanReadableId()
		{
			return $"with id '{_clientId}'";
		}
	}
}

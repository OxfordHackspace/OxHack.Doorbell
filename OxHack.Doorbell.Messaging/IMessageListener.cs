using System.Threading.Tasks;

namespace OxHack.Doorbell.Messaging
{
	public interface IMessageListener
	{
		Task Connect();
		Task Disconnect();
	}
}
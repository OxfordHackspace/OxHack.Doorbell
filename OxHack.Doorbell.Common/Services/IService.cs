using System.Threading.Tasks;

namespace OxHack.Doorbell.Common.Services
{
	public interface IService
	{
		Task Start();
		Task Stop();
	}
}
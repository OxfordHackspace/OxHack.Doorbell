using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxHack.Doorbell.Notifier.Extensions
{
	public static class Extensions
	{
		public static IEnumerable<T> Yield<T>(this T item)
		{
			yield return item;
		}
	}
}

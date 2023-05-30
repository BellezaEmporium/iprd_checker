using System.Configuration;

namespace Properties
{
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		public static Settings Default
		{
			get
			{
				return defaultInstance;
			}
		}
		
		private static Settings defaultInstance = (Settings)Synchronized(new Settings());
	}
}

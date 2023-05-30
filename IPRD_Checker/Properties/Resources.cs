using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Properties
{
    internal class Resources
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                var flag = resourceMan == null;
                if (flag)
                {
                    resourceMan = new ResourceManager("IPRD_Checker._5.Properties.Resources", typeof(Resources).Assembly);
                }
                return resourceMan;
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }

        private static ResourceManager resourceMan;
    }
}

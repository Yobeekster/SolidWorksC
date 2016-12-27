﻿using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AngelSix.SolidDna
{
    public partial class SolidWorksApplication : SolidDnaObject<SldWorks>
    {
        public class SolidWorksPreferences
        {
            /// <summary>
            /// The scaling factor used when exporting as DXF
            /// </summary>
            public double DxfOutputScaleFactor { get { return Dna.Application.GetUserPreferencesDouble(swUserPreferenceDoubleValue_e.swDxfOutputScaleFactor); } }
        }
    }
}

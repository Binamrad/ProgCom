using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public interface PCGUIListener
    {
        void recGUIState(GUIStates g);
    }
}

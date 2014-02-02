using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public class PCStageManager : PartModule, IPCHardware
    {
        int lastInput = 0;
        public void connect()
        {
            
        }

        public void disconnect()
        {
            
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(55, 1);
        }

        public int getSegmentCount()
        {
            return 1;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            
        }

        public int memRead(ushort position)
        {
            return lastInput;
        }

        public void memWrite(ushort position, int value)
        {
            lastInput = value;
            ActionGroupList a = this.vessel.ActionGroups;
            switch (value) {
                case 1:
                    a.ToggleGroup(KSPActionGroup.Custom01);
                    break;
                case 2:
                    a.ToggleGroup(KSPActionGroup.Custom02);
                    break;
                case 3:
                    a.ToggleGroup(KSPActionGroup.Custom03);
                    break;
                case 4:
                    a.ToggleGroup(KSPActionGroup.Custom04);
                    break;
                case 5:
                    a.ToggleGroup(KSPActionGroup.Custom05);
                    break;
                case 6:
                    a.ToggleGroup(KSPActionGroup.Custom06);
                    break;
                case 7:
                    a.ToggleGroup(KSPActionGroup.Custom07);
                    break;
                case 8:
                    a.ToggleGroup(KSPActionGroup.Custom08);
                    break;
                case 9:
                    a.ToggleGroup(KSPActionGroup.Custom09);
                    break;
                case 10:
                    a.ToggleGroup(KSPActionGroup.Custom10);
                    break;
                case 11:
                    a.ToggleGroup(KSPActionGroup.Abort);
                    break;
                case 12:
                    if (Staging.CurrentStage > 0) {
                        Staging.ActivateNextStage();
                    }
                    break;
            }
        }

        public void tick(int ticks)
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProgCom
{
    //this module keeps track of navigation info, height over sea, orientation etc.
    //device mapped to 4, length = plenty
    //throws interrupt 257 at update
    public class PCNavModule : PartModule, IPCHardware
    {
        Int32[] mem;
        Int32[] accs;
        InterruptHandle inth;

        //TODO:
        //  add a way to read the radar altitude
        //  bring the vector precision things into the memory space reserved here, currently this class throws a lot of indexoutofrangeexceptions


        public void connect()
        {
            return;
        }

        public void disconnect()
        {
            return;
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            if (id == 0) {
                return new Tuple<UInt16, int>(4, 28);
            } else if (id == 1) {
                return new Tuple<UInt16, int>(41, 3);
            }
            return null;
        }

        public int getSegmentCount()
        {
            return 2;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            inth = seg;
        }

        public int memRead(ushort position)
        {
            if (position < 32) {
                return mem[position];
            } else {
                return accs[position - 41];
            }
        }

        public void memWrite(ushort position, int value)
        {
            if (position < 32) {
                mem[position] = value;
            } else {
                accs[position - 41] = value;
            }
        }

        public void tick(int ticks)
        {
            return;
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            mem = new Int32[32];//we don't need to be memory-efficient here, so we store the first 4 words too for simplicity
            accs = new Int32[3];
            accs[0] = 1024;//init vector precision
            accs[2] = 16;//default speed precision
        }

        public override void OnUpdate()
        {
            //update the variables which describe the vessels location
            //position of the ship's center of mass:
            Vector3d position = vessel.findWorldCenterOfMass();//somewhat useless for anything other than as a parameter
            double altitude = vessel.mainBody.GetAltitude(position);

            //unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
            Vector3d eastUnit = vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
            Vector3d upUnit = (position - vessel.mainBody.position).normalized;
            Vector3d northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

            //vessel speeds
            Vector3d orbitalSpeed = vessel.orbit.GetVel();
            Vector3d groundSpeed = orbitalSpeed - vessel.mainBody.getRFrmVel(position);

            //local acceleration vector due to gravity:
            Vector3d geeAcceleration = FlightGlobals.getGeeForceAtPosition(position);

            //find angular velocity (roll rate, yaw rate, pitch rate)
            Vector3d angularVelocity = (Vector3d)vessel.transform.InverseTransformDirection(vessel.rigidbody.angularVelocity);

            //find ship coordinate system
            UnityEngine.Vector3 tmpVector = new UnityEngine.Vector3(1, 0, 0);
            Vector3d shipEastVector = (Vector3d)vessel.transform.TransformDirection(tmpVector);
            tmpVector.x = 0;
            tmpVector.y = 1;
            Vector3d shipUpVector = (Vector3d)vessel.transform.TransformDirection(tmpVector);
            tmpVector.y = 0;
            tmpVector.z = 1;
            Vector3d shipNorthVector = -(Vector3d)vessel.transform.TransformDirection(tmpVector);
            //figure out what these newfangled vector3d doodads do
            /*
            print("***************************************************");
            print("geeAccelleration: " + (UnityEngine.Vector3)geeAcceleration);
            print("altitude: " + altitude);
            print("eastUnit: " + (UnityEngine.Vector3)eastUnit);
            print("upUnit: " + (UnityEngine.Vector3)upUnit);
            print("northUnit: " + (UnityEngine.Vector3)northUnit);
            print("orbitalSpeed: " + (UnityEngine.Vector3)orbitalSpeed);
            print("groundSpeed: " + (UnityEngine.Vector3)groundSpeed);
            print("heading: " + (UnityEngine.Vector3)shipUpVector);
            print("angularveclocity: " + (UnityEngine.Vector3)angularVelocity);
            print("shipEast: " + (UnityEngine.Vector3)shipEastVector);
            print("shipUp: " + (UnityEngine.Vector3)shipUpVector);
            print("shipNorth: " + (UnityEngine.Vector3)shipNorthVector);
            print("***************************************************");
            */
            //this needs to be updated to reflect the new stuff
            int accuracy = accs[0];//vector accuracy
            float speedAccuracy = accs[2] >= 0 ? (float)accs[2] : 1.0f / ((float)-accs[2]);//speed accuracy
            //int speedAccuracy = 16;//old speedaccuracy model
            
            //this is stolen from the kerbal engineer code, which apparently store it from ISA mapsat according to the source
            //I have no idea why this works, but it does
            Vector3d rad = QuaternionD.AngleAxis(this.vessel.longitude, Vector3d.down) * QuaternionD.AngleAxis(this.vessel.latitude, Vector3d.forward) * Vector3d.right;
            accs[1] = (int)(altitude - (this.vessel.mainBody.pqsController.GetSurfaceHeight(rad) - this.vessel.mainBody.pqsController.radius));

            mem[4] = (Int32)(eastUnit.x * accuracy);
            mem[5] = (Int32)(eastUnit.y * accuracy);
            mem[6] = (Int32)(eastUnit.z * accuracy);
            mem[7] = (Int32)(upUnit.x * accuracy);
            mem[8] = (Int32)(upUnit.y * accuracy);
            mem[9] = (Int32)(upUnit.z * accuracy);
            mem[10] = (Int32)(northUnit.x * accuracy);
            mem[11] = (Int32)(northUnit.y * accuracy);
            mem[12] = (Int32)(northUnit.z * accuracy);
            mem[13] = (Int32)(shipEastVector.x * accuracy);
            mem[14] = (Int32)(shipEastVector.y * accuracy);
            mem[15] = (Int32)(shipEastVector.z * accuracy);
            mem[16] = (Int32)(shipUpVector.x * accuracy);
            mem[17] = (Int32)(shipUpVector.y * accuracy);
            mem[18] = (Int32)(shipUpVector.z * accuracy);
            mem[19] = (Int32)(shipNorthVector.x * accuracy);
            mem[20] = (Int32)(shipNorthVector.y * accuracy);
            mem[21] = (Int32)(shipNorthVector.z * accuracy);
            mem[22] = (Int32)(orbitalSpeed.x * speedAccuracy);
            mem[23] = (Int32)(orbitalSpeed.y * speedAccuracy);
            mem[24] = (Int32)(orbitalSpeed.z * speedAccuracy);
            mem[25] = (Int32)(groundSpeed.x * speedAccuracy);
            mem[26] = (Int32)(groundSpeed.y * speedAccuracy);
            mem[27] = (Int32)(groundSpeed.z * speedAccuracy);
            mem[28] = (Int32)(angularVelocity.x * speedAccuracy);
            mem[29] = (Int32)(angularVelocity.y * speedAccuracy);
            mem[30] = (Int32)(angularVelocity.z * speedAccuracy);
            mem[31] = (Int32)(UInt32)(altitude);

            //give the update interrupt to the processor
            if(accs[1] != 0)
                inth.interrupt(257);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProgCom
{
    class CacheManager
    {
        MemoryManager mem;
        //Int32[] memory;
        UInt16[,] memLoadedLocations;//TODO:should probably change to [][]
        UInt16[,] insLoadedLocations;
        int[] memLastLoaded; //block MRU
        int[] insLastLoaded; //the scheme I'm using for row replacement is kind of LRU. It is LRU with two blocks, with more it kind of falls apart.
        const int memCacheSize = 256;//must be a power of two
        const int insCacheSize = 256;//ditto
        const int rowSize = 8;//must be a power of two
        const int blocks = 2;//must be a number of two
        const int firstWordDelay = 2;//number of cycles until the first word appears on cache load
        const int wordFetchRate = 2;//number of cycles for each subsequent word on cache load

        //check if address addr in in the cache
        private bool isInCache(UInt16 addr, UInt16[,] cache, int[] loadOrder)
        {
            int row = adressRow(addr, cache);
            for (int i = 0; i < blocks; ++i) {
                UInt16 loc = cache[i, row];
                if (addr >= loc && addr < (loc + rowSize)) {
                    loadOrder[row] = i;
                    return true;
                }
            }
            return false;
        }

        //make sure adress addr is loaded in the cache
        private void loadLoc(UInt16 addr, UInt16[,] cache, int[] loadOrder)
        {
            int row = adressRow(addr, cache);
            loadOrder[row] = (loadOrder[row] + 1) & (blocks - 1);
            cache[loadOrder[row], row] = (UInt16)(addr - (addr & (rowSize - 1)));
        }

        public CacheManager()
        {
            mem = new MemoryManager();
            memLoadedLocations = new UInt16[blocks, memCacheSize / (blocks * rowSize)];
            insLoadedLocations = new UInt16[blocks, insCacheSize / (blocks * rowSize)];
            memLastLoaded = new int[memCacheSize / (blocks * rowSize)];
            insLastLoaded = new int[insCacheSize / (blocks * rowSize)];
        }

        //returns the access time for the specified adress in the specifed cache
        private int getAccessTime(UInt16 address, UInt16[,] cache, int[] loadOrder)
        {
            if (!isInCache(address, cache, loadOrder)) {
                //if the address is uncacheable, do a spearate process for hardware memory access here
                int devices = mem.devicesInRange((ushort)(address & (0xffff - rowSize + 1)), rowSize);
                if (devices > 0) {
                    return firstWordDelay + (devices >> 1) - ((devices+2)>>3);//approx log2(devices) + delay
                } else {
                    loadLoc(address, cache, loadOrder);
                    return firstWordDelay + (rowSize - 1) * wordFetchRate;
                }
            } else return 1;
        }

        //for checking if a particular location is cached.
        public bool isCached(UInt16 adress)
        {
            return isInCache(adress, memLoadedLocations, memLastLoaded);
        }

        //writes the chosen int to the specified memory location and returns the access time
        public int writeMem(Int32 data, UInt16 adress)
        {
            mem.memoryWrite(adress, data);
            //memory[adress] = data;
            return getAccessTime(adress, memLoadedLocations, memLastLoaded);

        }

        //returns the memory at the specified adress and makes the second parameter into the access time
        public Int32 readMem(UInt16 adress, out int accessTime)
        {
            accessTime = getAccessTime(adress, memLoadedLocations, memLastLoaded);
            return mem.memoryRead(adress);
        }

        //returns the instruction at the specified adress and makes the second parameter into the access time
        public UInt32 instructionLoad(UInt16 adress, out int accessTime)
        {
            accessTime = getAccessTime(adress, insLoadedLocations, insLastLoaded);
            return (UInt32)mem.memoryRead(adress);
        }

        //return the row in the cache memory the memory location would be stored at
        private int adressRow(int adress, UInt16[,] cache)
        {
            return (adress & ((cache.GetLength(1) - 1) * rowSize)) / rowSize;
        }

        public Int32 getMem(UInt16 address) {
            return mem.memoryRead(address);
        }

        public Int32[] Memory
        {
            get
            {
                return mem.Memory();
            }
        }

        public void hwConnect(IPCHardware hw)
        {
            mem.map(hw);
        }
    }
}
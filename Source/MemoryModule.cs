using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MemoryModule
{
    Int32[] memory;
    UInt16[,] memLoadedLocations;//TODO:should probably change to [][]
    UInt16[,] insLoadedLocations;
    int[] memLastLoaded; //block MRU
    int[] insLastLoaded; //the scheme I'm using for row replacement is kind of LRU. It is LRU with two blocks, with more it kind of falls apart.
    int memCacheSize;
    int insCacheSize;
    int rowSize;
    int blocks;

    //check if address addr in in the cache
    private bool isInCache(UInt16 addr, UInt16[,] cache, int[] loadOrder) {
        int row = adressRow(addr, cache);
        for(int i = 0; i < blocks; ++i) {
            UInt16 loc = cache[i, row];
            if(addr >= loc && addr < (loc + rowSize) ) {
                loadOrder[row] = i;
                return true;
            }
        }
        return false;
    }

    //make sure adress addr is loaded in the cache
    private void loadLoc(UInt16 addr, UInt16[,] cache, int[] loadOrder) {
        int row = adressRow(addr, cache);
        loadOrder[row] = (loadOrder[row] + 1) & (blocks - 1);
        cache[loadOrder[row], row] = (UInt16)(addr - (addr & (rowSize - 1)));
    }

    public MemoryModule() {
        memCacheSize = 256;
        insCacheSize = 256;
        blocks = 2;
        rowSize = 8;
        memLoadedLocations = new UInt16[blocks,memCacheSize/(blocks*rowSize)];
        insLoadedLocations = new UInt16[blocks,insCacheSize/(blocks*rowSize)];
        memLastLoaded = new int[memCacheSize/(blocks*rowSize)];
        insLastLoaded = new int[insCacheSize/(blocks*rowSize)];

        memory = new Int32[1024*64];
    }

    //returns the access time for the specified adress in the specifed cache
    private int getAccessTime(UInt16 adress, UInt16[,] cache, int[] loadOrder) {
        if (adress < 128) {
            return 2;
        } else if (!isInCache(adress, cache, loadOrder)) {
            loadLoc(adress, cache, loadOrder);
            return 9 + rowSize;
        }
        else return  1;
    }

    //for checking if a particular location is cached.
    public bool isCached(UInt16 adress)
    {
        return isInCache(adress, memLoadedLocations, memLastLoaded);
    }

    //writes the chosen int to the specified memory location and returns the access time
    public int writeMem(Int32 data, UInt16 adress) {
        memory[adress] = data;
        return getAccessTime(adress, memLoadedLocations, memLastLoaded);

    }

    //returns the memory at the specified adress and makes the second parameter into the access time
    public Int32 readMem(UInt16 adress, out int accessTime ) {
        accessTime = getAccessTime(adress, memLoadedLocations, memLastLoaded);
        return memory[adress];
    }

    //returns the instruction at the specified adress and makes the second parameter into the access time
    public UInt32 instructionLoad(UInt16 adress, out int accessTime) {
        accessTime = getAccessTime(adress, insLoadedLocations, insLastLoaded);
        return (UInt32) memory[adress];
    }

    //return the row in the cache memory the memory location would be stored at
    private int adressRow(int adress, UInt16[,] cache)
    {
        //I don't think this works
        //return (adress & (cache.GetLength(1) - 1)/*Removes unneccecary zeroes at the high bits*/) / rowSize;
        return (adress & ((cache.GetLength(1) - 1) * rowSize)) / rowSize;
    }

    //returns the memory array
    public Int32[] Memory
    {
        get { return memory; }
    }
}

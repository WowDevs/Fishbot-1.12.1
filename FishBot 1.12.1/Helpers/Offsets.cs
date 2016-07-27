using System;

namespace FishBot
{
    public static class Offsets
    {
        public static IntPtr CurMgrPointer = new IntPtr(0x00741414); // 1.12.1
        public static IntPtr CurMgrOffset = new IntPtr(0xAC); // 1.12.1
        public static IntPtr FirstObjectOffset = new IntPtr(0xAC); // 1.12.1
        public static IntPtr NextObjectOffset = new IntPtr(0x3C); // 1.12.1
        public static int Type = 0x14; // 1.12.1
        public static int LocalGUID = 0x30; // 1.12.1

        public static IntPtr MouseOverGUID = new IntPtr(0x00B4E2C8); // 1.12.1

        public static int BobberState = 0xE8; // 1.12.1

        public static int ObjectName1 = 0x214; // 1.12.1
        public static int ObjectName2 = 0x8; // 1.12.1
        public static int UnitName1 = 0xB30; // 1.12.1
        public static int UnitName2 = 0x0; // 1.12.1

        public static IntPtr PlayerName = new IntPtr(0x827D88); // 1.12.1
        public static IntPtr TargetGUID = new IntPtr(0x74E2D4); // 1.12.1
        public static IntPtr PlayerBase = new IntPtr(0x853D40); // 1.12.1

        public static IntPtr FrameScript__Execute = new IntPtr(0x304CD0); // 1.12.1
        public static IntPtr ClntObjMgrGetActivePlayerObj = new IntPtr(0x168550); // 1.12.1 ClntObjMgrGetActivePlayer 
        public static IntPtr FrameScript_GetText = new IntPtr(0x303BF0); // 1.12.1

        public static IntPtr OnRightClickObject = new IntPtr(0x005F8660); // 1.12.1
        public static IntPtr OnRightClickUnit = new IntPtr(0x60BEA0); // 1.12.1
    }
}

// http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/347720-wow-4-3-4-15595-info-dump-thread.html

// https://subversion.assembla.com/svn/nova-project/

// https://www.assembla.com/code/nova-project/subversion/nodes/96/trunk/Old%2520Profiles/4.3%2520Patch/Offsets/Offsets_15595.xml

/*
Offsets_15595.xml
<?xml version="1.0" encoding="UTF-8"?>
<Offsets>
    <CurrentWoWVersion>15595</CurrentWoWVersion>
    <WoWVersionOffset>0x99B1CF</WoWVersionOffset>
    <PlayerName>0x9BE820</PlayerName>
    <PlayerClass>0x9BE99D</PlayerClass>
    <GetCurrentKeyBoardFocus>0x9D39FC</GetCurrentKeyBoardFocus>
    <GameState>0xAD7426</GameState>
    <Lua_DoStringAddress>0x43C230</Lua_DoStringAddress>
    <Lua_GetLocalizedTextAddress>0x1BBBF0</Lua_GetLocalizedTextAddress>
    <CVarBaseMgr>0xA4D3A8</CVarBaseMgr>
    <CVarArraySize>0x200</CVarArraySize>
    <ObjMgr>0x9BE7E0</ObjMgr>
    <CurMgr>0x463C</CurMgr>
    <LocalGUID>0xC8</LocalGUID>
    <FirstObject>0xC0</FirstObject>
    <NextObject>0x3C</NextObject>
    <Descriptors>0xC</Descriptors>
    <Obj_TypeOffset>0x14</Obj_TypeOffset>
    <Obj_X>0x790</Obj_X>
    <Obj_TargetGUID>0x50</Obj_TargetGUID>
</Offsets>
*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace FishBot
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Lua
    {
        private readonly Hook _wowHook;
        public Lua(Hook wowHook)
        {
            _wowHook = wowHook;
        }

        public void DoString(string command)
        {
            if (!_wowHook.Installed) return;

            // Allocate memory
            var doStringArgCodecave = _wowHook.Memory.AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            // Write value:
            _wowHook.Memory.WriteBytes(doStringArgCodecave, Encoding.UTF8.GetBytes(command));

            var asm = new[] 
            {
                "mov ecx, " + doStringArgCodecave,
                "mov edx, " + doStringArgCodecave,
                "call " + ((uint) Offsets.FrameScript__Execute + _wowHook.Process.BaseOffset()),  // Lua_DoString   
                "retn",    
            };
                
            // Inject
            _wowHook.InjectAndExecute(asm);
            // Free memory allocated 
            _wowHook.Memory.FreeMemory(doStringArgCodecave);
        }

        //internal string GetLocalizedText(string localVar)
        //{
        //    if (_wowHook.Installed)
        //    {
        //        IntPtr Lua_GetLocalizedText_Space = _wowHook.Memory.AllocateMemory(Encoding.UTF8.GetBytes(localVar).Length + 1);

        //        _wowHook.Memory.Write<byte>(Lua_GetLocalizedText_Space, Encoding.UTF8.GetBytes(localVar), false);

        //        String[] asm = new String[] 
        //        {
        //            "call " + ((uint) Offsets.ClntObjMgrGetActivePlayerObj + _wowHook.Process.BaseOffset()  ),
        //            "mov ecx, eax",
        //            "push -1",
        //            "mov edx, " + Lua_GetLocalizedText_Space + "",
        //            "push edx",
        //            "call " + ((uint) Offsets.FrameScript__GetLocalizedText + _wowHook.Process.BaseOffset() ) ,                    
        //            "retn",
        //        };

        //        string sResult = Encoding.UTF8.GetString(_wowHook.InjectAndExecute(asm));

        //        // Free memory allocated 
        //        _wowHook.Memory.FreeMemory(Lua_GetLocalizedText_Space);
        //        return sResult;
        //    }
        //    return "WoW Hook not installed";
        //}

        public void SendTextMessage(string message)
        {
            //DoString(string.Format("SendChatMessage(\"" + message + "\", \"EMOTE\", nil, \"General\")"));

            DoString("RunMacroText('/me " + message + "')");
        }

        public void CastSpellByName(string spell)
        {
            DoString($"CastSpellByName('{spell}')");
            SendTextMessage("Casting: " + spell);
        }

        public void ctm(float x, float y, float z, ulong guid, int action, float precision, IntPtr playerBaseAddress)
        {
            // Offset:
            const uint CGPlayer_C__ClickToMove = 0x727400;

            // Allocate Memory:
            var GetNameVMT_Codecave = _wowHook.Memory.AllocateMemory(0x3332);
            var Pos_Codecave = _wowHook.Memory.AllocateMemory(0x4 * 3);
            var GUID_Codecave = _wowHook.Memory.AllocateMemory(0x8);
            var Angle_Codecave = _wowHook.Memory.AllocateMemory(0x4);

            // Write value:
            _wowHook.Memory.Write(GUID_Codecave, guid);
            _wowHook.Memory.Write(Angle_Codecave, precision);
            _wowHook.Memory.Write(Pos_Codecave, x);
            _wowHook.Memory.Write(Pos_Codecave + 0x4, y);
            _wowHook.Memory.Write(Pos_Codecave + 0x8, z);

            try
            {
                // BOOL __thiscall CGPlayer_C__ClickToMove(WoWActivePlayer *this, CLICKTOMOVETYPE clickType, WGUID *interactGuid, WOWPOS *clickPos, float precision)
                _wowHook.Memory.Asm.Clear();

                _wowHook.Memory.Asm.AddLine("mov edx, [" + Angle_Codecave + "]");
                _wowHook.Memory.Asm.AddLine("push edx");

                _wowHook.Memory.Asm.AddLine("push " + Pos_Codecave);
                _wowHook.Memory.Asm.AddLine("push " + GUID_Codecave);
                _wowHook.Memory.Asm.AddLine("push " + action);

                _wowHook.Memory.Asm.AddLine("mov ecx, " + playerBaseAddress);
                _wowHook.Memory.Asm.AddLine("call " + CGPlayer_C__ClickToMove);
                _wowHook.Memory.Asm.AddLine("retn");

                _wowHook.Memory.Asm.InjectAndExecute((uint)GetNameVMT_Codecave);
            }
            catch
            {
                // ignored
            }

            _wowHook.Memory.FreeMemory(GetNameVMT_Codecave);
            _wowHook.Memory.FreeMemory(Pos_Codecave);
            _wowHook.Memory.FreeMemory(GUID_Codecave);
            _wowHook.Memory.FreeMemory(Angle_Codecave);
        }

        internal void OnRightClickObject(uint baseAddr, int autoLoot)
        {
            if (!_wowHook.Installed) return;
            if (baseAddr == 0 || (autoLoot != 1 && autoLoot != 0)) return;

            // Write the asm stuff for Lua_DoString
            var asm = new[]
            {
                "push " + autoLoot,
                "mov ECX, " + baseAddr,
                "call " + (uint)Offsets.OnRightClickObject,
                "retn",
            };

            _wowHook.InjectAndExecute(asm);
        }

        public void TargetUnit(ulong guid)  // Should work with 1.12.1
                                            // Source = post 69 on page below
        {                                   // http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/328263-wow-1-12-1-5875-info-dump-thread-3.html
            // Allocate memory
            var ptr = _wowHook.Memory.AllocateMemory(8);
            // Write value:
            _wowHook.Memory.Write(ptr, guid);

            string[] asm =
                {
                    "mov ecx, " + ptr,
                    "call " + 0x489A40,
                    "retn"
                };

            // Inject
            _wowHook.InjectAndExecute(asm);
            // Free memory allocated 
            _wowHook.Memory.FreeMemory(ptr);
        }
    }
}

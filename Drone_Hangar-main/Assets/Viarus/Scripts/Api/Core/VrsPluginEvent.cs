













using UnityEngine;
using System;



namespace Vrs.Internal
{
    public enum ViarusRenderEventType
    {
        
        InitRenderThread = 0,
        Pause = 1,
        Resume = 2,
        LeftEyeEndFrame = 3,
        RightEyeEndFrame = 4,
        TimeWarp = 5,
        PlatformUI = 6,
        PlatformUIConfirmQuit = 7,
        ResetVrModeParms = 8,
        PlatformUITutorial = 9,
        ShutdownRenderThread = 10,
        LeftEyeBeginFrame = 11,
        RightEyeBeginFrame = 12,
        PrepareFrame = 13,
        BeginVR=14,
        EndVR=15,
        ShutDown=16,
        Event_Extern_Texture = 17
    }

    
    
    
    public static class VrsPluginEvent
    {
        
        
        
        public static void Issue(ViarusRenderEventType eventType)
        {
            GL.IssuePluginEvent(EncodeType((int)eventType));
        }

        
        
        
        
        
        
        public static void IssueWithData(ViarusRenderEventType eventType, int eventData)
        {
            
            GL.IssuePluginEvent(EncodeData((int)eventType, eventData, 0));

            
            GL.IssuePluginEvent(EncodeData((int)eventType, eventData, 1));

            
            GL.IssuePluginEvent(EncodeType((int)eventType));
        }

        
        
        
        
        
        
        
        
        
        
        
        private const UInt32 IS_DATA_FLAG = 0x80000000;
        private const UInt32 DATA_POS_MASK = 0x40000000;
        private const int DATA_POS_SHIFT = 30;
        private const UInt32 EVENT_TYPE_MASK = 0x3E000000;
        private const int EVENT_TYPE_SHIFT = 25;
        private const UInt32 PAYLOAD_MASK = 0x0000FFFF;
        private const int PAYLOAD_SHIFT = 16;

        private static int EncodeType(int eventType)
        {
            return (int)((UInt32)eventType & ~IS_DATA_FLAG); 
        }

        private static int EncodeData(int eventId, int eventData, int pos)
        {
            UInt32 data = 0;
            data |= IS_DATA_FLAG;
            data |= (((UInt32)pos << DATA_POS_SHIFT) & DATA_POS_MASK);
            data |= (((UInt32)eventId << EVENT_TYPE_SHIFT) & EVENT_TYPE_MASK);
            data |= (((UInt32)eventData >> (pos * PAYLOAD_SHIFT)) & PAYLOAD_MASK);

            return (int)data;
        }

        private static int DecodeData(int eventData)
        {
            
            UInt32 pos = (((UInt32)eventData & DATA_POS_MASK) >> DATA_POS_SHIFT);
            
            UInt32 payload = (((UInt32)eventData & PAYLOAD_MASK) << (PAYLOAD_SHIFT * (int)pos));

            return (int)payload;
        }
    }
}
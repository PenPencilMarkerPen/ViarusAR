using UnityEngine;
namespace Vrs.Internal
{
    public class ViarusSensorEvent
    {
        public float x;
        public float y;
        public float z;
        public long timestamp;
        public SENSOR_LOCATION sensorLocation;
        public SENSOR_TYPE sensorType;

        public ViarusSensorEvent(float x, float y, float z, long timestamp, SENSOR_TYPE sensorType, SENSOR_LOCATION sensorLocation)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.timestamp = timestamp;
            this.sensorLocation = sensorLocation;
            this.sensorType = sensorType;
        }

        public void printLog()
        {
            Debug.Log("ViarusSensorEvent{" +
                    "sensorLocation=" + sensorLocation +
                    ", sensorType=" + sensorType +
                    ", x=" + x +
                    ", y=" + y +
                    ", z=" + z +
                    ", timestamp=" + timestamp +
                    "}");
        }
    }
}
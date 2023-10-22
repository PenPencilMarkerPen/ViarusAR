
















using UnityEngine;
namespace Vrs.Internal
{
    public class Pose3D
    {
        
        protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));

        
        public Vector3 Position { get; protected set; }

        
        public Quaternion Orientation { get; protected set; }

        
        public Matrix4x4 Matrix { get; protected set; }

        
        public Matrix4x4 RightHandedMatrix
        {
            get
            {
                return flipZ * Matrix * flipZ;
            }
        }

        
        
        public Pose3D()
        {
            Position = Vector3.zero;
            Orientation = Quaternion.identity;
            Matrix = Matrix4x4.identity;
        }

        
        public Pose3D(Vector3 position, Quaternion orientation)
        {
            Set(position, orientation, false);
        }

        public Pose3D(Vector3 position, Quaternion orientation, bool updateMatrix)
        {
            Set(position, orientation, updateMatrix);
        }

        
        public Pose3D(Matrix4x4 matrix)
        {
            Set(matrix);
        }

        protected void Set(Vector3 position, Quaternion orientation, bool updateMatrix)
        {
            Position = position;
            Orientation = orientation;
            if (updateMatrix)
            {
                Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
            }
        }

        protected void Set(Matrix4x4 matrix)
        {
            Matrix = matrix;
            Position = matrix.GetColumn(3);
            Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }

        public void SetPosition(Vector3 position)
        {
            Position = position*VrsViewer.Instance.DisplacementCoefficient;
        }

    }
    

    
    
    public class MutablePose3D : Pose3D
    {
        
        public new void Set(Vector3 position, Quaternion orientation)
        {
            base.Set(position, orientation, false);
        }

        public new void Set(Vector3 position, Quaternion orientation, bool updateMatrix)
        {
            base.Set(position, orientation, updateMatrix);
        }

        
        public new void Set(Matrix4x4 matrix)
        {
            base.Set(matrix);
        }

        
        public void SetRightHanded(Matrix4x4 matrix)
        {
            Set(flipZ * matrix * flipZ);
        }

        public new void SetPosition(Vector3 position)
        {
            base.SetPosition(position);
        }
    }
}


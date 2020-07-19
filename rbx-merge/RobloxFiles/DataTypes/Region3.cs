﻿using System;

namespace RobloxFiles.DataTypes
{
    public class Region3
    {
        public readonly Vector3 Min, Max;

        public Vector3 Size => (Max - Min);
        public CFrame CFrame => new CFrame((Min + Max) / 2);

        public override string ToString() => $"{CFrame}; {Size}";

        public Region3(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        internal Region3(Attribute attr)
        {
            Min = new Vector3(attr);
            Max = new Vector3(attr);
        }
        
        public Region3 ExpandToGrid(float resolution)
        {
            Vector3 emin = new Vector3
            (
                (float)Math.Floor(Min.X) * resolution,
                (float)Math.Floor(Min.Y) * resolution,
                (float)Math.Floor(Min.Z) * resolution
            );

            Vector3 emax = new Vector3
            (
                (float)Math.Floor(Max.X) * resolution,
                (float)Math.Floor(Max.Y) * resolution,
                (float)Math.Floor(Max.Z) * resolution
            );

            return new Region3(emin, emax);
        }
    }
}

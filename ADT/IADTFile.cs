﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.ADT
{
    public abstract class IADTFile
    {
        public abstract void RenderADT();
        public abstract bool Intersect(Ray ray, ref float height);
        public abstract void ChangeTerrain(Vector3 pos, bool lower);
        public abstract void FlattenTerrain(Vector3 pos, bool lower);
        public abstract void BlurTerrain(Vector3 pos, bool lower);
        public abstract void Unload();
        public abstract void WaitLoad();
        public abstract IADTChunk GetChunk(uint index);

        public abstract List<string> TextureNames { get; }
        public string FileName { get; protected set; }
        public uint IndexX { get; protected set; }
        public uint IndexY { get; protected set; }
        public List<Wotlk.MDDF> ModelDefinitions { get; protected set; }
        public List<uint> ModelIdentifiers { get; protected set; }
        public Dictionary<uint, string> DoodadNames = new Dictionary<uint, string>();
    }
}
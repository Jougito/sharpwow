﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.MDX
{
    public class M2ModelMover : IModelMover
    {
        public M2ModelMover(MdxIntersectionResult result)
        {
            mResult = result;
        }

        public void rotateModel(SlimDX.Vector3 axis, float amount)
        {
            var newData = mResult.InstanceData;
            var invWorld = SlimDX.Matrix.RotationAxis(axis, (amount / 180.0f) * (float)Math.PI) * newData.ModelMatrix;
            newData.ModelMatrix = invWorld;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 axis, float amount)
        {
            var newData = mResult.InstanceData;
            var newMatrix = newData.ModelMatrix * SlimDX.Matrix.Translation(axis * amount);
            newData.ModelMatrix = newMatrix;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        public void moveModel(SlimDX.Vector3 newPos)
        {
            var newData = mResult.InstanceData;
            Vector3 oldPos = new Vector3(mResult.InstanceData.ModelMatrix.M41, mResult.InstanceData.ModelMatrix.M42, mResult.InstanceData.ModelMatrix.M43);
            var diff = newPos - oldPos;
            var newMatrix = Matrix.Translation(diff) * mResult.InstanceData.ModelMatrix;
            newData.ModelMatrix = newMatrix;
            mResult.InstanceData = newData;
            mResult.Renderer.InstanceLoader.setInstance(mResult.InstanceID, mResult.InstanceData);
            if (ModelChanged != null)
                ModelChanged(mResult.InstanceData.ModelMatrix);
        }

        MdxIntersectionResult mResult;

        public event Action<SlimDX.Matrix> ModelChanged;
    }
}

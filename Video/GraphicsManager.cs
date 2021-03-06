﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;
using SlimDX;

namespace SharpWoW.Video
{
    public class GraphicsManager
    {
        public GraphicsManager(Control dstWindow)
        {
            mRenderWindow = dstWindow;
            mRenderWindow.MouseMove += new MouseEventHandler(MouseMoved);
            Input.InputManager.Input.InputWindow = mRenderWindow;
            Picking.InitPicking();
        }

        void MouseMoved(object sender, MouseEventArgs e)
        {
            //UpdateMouseTerrainPos(e.X, e.Y);
        }

        public void CreateDevice(bool useRegistry, bool showDialog = true)
        {
            Direct3D = new SlimDX.Direct3D9.Direct3D();
            VideoConfig cfg = VideoConfig.Load(useRegistry, showDialog);
            CurrentConfig = cfg;

            mPresentParams = new PresentParameters()
                {
                    AutoDepthStencilFormat = cfg.DepthStencilFormat,
                    EnableAutoDepthStencil = true,
                    BackBufferFormat = Format.A8R8G8B8,
                    BackBufferHeight = mRenderWindow.ClientSize.Height,
                    BackBufferWidth = mRenderWindow.ClientSize.Width,
                    DeviceWindowHandle = mRenderWindow.Handle,
                    Windowed = true,
                    Multisample = cfg.Multisampling,
                    MultisampleQuality = (int)cfg.MultisampleQuality
                };

            Device = new Device(Direct3D, cfg.Adapter.Adapter, DeviceType.Hardware, mRenderWindow.Handle,
                CreateFlags.HardwareVertexProcessing,
                mPresentParams
            );

            VideoResourceMgr = new VideoResourceManager();

            mActiveCamera = new PerspectiveCamera();

            Device.SetTransform(TransformState.World, Matrix.Identity);
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);

            if (DeviceLoaded != null)
                DeviceLoaded();
        }

        public void Enter2DMode(bool updateWorld = true)
        {
            if (Camera is OrthogonalCamera)
                return;

            var oldCamera = Camera;
            var newCamera = new OrthogonalCamera();
            newCamera.PreventWorldUpdate = !updateWorld;
            Camera = newCamera;
            float height = 0;
            var position = oldCamera.Position;

            if (Game.GameManager.WorldManager.GetLandHeightFast(position.X, position.Y, ref height))
                position.Z = height;

            Camera.SetPosition(oldCamera.Position);
            Game.GameManager.WorldManager.FogStart = 1000.0f;
            newCamera.PreventWorldUpdate = false;
        }

        public void UpdateMouseTerrainPos(int x, int y)
        {
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            pt = mRenderWindow.PointToClient(pt);

            Vector3 screenCoord = new Vector3();
            screenCoord.X = (((2.0f * pt.X) / Device.Viewport.Width) - 1);
            screenCoord.Y = -(((2.0f * pt.Y) / Device.Viewport.Height) - 1);

            var invProj = Matrix.Invert(Device.GetTransform(TransformState.Projection));
            var invView = Matrix.Invert(Device.GetTransform(TransformState.View));

            var nearPos = new Vector3(screenCoord.X, screenCoord.Y, 0);
            var farPos = new Vector3(screenCoord.X, screenCoord.Y, 1);

            nearPos = Vector3.TransformCoordinate(nearPos, invProj * invView);
            farPos = Vector3.TransformCoordinate(farPos, invProj * invView);

            Ray ray = new Ray(nearPos, Vector3.Normalize((farPos - nearPos)));
            float distance = 0;
            ADT.IADTChunk hitChunk = null;
            bool hit = ADT.ADTManager.Intersect(ray, ref distance, out hitChunk);
            ShaderCollection.TerrainShader.SetValue("DrawMouse", hit);
            if (hit)
            {
                ShaderCollection.TerrainShader.SetValue("MousePosition", ray.Position + distance * ray.Direction);
                MousePosition = ray.Position + distance * ray.Direction;
                Game.GameManager.WorldManager.MouseHoverChunk = hitChunk;
            }
            else
            {
                Game.GameManager.WorldManager.MouseHoverChunk = null;
                MousePosition = new Vector3(999999, 999999, 999999);
            }
        }

        public Vector3 MousePosition
        {
            get;
            private set;
        }

        public void DoDeviceReset()
        {
            while (Device.TestCooperativeLevel().Code != (((1 << 31) | (0x876 << 16) | 2153)))
            {
                System.Threading.Thread.Sleep(0);
            }
            VideoResourceMgr.BeforeReset();
            Device.Reset(mPresentParams);
            VideoResourceMgr.AfterReset();
        }

        Control mRenderWindow;
        PresentParameters mPresentParams;
        ICamera mActiveCamera;

        public Control RenderWindow { get { return mRenderWindow; } }
        public Device Device { get; private set; }
        public Direct3D Direct3D { get; private set; }
        public ICamera Camera { get { return mActiveCamera; } set { mActiveCamera = value; mActiveCamera.DeviceAttached(Device); } }
        public VideoConfig CurrentConfig { get; set; }
        public event Action DeviceLoaded;
        public VideoResourceManager VideoResourceMgr { get; private set; }
        public bool IsIn2D { get { return Camera is OrthogonalCamera; } }
    }
}

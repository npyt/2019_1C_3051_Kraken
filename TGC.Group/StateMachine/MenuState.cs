using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Group.Camara;
using TGC.Group.Core2d;
using TGC.Group.Model;
using TGC.Group.VertexMovement;

namespace TGC.Group.StateMachine
{
    class MenuState : State
    {
        public TimeManager gameTime = new TimeManager();

        List<string> folders = new List<string>();
        List<string> names = new List<string>();
        int selectedLevel = 0;

        private CustomSprite logoSprite;
        private CustomSprite bgSprite;

        Font fontMenu;

        TgcMesh Ship;
        TgcThirdPersonCamera myCamera;
        TgcSkyBox skyBox;

        private TgcMp3Player mp3Player;
        String mp3Path;

        public MenuState(GameModel mparent) : base(mparent)
        {
            init();
        }

        public override void dispose()
        {
            throw new NotImplementedException();
        }

        public override void init()
        {
            loadLevelList();

            mp3Path = parent.MediaDir + "Music\\menumusic.wav";

            logoSprite = new CustomSprite();
            logoSprite.Bitmap = new CustomBitmap(parent.MediaDir + "\\Logo.png", D3DDevice.Instance.Device);
            float sx = Screen.PrimaryScreen.Bounds.Size.Width / 3;
            logoSprite.Scaling = new TGCVector2(sx / logoSprite.Bitmap.Size.Width, sx / logoSprite.Bitmap.Size.Width);
            logoSprite.Position = new TGCVector2(sx, 50);

            bgSprite = new CustomSprite();
            bgSprite.Bitmap = new CustomBitmap(parent.MediaDir + "\\dims.jpg", D3DDevice.Instance.Device);
            bgSprite.Scaling = new TGCVector2(1, 1);
            bgSprite.Position = new TGCVector2(-30, 0);

            fontMenu = new Font("BigNoodleTitling", 30, FontStyle.Regular);
            
            var loader = new TgcSceneLoader();

            // Nave
            Ship = loader.loadSceneFromFile(parent.MediaDir + "Test\\ship-TgcScene.xml").Meshes[0];
            Ship.Move(0, 0, 0);
            Ship.Scale = new TGCVector3(1f, 1f, -1f);

            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(20000, 20000, 20000);
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, parent.MediaDir + "\\dims.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, parent.MediaDir + "\\dims.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, parent.MediaDir + "\\dims.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, parent.MediaDir + "\\dims.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, parent.MediaDir + "\\dims.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, parent.MediaDir + "\\dims.jpg");
            skyBox.SkyEpsilon = 25f;
            skyBox.Init();

            myCamera = new TgcThirdPersonCamera(Ship.Position, 60, -135);
            parent.Camara = myCamera;
        }

        public override void render(float ElapsedTime)
        {
            parent.drawer2D.BeginDrawSprite();
            parent.drawer2D.DrawSprite(logoSprite);
            parent.drawer2D.EndDrawSprite();

            skyBox.Render();

            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance * 2f).ToMatrix();

            int sx = Screen.PrimaryScreen.Bounds.Size.Width / 2;
            int sy = Screen.PrimaryScreen.Bounds.Size.Height / 2;

            parent.DrawText.changeFont(fontMenu);
            for(int i=0; i<folders.Count; i++) {
                string folder = folders[i];
                string name = names[i];

                Color tColor = Color.White;
                if (i == selectedLevel)
                    tColor = Color.Yellow;

                parent.DrawText.drawText(name, sx, sy, tColor);

                sy += 40;
            }
            parent.DrawText.changeFont(parent.defaultFont);

            Ship.Render();
        }

        public override void update(float ElapsedTime)
        {
            gameTime.update(ElapsedTime);

            if(mp3Player == null)
            {
                mp3Player = new TgcMp3Player();
                mp3Player.FileName = mp3Path;
                mp3Player.play(true);
            }

            if(parent.Input.keyPressed(Key.DownArrow))
            {
                selectedLevel++;
                if (selectedLevel == names.Count)
                    selectedLevel--;
            }
            if (parent.Input.keyPressed(Key.UpArrow))
            {
                selectedLevel--;
                if (selectedLevel == -1)
                    selectedLevel++;
            }
            if (parent.Input.keyPressed(Key.RightControl))
            {
                mp3Player.stop();
                mp3Player.closeFile();
                mp3Player = null;
                parent.selectLevel(folders[selectedLevel]);
            }

            Ship.Move(new TGCVector3(0, TGC.Core.Mathematica.FastMath.Sin(gameTime.sum_elapsed * 1.5f) * 0.3f, 0));
            Ship.RotateY(gameTime.counter_elapsed * 0.000003f);

            Ship.Transform = TGCMatrix.Scaling(Ship.Scale) * TGCMatrix.RotationYawPitchRoll(Ship.Rotation.Y, Ship.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(Ship.Position);
        }

        private void loadLevelList()
        {
            string csv_levels = File.ReadAllText(parent.MediaDir + "Levels\\levelIndex.csv", Encoding.UTF8);
            string separator_levels = ";";
            string[] values_levels = Regex.Split(csv_levels, separator_levels);
            string name;
            string folder;
            for (int i = 0; i < values_levels.Length; i++)
            {
                folder = values_levels[i] = values_levels[i].Trim('\"');
                i++;
                name = values_levels[i] = values_levels[i].Trim('\"');

                folders.Add(folder);
                names.Add(name);

                System.Diagnostics.Debug.WriteLine(folder + " - " + name);
            }
            System.Diagnostics.Debug.WriteLine("Levels Loaded");
        }

    }
}

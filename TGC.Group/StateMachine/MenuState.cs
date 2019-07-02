using Microsoft.DirectX.Direct3D;
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
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Group.Camara;
using TGC.Group.Core2d;
using TGC.Group.Model;
using TGC.Group.VertexMovement;
using Font = System.Drawing.Font;

namespace TGC.Group.StateMachine
{
    class MenuState : State
    {
        public TimeManager gameTime = new TimeManager();

        List<string> folders = new List<string>();
        List<string> names = new List<string>();
        int selectedLevel = 0;
        static int scale = 10000;
        TGCVector3 sphereScale = new TGCVector3(scale, scale, scale);

        private CustomSprite logoSprite;
        private CustomSprite bgSprite;

        Font fontMenu;

        TGCSphere skyBox;

        TgcMesh Ship;
        private Microsoft.DirectX.Direct3D.Effect shipEffect;
        private Microsoft.DirectX.Direct3D.Effect skyEffect;
        TgcThirdPersonCamera myCamera;

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

            // Music path
            mp3Path = parent.MediaDir + "Music\\menumusic.wav";

            // Sprites
            logoSprite = new CustomSprite();
            logoSprite.Bitmap = new CustomBitmap(parent.MediaDir + "\\Logo.png", D3DDevice.Instance.Device);
            float sx = Screen.PrimaryScreen.Bounds.Size.Width / 3;
            logoSprite.Scaling = new TGCVector2(sx / logoSprite.Bitmap.Size.Width, sx / logoSprite.Bitmap.Size.Width);
            logoSprite.Position = new TGCVector2(sx, 50);

            bgSprite = new CustomSprite();
            bgSprite.Bitmap = new CustomBitmap(parent.MediaDir + "\\dims.jpg", D3DDevice.Instance.Device);
            bgSprite.Scaling = new TGCVector2(1, 1);
            bgSprite.Position = new TGCVector2(-30, 0);

            System.Drawing.Text.PrivateFontCollection bignoodle = new System.Drawing.Text.PrivateFontCollection();
            bignoodle.AddFontFile(parent.MediaDir + "Fonts\\big_noodle_titling.ttf");
            fontMenu = new Font(bignoodle.Families[0], 30, FontStyle.Regular);
            var loader = new TgcSceneLoader();

            // Nave

            Ship = loader.loadSceneFromFile(parent.MediaDir + "Test\\ship-TgcScene.xml").Meshes[0];
            Ship.Move(0, 0, 0);
            Ship.Scale = new TGCVector3(1f, 1f, -1f);
            
            shipEffect = TGCShaders.Instance.LoadEffect(parent.ShadersDir + "ShipShader.fx");
            Ship.Effect = shipEffect;
            Ship.Technique = "RenderScene";


            // SkyBox
            skyEffect = TGCShaders.Instance.LoadEffect(parent.ShadersDir + "SkyShader.fx");

            var texture = TgcTexture.createTexture(parent.MediaDir + "SkyBox\\universe2.png");
            skyBox = new TGCSphere();
            skyBox.Position = new TGCVector3(0, 0, 0);
            skyBox.Color = Color.White;
            skyBox.setTexture(texture);
            skyBox.LevelOfDetail = 2;
            skyBox.updateValues();
            skyBox.RotateY(FastMath.PI/2);

            // Camara 3ra persona
            myCamera = new TgcThirdPersonCamera(Ship.Position + new TGCVector3(20,0,0), 60, -135);
            parent.Camara = myCamera;
        }

        public override void render(float ElapsedTime)
        {
            var device = D3DDevice.Instance.Device;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            parent.drawer2D.BeginDrawSprite();
            parent.drawer2D.DrawSprite(logoSprite);
            parent.drawer2D.EndDrawSprite();
            
            skyBox.Render();

            // Fix del skybox
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance * 2f).ToMatrix();

            // Opciones del menu
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

            // Shaders de la nave
            shipEffect.SetValue("camaraX", myCamera.Target.X - myCamera.Position.X);
            shipEffect.SetValue("camaraY", myCamera.Target.Y - myCamera.Position.Y);
            shipEffect.SetValue("camaraZ", myCamera.Target.Z - myCamera.Position.Z);
            Ship.Render();
            device.EndScene();
            device.Present();
        }

        public override void update(float ElapsedTime)
        {
            gameTime.update(ElapsedTime);

            // Musica de fondo
            if(mp3Player == null)
            {
                mp3Player = new TgcMp3Player();
                mp3Player.FileName = mp3Path;
                mp3Player.play(true);
            }

            // Interaccion de teclado para menu
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
            if (parent.Input.keyPressed(Key.Return))
            {
                mp3Player.stop();
                mp3Player.closeFile();
                mp3Player = null;
                parent.selectLevel(folders[selectedLevel]);
            }

            // Movimiento de la nave
            Ship.Move(new TGCVector3(0, TGC.Core.Mathematica.FastMath.Sin(gameTime.sum_elapsed * 1.5f) * 0.3f, 0));
            Ship.RotateY(gameTime.counter_elapsed * 0.000003f);

            // Rotación skybox
            skyBox.RotateY(-gameTime.counter_elapsed * 0.0000003f);
            skyBox.RotateZ(-gameTime.counter_elapsed * 0.00000003f / 0.3f);

            // Matrices de transformación
            skyBox.Transform = TGCMatrix.Scaling(sphereScale) * TGCMatrix.RotationYawPitchRoll(skyBox.Rotation.Y, skyBox.Rotation.X, skyBox.Rotation.Z) * TGCMatrix.Translation(skyBox.Position);
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

using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Group.Camara;
using TGC.Core.Terrain;
using TGC.Core.Collision;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        // Nave principal
        private TGCBox Ship { get; set; }

        //PowerBoxes
        private TGCBox ShortPowerBox { get; set; }

        // Nave test
        private TgcMesh shipMesh { get; set; }

        // Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }


        // SkyBox
        private TgcSkyBox skyBox;

        // Camara en tercera persona
        private TgcThirdPersonCamera camaraInterna;

        // Variables varias
        private const float MOVEMENT_SPEED = 200f;
        

        public override void Init()
        {
            // Inicialización del skybox
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(20000, 20000, 20000);
            //skyBox.Color = Color.Black; // Test de skyblock con color fijo
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "SkyBox\\purplenebula_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "SkyBox\\purplenebula_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "SkyBox\\purplenebula_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "SkyBox\\purplenebula_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "SkyBox\\purplenebula_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "SkyBox\\purplenebula_ft.jpg");
            skyBox.SkyEpsilon = 25f;
            skyBox.Init();

            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Textura de la carperta Media. Game.Default es un archivo de configuracion (Game.settings) util para poner cosas.
            //Pueden abrir el Game.settings que se ubica dentro de nuestro proyecto para configurar.
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;
            

            ///
            /// Caja de prueba para la nave
            /// 
           
            var texture = TgcTexture.createTexture(pathTexturaCaja);
            var size = new TGCVector3(5, 5, 10);
            Ship = TGCBox.fromSize(size, texture);
            Ship.Position = new TGCVector3(0, 10, 2000);
            Ship.Transform = TGCMatrix.Identity;

            ///
            /// Caja de prueba para el poder corto
            /// 

            var sizePowerShort = new TGCVector3(10, 2, 10);
            ShortPowerBox = TGCBox.fromSize(sizePowerShort, texture);
            ShortPowerBox.Position = new TGCVector3(0, 10, 2050);
            ShortPowerBox.Transform = TGCMatrix.Identity;

            ///
            /// Mesh de prueba nave desde 3ds
            /// 

            var loader = new TgcSceneLoader();
            shipMesh = loader.loadSceneFromFile(MediaDir + "Test\\test-TgcScene.xml").Meshes[0];
            shipMesh.Move(0, 0, 2000);
            //mainMesh.RotateY(180);


            // Cámara en tercera persona
            camaraInterna = new TgcThirdPersonCamera(Ship.Position, 10, 35);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();

            // Para que no surja el artifact del skybox
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView, D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance, D3DDevice.Instance.ZFarPlaneDistance * 2f).ToMatrix();

            // Detectar colisión entre la nave (Ship) y un poder corto (ShortPowerBox)
            if (TgcCollisionUtils.testAABBAABB(Ship.BoundingBox, ShortPowerBox.BoundingBox))
            {
                ShortPowerBox.Color = Color.Yellow;
                ShortPowerBox.updateValues();
                if (Input.keyDown(Key.Space))
                {
                    ShortPowerBox.Color = Color.Red;
                    ShortPowerBox.updateValues();
                }
            }
            else
            {
                ShortPowerBox.Color = Color.White;
                ShortPowerBox.updateValues();
            }

            // Movimiento de la nave (Ship)
            var movement = TGCVector3.Empty;
            //movement.Z = -1; // Descomentar para movimiento constante
            var originalPos = Ship.Position;
            movement *= MOVEMENT_SPEED * ElapsedTime;
            Ship.Move(movement);

            // Capturar Input teclado para activar o no el bounding box
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            // Movernos adelante y atras, sobre el eje Z.
            if (Input.keyDown(Key.Up) || Input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (Input.keyDown(Key.Down) || Input.keyDown(Key.S))
            {
                movement.Z = 1;
            }

            // Comentar si no se mueve por teclado
            movement *= MOVEMENT_SPEED * ElapsedTime;
            Ship.Move(movement);

            PostUpdate();
        }
        
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Especificaciones en pantalla: posición de la nave y de la cámara
            DrawText.drawText("Box Position: \n" + Ship.Position, 0, 40, Color.Red);
            DrawText.drawText("Camera Position: \n" + Camara.Position, 100, 40, Color.Red);

            // Renders
            shipMesh.Render();
            skyBox.Render();
            Ship.Transform = TGCMatrix.Scaling(Ship.Scale) * TGCMatrix.RotationYawPitchRoll(Ship.Rotation.Y, Ship.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(Ship.Position);
            Ship.Render();
            ShortPowerBox.Transform = TGCMatrix.Scaling(ShortPowerBox.Scale) * TGCMatrix.RotationYawPitchRoll(ShortPowerBox.Rotation.Y, ShortPowerBox.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(ShortPowerBox.Position);
            ShortPowerBox.Render();
            

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                Ship.BoundingBox.Render();
                ShortPowerBox.BoundingBox.Render();
            }

            camaraInterna.Target = Ship.Position;

            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            Ship.Dispose();
            ShortPowerBox.Dispose();
        }
    }
}
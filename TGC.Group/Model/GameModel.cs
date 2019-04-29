using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System;
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
using TGC.Core.Text;
using System.Collections.Generic;

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
        private TgcMesh Ship { get; set; }

        // Track01 path
        private TgcMesh track01 { get; set; }

        //PowerBoxes
        private TGCBox ShortPowerBox { get; set; }

        //GodBox
        private TGCBox godBox { get; set; }

        private TgcMesh ship_soft { get; set; }
        /*
         * Model Track01
         */
        private TgcMesh pCube1 { get; set; }
        /*
         * Earth
         */
        private TgcMesh pSphere1 { get; set; }
        /*
         * Sun
         */
        private TgcMesh pSphere3 { get; set; }
        /*
         * Mars
         */
        private TgcMesh pSphere2 { get; set; }
        /*
         * Sky
         */
        private TgcMesh pSphere4 { get; set; }

        /*
         * Camara en tercera persona
         */
        private TgcThirdPersonCamera camaraInterna;

        /*
        * Camara god
        */
        //private TgcRotationalCamera godCamera;
        private TgcThirdPersonCamera godCamera;

        // Variables varias
        private const float MOVEMENT_SPEED = 112f;
        private const float VERTEX_MIN_DISTANCE = 0.3f;

        TGCVector3 target;
        TGCVector3 previousTarget;
        private List<TGCVector3> vertex_pool;
        private List<TGCVector3> permanent_pool;

        float sum_elapsed = 0f;
        int counter_elapsed = 0;
        float medium_elapsed = 0f;
        private float angle = 0;

        private TGCBox test_hit { get; set; }

        TGCVector3 currentMeshRotVec;

        /* 
         * Variables test
         */

        private TgcText2D currentCamera;

        // Boundingbox test
        private bool BoundingBox { get; set; }

        // SkyBox test
        private TgcSkyBox skyBox;

        public override void Init()
        {
            vertex_pool = new List<TGCVector3>();
            permanent_pool = new List<TGCVector3>();

            // Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            // Textura de la carperta Media.
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;

            // Textura para pruebas
            var texture = TgcTexture.createTexture(pathTexturaCaja);
            var size = new TGCVector3(5, 5, 10);

            test_hit = TGCBox.fromSize(new TGCVector3(1, 1, 1), texture);
            test_hit.Position = new TGCVector3(0, 0, 0);
            test_hit.Transform = TGCMatrix.Identity;

            // Caja de prueba para poderes
            var sizePowerShort = new TGCVector3(10, 2, 10);
            ShortPowerBox = TGCBox.fromSize(sizePowerShort, texture);
            ShortPowerBox.Position = new TGCVector3(0, 0, 0);
            ShortPowerBox.Transform = TGCMatrix.Identity;

            // Caja god
            var sizeGodBox = new TGCVector3(10, 2, 10);
            godBox = TGCBox.fromSize(sizePowerShort, texture);
            godBox.Position = new TGCVector3(0, 0, 0);
            godBox.Transform = TGCMatrix.Identity;

            // Loader para los mesh
            var loader = new TgcSceneLoader();

            // Track path
            track01 = loader.loadSceneFromFile(MediaDir + "Test\\ship_path-TgcScene.xml").Meshes[0];
            track01.Move(0, 5, 0);

            // Nave
            Ship = loader.loadSceneFromFile(MediaDir + "Test\\ship_soft-TgcScene.xml").Meshes[0];
            Ship.Move(0, 5, 0);

            // Track model
            pCube1 = loader.loadSceneFromFile(MediaDir + "Test\\pCube1-TgcScene.xml").Meshes[0];
            pCube1.Move(0, 0, 0);

            // Tierra
            pSphere1 = loader.loadSceneFromFile(MediaDir + "Test\\pSphere1-TgcScene.xml").Meshes[0];
            pSphere1.Move(0, 0, 0);

            // Sol
            pSphere3 = loader.loadSceneFromFile(MediaDir + "Test\\pSphere3-TgcScene.xml").Meshes[0];
            pSphere3.Move(0, 0, 0);

            // Marte
            pSphere2 = loader.loadSceneFromFile(MediaDir + "Test\\pSphere2-TgcScene.xml").Meshes[0];
            pSphere2.Move(0, 0, 0);

            // Sky
            pSphere4 = loader.loadSceneFromFile(MediaDir + "Test\\pSphere4-TgcScene.xml").Meshes[0];
            pSphere4.Move(0, 0, 0);

            // Cámara en tercera persona
            camaraInterna = new TgcThirdPersonCamera(Ship.Position, 60, -135);

            // GOD Camera
            //godCamera = new TgcRotationalCamera(godBox.Position, godBox.Size.Length() * 6, Input);
            godCamera = new TgcThirdPersonCamera(godBox.Position, 60, -135);
            Camara = camaraInterna;

            target = Ship.Position;
            addVertexCollection(track01.getVertexPositions(), new TGCVector3(0, 5, 0));

            target = findNextTarget(vertex_pool);
            

            /*
             *Skybox TEST
             */

             
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
            
        }

        public override void Update()
        {
            PreUpdate();

            float rotate = 0;

            counter_elapsed++;
            sum_elapsed += ElapsedTime;
            medium_elapsed = sum_elapsed / counter_elapsed;
            if (counter_elapsed >= float.MaxValue / 2 | sum_elapsed >= float.MaxValue)
            {
                sum_elapsed = 0f;
                counter_elapsed = 0;
            }

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

            // Activar bounding box
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            // Cambiar cámara
            if (Input.keyPressed(Key.Tab))
            {
                if (Camara.Equals(camaraInterna))
                {
                    Camara = godCamera;
                }
                else
                {
                    Camara = camaraInterna;
                };
            }

            // Movimiento de la godCamera
            var movementGod = TGCVector3.Empty;
            float moveForward = 0;

            if (Camara.Equals(godCamera))
            {
                if (Input.keyDown(Key.Up) || Input.keyDown(Key.W))
                {
                    moveForward = 1;
                }
                else if (Input.keyDown(Key.Down) || Input.keyDown(Key.S))
                {
                    moveForward = -1;
                }
                else if (Input.keyDown(Key.Space))
                {
                    movementGod.Y = 1 * ElapsedTime * MOVEMENT_SPEED;
                }
                else if (Input.keyDown(Key.C))
                {
                    movementGod.Y = -1 * ElapsedTime * MOVEMENT_SPEED;
                }
                else if (Input.keyDown(Key.D) || Input.keyPressed(Key.Right))
                {
                    rotate = 2 * ElapsedTime;

                }
                else if (Input.keyDown(Key.A) || Input.keyPressed(Key.Left))
                {
                    rotate = -2 * ElapsedTime;
                }

            }
            godBox.Move(movementGod);

            // Rotación de la GodCamera
            godBox.Rotation += new TGCVector3(0, rotate, 0);
            godCamera.rotateY(rotate);
            float moveF = moveForward * ElapsedTime * MOVEMENT_SPEED;
            movementGod.Z = (float)Math.Cos(godBox.Rotation.Y) * moveF;
            movementGod.X = (float)Math.Sin(godBox.Rotation.Y) * moveF;
            

            // Movimiento de la nave (Ship)
            var movement = TGCVector3.Empty;
            var originalPos = Ship.Position;
            
            movement = target;
            movement.Subtract(originalPos);

            if (movement.Length() < (MOVEMENT_SPEED * ElapsedTime))
            {
                movement = target;
                movement.Subtract(originalPos);
            }
            else
            {
                movement.Normalize();
                movement.Multiply(MOVEMENT_SPEED * ElapsedTime);

            }
            //test_hit.Position = getPositionAtMiliseconds(1000);
                        
            float angleXZ = 0;
            if (movement.Length() < (MOVEMENT_SPEED * ElapsedTime) / 2)
            {
                previousTarget = target;
                target = findNextTarget(vertex_pool);

                // XZ
                //TGCVector3 targetXZ = new TGCVector3(target.X, target.Y, 0);
                //TGCVector3 previousTargetXZ = new TGCVector3(previousTarget.X, previousTarget.Z, 0);
                //angleXZ = FastMath.Acos(TGCVector3.Dot(TGCVector3.Normalize(targetXZ), TGCVector3.Normalize(previousTargetXZ)));
                angleXZ = FastMath.Atan2(target.X, target.Z) - FastMath.Atan2(previousTarget.X, previousTarget.Z);

            }
            Ship.Move(movement);
            Ship.RotateY(angleXZ);
            //camaraInterna.rotateY(angleXZ * ElapsedTime);

            /* // Screen size 
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            */

            // Texto de currentCamera
            currentCamera = new TgcText2D();
            currentCamera.Text = "CAMBIAR CÁMARA CON TAB";
            currentCamera.Align = TgcText2D.TextAlign.CENTER;
            currentCamera.Color = Color.Yellow;
            currentCamera.changeFont(new Font(FontFamily.GenericMonospace, 14, FontStyle.Italic));

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Especificaciones en pantalla: posición de la nave y de la cámara
            DrawText.drawText("Ship Position: \n" + Ship.Position, 5, 20, Color.Yellow);
            DrawText.drawText("Power Position: \n " + ShortPowerBox.Position, 5, 90, Color.Yellow);
            DrawText.drawText("Camera Position: \n" + Camara.Position, 5, 160, Color.Yellow);
            DrawText.drawText("Medium Elapsed: \n" + medium_elapsed, 145, 20, Color.Yellow);
            DrawText.drawText("Elapsed: \n" + ElapsedTime, 145, 60, Color.Yellow);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;      // Horizontal Alignment
            stringFormat.LineAlignment = StringAlignment.Center;  // Vertical Alignment


            // Transformaciones
            Ship.Transform = TGCMatrix.Scaling(Ship.Scale) * TGCMatrix.RotationYawPitchRoll(Ship.Rotation.Y, Ship.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(Ship.Position);
            godBox.Transform = TGCMatrix.Scaling(godBox.Scale) * TGCMatrix.RotationYawPitchRoll(godBox.Rotation.Y, godBox.Rotation.X, godBox.Rotation.Z) * TGCMatrix.Translation(godBox.Position);
            ShortPowerBox.Transform = TGCMatrix.Scaling(ShortPowerBox.Scale) * TGCMatrix.RotationYawPitchRoll(ShortPowerBox.Rotation.Y, ShortPowerBox.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(ShortPowerBox.Position);
            test_hit.Transform = TGCMatrix.Scaling(test_hit.Scale) * TGCMatrix.RotationYawPitchRoll(test_hit.Rotation.Y, test_hit.Rotation.X, test_hit.Rotation.Z) * TGCMatrix.Translation(test_hit.Position);


            // Renders
            skyBox.Render();
            Ship.Render();
            track01.Render();
            pCube1.Render();
            pSphere1.Render();
            pSphere3.Render();
            pSphere2.Render(); 
            godBox.Render();
            currentCamera.render();
            //pSphere4.Render();
            //ShortPowerBox.Render();
            test_hit.Render();
            if (BoundingBox)
            {
                Ship.BoundingBox.Render();
                ShortPowerBox.BoundingBox.Render();
                godBox.BoundingBox.Render();
            }

            // Posición de cámaras
            camaraInterna.Target = Ship.Position;
            godCamera.Target = godBox.Position;

            PostRender();
        }

        public void addVertexCollection(TGCVector3[] vertex_collection, TGCVector3 offset)
        {
            for (int i = 0; i < vertex_collection.Length; i++)
            {
                Boolean present = false;
                TGCVector3 v = vertex_collection[i];
                v.Add(offset);

                for (int j = 0; j < vertex_pool.Count; j++)
                {
                    TGCVector3 comparator = vertex_pool[j];
                    if (comparator.X == v.X && comparator.Y == v.Y && comparator.Z == v.Z)
                    {
                        present = true;
                    }
                }

                if (!present)
                {
                    vertex_pool.Add(v);
                    permanent_pool.Add(v);
                }
            }
        }
       
        public TGCVector3 findNextTarget(List<TGCVector3> pool)
        {
            TGCVector3 current_target = target;
            pool.Remove(target);

            float distance = -1f;
            TGCVector3 new_target = new TGCVector3();

            if (pool.Count > 0)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    TGCVector3 this_vertex = pool[i];
                    this_vertex.Subtract(current_target);
                    float this_distance = this_vertex.Length();

                    if (distance < 0)
                    {
                        distance = this_distance;
                        new_target = pool[i];
                    }
                    else if (this_distance < distance)
                    {
                        distance = this_distance;
                        new_target = pool[i];
                    }
                }
            }
            else
            {
                new_target = current_target;
                addVertexCollection(track01.getVertexPositions(), new TGCVector3(0, 0, 0));
            }
            target = new_target;

            return target;
        }

        public TGCVector3 getPositionAtMiliseconds(float miliseconds)
        {
            if (medium_elapsed == 0)
            {
                return TGCVector3.Empty;
            }

            List<TGCVector3> mypool = new List<TGCVector3>(permanent_pool);
            TGCVector3 originalTarget = target;
            TGCVector3 simulated_ship_position = TGCVector3.Empty;


            target = originalTarget;
            return simulated_ship_position;
        }

        public override void Dispose()
        {
            Ship.Dispose();
            ShortPowerBox.Dispose();
        }
    }
}
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
using TGC.Core.Sound;
using TGC.Group.Stats;
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

        // Lista de paths
        private List<TgcMesh> paths;

        // PowerBoxes
        private List<TGCBox> power_boxes;
        private List<Boolean> power_boxes_states;

        // GodBox (box para la camara god)
        private TGCBox godBox { get; set; }

        // SuperPowerBox
        private TGCBox superPowerBox { get; set; }
        private bool superPowerStatus;

        // Lista de tracks
        private List<TgcMesh> tracks;
        
        // Mesh: tierra
        private TgcMesh mTierra { get; set; }
        // Mesh: sol
        private TgcMesh mSol { get; set; }
        // Mesh: marte
        private TgcMesh mMarte { get; set; }

        // Camara principal (tercera persona)
        private TgcThirdPersonCamera camaraInterna;

        // Cámara god (tercera persona)
        private TgcThirdPersonCamera godCamera;

        // Musica
        private TgcMp3Player mp3Player;
        String path;

        // Movimiento y rotacion
        private const float MOVEMENT_SPEED = 12f;
        private const float VERTEX_MIN_DISTANCE = 0.3f;

        TGCVector3 shipTarget;
        TGCVector3 previousTarget;
        private List<TGCVector3> vertex_pool;
        private List<TGCVector3> permanent_pool;

        float sum_elapsed = 0f;
        int counter_elapsed = 0;
        float medium_elapsed = 0f;

        private TGCBox test_hit { get; set; }

        TGCVector3 currentRot;
        TGCVector3 originalRot;

        // Textos 2D
        private TgcText2D currentCamera;
        private TgcText2D playerStats;

        // Boundingbox status
        private bool BoundingBox { get; set; }

        // SkyBox
        private TgcSkyBox skyBox;

        // Player creation
        Stat stat = new Stat("PLAYER");

        public override void Init()
        {
            // Listas de tracks, paths y powerBoxes
            tracks = new List<TgcMesh>();
            paths = new List<TgcMesh>();
            power_boxes = new List<TGCBox>();
            power_boxes_states = new List<Boolean>();

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
            
            // Caja god para la camara god
            var sizeGodBox = new TGCVector3(10, 2, 10);
            godBox = TGCBox.fromSize(sizeGodBox, texture);
            godBox.Position = new TGCVector3(0, 0, 0);
            godBox.Transform = TGCMatrix.Identity;

            // Loader para los mesh
            var loader = new TgcSceneLoader();

            // Nave
            Ship = loader.loadSceneFromFile(MediaDir + "Test\\ship_soft-TgcScene.xml").Meshes[0];
            Ship.Move(0, 25, 0);
            originalRot = new TGCVector3(0, 0, 1);

            // Tierra
            mTierra = loader.loadSceneFromFile(MediaDir + "Test\\pSphere1-TgcScene.xml").Meshes[0];
            mTierra.Move(0, 0, 0);

            // Sol
            mSol = loader.loadSceneFromFile(MediaDir + "Test\\pSphere3-TgcScene.xml").Meshes[0];
            mSol.Move(0, 0, 0);

            // Marte
            mMarte = loader.loadSceneFromFile(MediaDir + "Test\\pSphere2-TgcScene.xml").Meshes[0];
            mMarte.Move(0, 0, 0);

            // Camara a la ship
            camaraInterna = new TgcThirdPersonCamera(Ship.Position + new TGCVector3(0, 0, 15), 30, -55);

            // Camara god
            godCamera = new TgcThirdPersonCamera(godBox.Position, 60, -135);

            // Seteo de camara principal
            Camara = camaraInterna;
            
            // Asignar target inicial a la ship
            shipTarget = Ship.Position;
            
            // Init de tracks
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\track01-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\path01-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\track01-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\path01-TgcScene.xml").Meshes[0]);

            // Asignar proximo target de la nave
            shipTarget = findNextTarget(vertex_pool);

            // Init de poderes
            addPowerBox();
            addPowerBox();
            addPowerBox();

            // SuperPower
            var sizeSuperPower = new TGCVector3(25, 2, 2);
            superPowerBox = TGCBox.fromSize(sizeSuperPower, texture);
            superPowerBox.Color = Color.White;
            superPowerBox.updateValues();
            superPowerBox.Position = new TGCVector3(0, 0, 0);
            superPowerBox.Transform = TGCMatrix.Identity;
            superPowerStatus = false;
               
            // SkyBox
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

            // Musica
            path = MediaDir + "Music\\hz-circle.mp3";
            mp3Player = new TgcMp3Player();
            mp3Player.FileName = path;
            mp3Player.play(true);
            mp3Player.pause();


        }

        public override void Update()
        {
            PreUpdate();

            // Apretar M para activar musica
           if (Input.keyPressed(Key.M))
            {
                if (mp3Player.getStatus() == TgcMp3Player.States.Playing)
                {
                    //Pausar el MP3
                    mp3Player.pause();
                } else if(mp3Player.getStatus() == TgcMp3Player.States.Paused)
                {
                    //Resumir la ejecución del MP3
                    mp3Player.resume();
                }
            }


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

            // Deteccion entre poderes y ship al apretar ESPACIO
            if (Input.keyPressed(Key.Space))
            {
                int noTouching = 0;
                for (int a = 0; a < power_boxes.Count; a++)
                {
                    TGCBox ShortPowerBox = power_boxes[a];
                    if (TgcCollisionUtils.testAABBAABB(Ship.BoundingBox, power_boxes[a].BoundingBox))
                    {
                        //power_boxes[a].Color = Color.Yellow;
                        //power_boxes[a].updateValues();
                        power_boxes[a].Color = Color.Red;
                        power_boxes[a].updateValues();
                        if (!power_boxes_states[a])
                        {
                            power_boxes_states[a] = true;
                            stat.addMultiply();
                            stat.addPoints(10);
                        }
                    }
                    else
                    {
                        noTouching++;
                        power_boxes[a].Color = Color.White;
                        power_boxes[a].updateValues();
                    }
                }
                if (noTouching == power_boxes.Count)
                {

                    stat.cancelMultiply();
                    stat.addPoints(-10);
                }
            }

            // Activar bounding box al presionar F
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            // Cambiar cámara al presionar TAB
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

            // SuperPower al presionar SHIFT DERECHO
            if (Input.keyPressed(Key.RightShift))
            {
                superPowerStatus = true;
                superPowerBox.Position = Ship.Position;
            }

            if (superPowerStatus)
            {
                var superPowerMovement = TGCVector3.Empty;
                var superPowerOriginalPos = Ship.Position;

                superPowerMovement = shipTarget;
                superPowerMovement.Subtract(superPowerOriginalPos);

                if (superPowerMovement.Length() < (MOVEMENT_SPEED * 3 * ElapsedTime))
                {
                    superPowerMovement = shipTarget;
                    superPowerMovement.Subtract(superPowerOriginalPos);
                }else
                {
                    superPowerMovement.Normalize();
                    superPowerMovement.Multiply(MOVEMENT_SPEED * 3 * ElapsedTime);

                }
                //test_hit.Position = getPositionAtMiliseconds(1000);
                if (superPowerMovement.Length() < (MOVEMENT_SPEED * ElapsedTime) / 2)
                {
                    shipTarget = findNextTarget(vertex_pool);
                }
                superPowerBox.Move(superPowerMovement);
            }

            // Movimiento de la godCamera con W A S D ESPACIO C
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
            
            // Rotación de la GodCamera
            godBox.Rotation += new TGCVector3(0, rotate, 0);
            godCamera.rotateY(rotate);
            float moveF = moveForward * ElapsedTime * MOVEMENT_SPEED;
            movementGod.Z = (float)Math.Cos(godBox.Rotation.Y) * moveF;
            movementGod.X = (float)Math.Sin(godBox.Rotation.Y) * moveF;

            godBox.Move(movementGod);

            // Movimiento de la nave (Ship)
            var shipMovement = TGCVector3.Empty;
            var shipOriginalPos = Ship.Position;

            shipMovement = shipTarget;
            shipMovement.Subtract(shipOriginalPos);

            if (shipMovement.Length() < (MOVEMENT_SPEED * ElapsedTime))
            {
                shipMovement = shipTarget;
                shipMovement.Subtract(shipOriginalPos);
            }
            else
            {
                shipMovement.Normalize();
                shipMovement.Multiply(MOVEMENT_SPEED * ElapsedTime);

            }
            //test_hit.Position = getPositionAtMiliseconds(1000);
            if (shipMovement.Length() < (MOVEMENT_SPEED * ElapsedTime) / 2)
            {
                shipTarget = findNextTarget(vertex_pool);
            }

            // Rotacion de la camara junto a la Ship por el camino
            float angleXZ = 0;
            float angleYZ = 0;

            TGCVector3 vectorA = new TGCVector3(shipTarget.X, 0, shipTarget.Z) - new TGCVector3(Ship.Position.X, 0, Ship.Position.Z);
            TGCVector3 vectorB = new TGCVector3(0, shipTarget.Y, shipTarget.Z) - new TGCVector3(0, Ship.Position.Y, Ship.Position.Z);

            TGCVector3 directionXZ = TGCVector3.Empty;
            TGCVector3 directionYZ = TGCVector3.Empty;

            if(vectorA.Length() != 0)
            {
                directionXZ = TGCVector3.Normalize(vectorA);
            }
            if(vectorB.Length() != 0)
            {
                directionYZ = TGCVector3.Normalize(vectorB);
            }
            
            angleXZ = -FastMath.Acos(TGCVector3.Dot(originalRot, directionXZ));
            angleYZ = -FastMath.Acos(TGCVector3.Dot(originalRot, directionYZ));
            if (directionXZ.X > 0)
            {
                angleXZ *= -1;
            }
            if (directionYZ.Y < 0)
            {
                angleYZ *= -1;
            }
            if(!float.IsNaN(angleXZ) && !float.IsNaN(angleYZ))
            {
                float orRotY = Ship.Rotation.Y;
                float angIntY = orRotY * (1.0f - 0.1f) + angleXZ * 0.1f;

                float orRotX = Ship.Rotation.X;
                float angIntX = orRotX * (1.0f - 0.1f) + angleYZ * 0.1f;

                Ship.Rotation = new TGCVector3(angIntX, angIntY, 0);

                float originalRotationY = camaraInterna.RotationY;
                float anguloIntermedio = originalRotationY * (1.0f - 0.03f) + angleXZ * 0.03f;
                //camaraInterna.RotationY = anguloIntermedio ;
                currentRot = directionXZ + directionYZ;
            }
            
            // Test console de angulos
            System.Diagnostics.Debug.WriteLine("DANIEEEEEEEEEEEEEL");
            System.Diagnostics.Debug.WriteLine(angleXZ + "," + angleYZ);
            System.Diagnostics.Debug.WriteLine(TGCVector3.Dot(originalRot, directionXZ) + "," + TGCVector3.Dot(originalRot, directionYZ));
            System.Diagnostics.Debug.WriteLine(originalRot);
            System.Diagnostics.Debug.WriteLine(directionXZ + "," + directionYZ);

            Ship.Move(shipMovement);

            // Texto informativo de botones
            currentCamera = new TgcText2D();
            currentCamera.Text = "CÁMARA: TAB / MUSICA: M / NOTAS: ESPACIO / SUPERPODER: RIGHT SHIFT";
            currentCamera.Align = TgcText2D.TextAlign.CENTER;
            currentCamera.Color = Color.Yellow;
            currentCamera.changeFont(new Font(FontFamily.GenericMonospace, 14, FontStyle.Italic));

            // Texto informativo del player
            playerStats = new TgcText2D();
            playerStats.Text = "PUNTAJE ACTUAL: " + stat.totalPoints + " MULTIPLICADOR ACTUAL: " + 
                stat.totalMultiply + " MULTIPLICADOR PARCIAL: " + stat.partialMultiply;
            playerStats.Size = new Size(280, 200);
            playerStats.Align = TgcText2D.TextAlign.LEFT;
            playerStats.Position = new Point(10, 320);
            playerStats.Color = Color.Yellow;
            playerStats.changeFont(new Font(FontFamily.GenericMonospace, 14, FontStyle.Italic));

            // Transformaciones
            Ship.Transform = TGCMatrix.Scaling(Ship.Scale) * TGCMatrix.RotationYawPitchRoll(Ship.Rotation.Y, Ship.Rotation.X, Ship.Rotation.Z) * TGCMatrix.Translation(Ship.Position);
            godBox.Transform = TGCMatrix.Scaling(godBox.Scale) * TGCMatrix.RotationYawPitchRoll(godBox.Rotation.Y, godBox.Rotation.X, godBox.Rotation.Z) * TGCMatrix.Translation(godBox.Position);
            superPowerBox.Transform = TGCMatrix.Scaling(superPowerBox.Scale) * TGCMatrix.RotationYawPitchRoll(superPowerBox.Rotation.Y, superPowerBox.Rotation.X, superPowerBox.Rotation.Z) * TGCMatrix.Translation(superPowerBox.Position);

            for (int a = 0; a < power_boxes.Count; a++)
            {
                TGCBox ShortPowerBox = power_boxes[a];
                ShortPowerBox.Transform = TGCMatrix.Scaling(ShortPowerBox.Scale) * TGCMatrix.RotationYawPitchRoll(ShortPowerBox.Rotation.Y, ShortPowerBox.Rotation.X, ShortPowerBox.Rotation.Z) * TGCMatrix.Translation(ShortPowerBox.Position);
            }
            test_hit.Transform = TGCMatrix.Scaling(test_hit.Scale) * TGCMatrix.RotationYawPitchRoll(test_hit.Rotation.Y, test_hit.Rotation.X, test_hit.Rotation.Z) * TGCMatrix.Translation(test_hit.Position);


            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            // Especificaciones en pantalla: posición de la nave y de la cámara
            DrawText.drawText("Ship Position: \n" + Ship.Position, 5, 20, Color.Yellow);
            DrawText.drawText("Camera Position: \n" + Camara.Position, 5, 160, Color.Yellow);
            DrawText.drawText("Medium Elapsed: \n" + medium_elapsed, 145, 20, Color.Yellow);
            DrawText.drawText("Elapsed: \n" + ElapsedTime, 145, 60, Color.Yellow);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;      // Horizontal Alignment
            stringFormat.LineAlignment = StringAlignment.Center;  // Vertical Alignment

            // Renders
            skyBox.Render();
            Ship.Render();
            for(int a=0; a<tracks.Count; a++)
            {
                tracks[a].Render();
            }
            mTierra.Render();
            mSol.Render();
            mMarte.Render(); 
            currentCamera.render();
            playerStats.render();
            for (int a = 0; a < power_boxes.Count; a++)
            {
                TGCBox ShortPowerBox = power_boxes[a];
                ShortPowerBox.Render();
            }
            if (BoundingBox)
            {
                Ship.BoundingBox.Render();

                for (int a = 0; a < power_boxes.Count; a++)
                {
                    TGCBox ShortPowerBox = power_boxes[a];
                    ShortPowerBox.BoundingBox.Render();
                }
                godBox.BoundingBox.Render();
            }
            superPowerBox.Render();

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
            TGCVector3 current_target = shipTarget;
            pool.Remove(shipTarget);

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
            }
            shipTarget = new_target;

            return shipTarget;
        }

        public TGCVector3 getPositionAtMiliseconds(float miliseconds)
        {
            if (medium_elapsed == 0)
            {
                return TGCVector3.Empty;
            }

            List<TGCVector3> mypool = new List<TGCVector3>(permanent_pool);
            TGCVector3 originalTarget = shipTarget;
            TGCVector3 simulated_ship_position = TGCVector3.Empty;


            shipTarget = originalTarget;
            return simulated_ship_position;
        }

        private void concatTrack(TgcMesh track, TgcMesh path)
        {
            TGCVector3 lastVertex = TGCVector3.Empty;

            if(tracks.Count != 0)
            {
                lastVertex = vertex_pool[0];
                for (int a = 0; a < vertex_pool.Count; a++)
                {
                    TGCVector3 thisvertex = vertex_pool[a];
                    if (thisvertex.Z >= lastVertex.Z)
                    {
                        lastVertex = thisvertex;
                    }
                }
            }            

            track.Move(lastVertex);
            tracks.Add(track);

            path.Move(lastVertex);
            paths.Add(path);

            addVertexCollection(path.getVertexPositions(), lastVertex);
        }

        private void addPowerBox()
        {
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;
            var texture = TgcTexture.createTexture(pathTexturaCaja);

            var sizePowerBox = new TGCVector3(1, 15, 1);
            TGCBox powerbox = TGCBox.fromSize(sizePowerBox, texture);
            powerbox.Position = vertex_pool[(new Random(Guid.NewGuid().GetHashCode())).Next(vertex_pool.Count)];
            powerbox.Transform = TGCMatrix.Identity;

            power_boxes.Add(powerbox);
            power_boxes_states.Add(false);
        }

        public override void Dispose()
        {
            Ship.Dispose();
            mp3Player.closeFile();
            for (int a = 0; a < power_boxes.Count; a++)
            {
                power_boxes[a].Dispose();
            }
        }
    }
}
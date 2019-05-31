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
using TGC.Group.Core2d;
using System.Collections.Generic;
using TGC.Group.VertexMovement;
using System.Windows.Forms;

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
        VertexMovementManager shipManager;

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
        private float superPowerTime;
        private const float SUPERPOWER_MOVEMENT_SPEED = 152f;

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
        String mp3Path;

        // Movimiento y rotacion
        private const float MOVEMENT_SPEED = 52f;
        private const float VERTEX_MIN_DISTANCE = 0.3f;

        float sum_elapsed = 0f;
        int counter_elapsed = 0;
        float medium_elapsed = 0f;

        private TGCBox test_hit { get; set; }

        TGCVector3 currentRot;
        TGCVector3 originalRot;

        // Textos 2D
        private TgcText2D tButtons;
        private TgcText2D multiplyPointsGUI;
        private TgcText2D totalPoints;
        private TgcText2D subPoints;

        // Boundingbox status
        private bool BoundingBox { get; set; }

        // SkyBox
        private TgcSkyBox skyBox;

        // Player creation
        Stat stat = new Stat("PLAYER");

        // GUI
        private bool helpGUI;
        private bool developerModeGUI;
        private float totalPointsGUItime;
        private float multiplyPointsGUItime;
        private bool penalty;
        private float penaltyTime;

        private Drawer2D drawer2D;
        private CustomSprite superPowerSprite;
        private CustomSprite songProgressBarSprite;

        public override void Init()
        {
            // GUI
            //Crear Sprite
            drawer2D = new Drawer2D();



            superPowerSprite = new CustomSprite();
            superPowerSprite.Bitmap = new CustomBitmap(MediaDir + "\\GUI\\superPowerBar.png", D3DDevice.Instance.Device);
            superPowerSprite.Scaling = new TGCVector2(0.5f, 0.5f);
            var textureSize = superPowerSprite.Bitmap.Size;
            superPowerSprite.Position = new TGCVector2(50, Screen.PrimaryScreen.Bounds.Bottom - textureSize.Height * 0.7f);

            songProgressBarSprite = new CustomSprite();
            songProgressBarSprite.Bitmap = new CustomBitmap(MediaDir + "\\GUI\\songProgressBar.png", D3DDevice.Instance.Device);
            songProgressBarSprite.Scaling = new TGCVector2(0.72f, 0.5f);
            songProgressBarSprite.Position = new TGCVector2(30, 30);

            helpGUI = true;
            developerModeGUI = false;

            penalty = false;

            tButtons = new TgcText2D();
            tButtons.Align = TgcText2D.TextAlign.RIGHT;
            tButtons.Color = Color.White;
            tButtons.Size = new Size(200, 240);
            tButtons.Position = new Point(Screen.PrimaryScreen.Bounds.Right - tButtons.Size.Width - 30, Screen.PrimaryScreen.Bounds.Bottom - tButtons.Size.Height);
            tButtons.changeFont(new Font("BigNoodleTitling", 16, FontStyle.Italic));

            totalPoints = new TgcText2D();
            totalPoints.Align = TgcText2D.TextAlign.LEFT;
            totalPoints.Size = new Size(200, 300);
            totalPoints.Position = new Point(100, Screen.PrimaryScreen.Bounds.Bottom - totalPoints.Size.Height - 20);
            totalPoints.Color = Color.White;
            totalPoints.changeFont(new Font("BigNoodleTitling", 180, FontStyle.Regular));

            multiplyPointsGUI = new TgcText2D();
            multiplyPointsGUI.Align = TgcText2D.TextAlign.LEFT;
            multiplyPointsGUI.Size = new Size(150, 150);
            multiplyPointsGUI.Position = new Point(110, Screen.PrimaryScreen.Bounds.Bottom - multiplyPointsGUI.Size.Height - 220);
            multiplyPointsGUI.Color = Color.White;
            multiplyPointsGUI.changeFont(new Font("BigNoodleTitling", 59, FontStyle.Italic));

            subPoints = new TgcText2D();
            subPoints.Text = "-10";
            subPoints.Align = TgcText2D.TextAlign.CENTER;
            subPoints.Position = new Point(totalPoints.Position.X + 80, 40);
            subPoints.Color = Color.Red;
            subPoints.changeFont(new Font("BigNoodleTitling", 40, FontStyle.Italic));

            // Listas de tracks, paths y powerBoxes
            tracks = new List<TgcMesh>();
            paths = new List<TgcMesh>();
            power_boxes = new List<TGCBox>();
            power_boxes_states = new List<Boolean>();

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
            Ship.Move(0, 0, 0);
            originalRot = new TGCVector3(0, 0, 1);
            currentRot = originalRot;

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
            camaraInterna = new TgcThirdPersonCamera(Ship.Position, 20, -55);

            // Camara god
            godCamera = new TgcThirdPersonCamera(godBox.Position, 60, -135);

            // Seteo de camara principal
            Camara = camaraInterna;
            
            // Asignar target inicial a la ship
            shipManager = new VertexMovementManager(Ship.Position, Ship.Position, MOVEMENT_SPEED);

            // Init de tracks
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track01-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path01-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track02-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path02-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track03-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path03-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track01-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path01-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track02-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path02-TgcScene.xml").Meshes[0]);
            concatTrack(loader.loadSceneFromFile(MediaDir + "Test\\new_track03-TgcScene.xml").Meshes[0],
                loader.loadSceneFromFile(MediaDir + "Test\\new_path03-TgcScene.xml").Meshes[0]);

            // Asignar proximo target de la nave
            shipManager.init();

            // Init de poderes
            addPowerBox();
            addPowerBox();
            addPowerBox();
            addPowerBox();

            // SuperPower
            var sizeSuperPower = new TGCVector3(225, 1, 1);
            superPowerBox = TGCBox.fromSize(sizeSuperPower, texture);
            superPowerBox.Color = Color.MediumPurple;
            superPowerBox.updateValues();
            superPowerBox.Position = new TGCVector3(0, 0, -50);
            superPowerBox.Transform = TGCMatrix.Identity;
            superPowerStatus = false;
            superPowerTime = 0;
               
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
            mp3Path = MediaDir + "Music\\BattlefieldMagicSword.wav";
            mp3Player = new TgcMp3Player();
            mp3Player.FileName = mp3Path;
            mp3Player.play(true);
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
                } else if (mp3Player.getStatus() == TgcMp3Player.States.Paused)
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

                            totalPointsGUItime = sum_elapsed;
                            multiplyPointsGUItime = sum_elapsed;
                        }
                    }
                    else
                    {
                        noTouching++;
                        power_boxes[a].updateValues();
                    }
                }
                if (noTouching == power_boxes.Count)
                {

                    stat.cancelMultiply();
                    if (stat.totalPoints != 0)
                    {
                        penalty = true;
                    }
                    stat.addPoints(-10);

                    totalPointsGUItime = sum_elapsed;
                    penaltyTime = sum_elapsed;
                    multiplyPointsGUItime = sum_elapsed;
                }
            }

            if (penalty)
            {
                if (sum_elapsed - penaltyTime > 0.5f)
                {
                    penalty = false;
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

            // SuperPower al presionar SHIFT IZQUIERDO cada 2 segundos
            if (Input.keyPressed(Key.LeftShift))
            {
                if (sum_elapsed - superPowerTime > 3.0f || superPowerTime == 0)
                {
                    stat.duplicatePoints();
                    superPowerStatus = true;
                    superPowerBox.Position = Ship.Position;
                    superPowerTime = sum_elapsed;


                }
            }

            if (Input.keyPressed(Key.H))
            {
                helpGUI = !helpGUI;
            }

            if (Input.keyPressed(Key.K))
            {
                developerModeGUI = !developerModeGUI;
            }
            if (superPowerStatus)
            {

                var superPowerMovement = TGCVector3.Empty;
                var superPowerOriginalPos = Ship.Position;

                //superPowerMovement = shipTarget;
                superPowerMovement.Subtract(superPowerOriginalPos);

                if (superPowerMovement.Length() < (SUPERPOWER_MOVEMENT_SPEED * ElapsedTime))
                {
                    //   superPowerMovement = shipTarget;
                    superPowerMovement.Subtract(superPowerOriginalPos);
                } else
                {
                    superPowerMovement.Normalize();
                    superPowerMovement.Multiply(SUPERPOWER_MOVEMENT_SPEED * ElapsedTime);

                }
                //test_hit.Position = getPositionAtMiliseconds(1000);
                if (superPowerMovement.Length() < (SUPERPOWER_MOVEMENT_SPEED * ElapsedTime) / 2)
                {
                    //   shipTarget = findNextTarget(vertex_pool);
                }
                superPowerBox.Move(superPowerMovement);


                if (sum_elapsed - superPowerTime > 2.7f)
                {
                    superPowerSprite.Scaling = new TGCVector2(0.5f, 0.5f);
                    superPowerStatus = false;
                }
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
            TGCVector3 shipMovement = shipManager.update(ElapsedTime, Ship.Position);
            Ship.Move(shipMovement);

            // Rotacion de la camara junto a la Ship por el camino
            float angleXZ = 0;
            float angleYZ = 0;

            TGCVector3 vectorA = new TGCVector3(shipMovement.X, 0, shipMovement.Z);
            TGCVector3 vectorB = new TGCVector3(0, shipMovement.Y, shipMovement.Z);

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
            if (directionYZ.Y <0)
            {
                angleYZ *= -1;
            }
            if(!float.IsNaN(angleXZ) && !float.IsNaN(angleYZ))
            {
                float orRotY = Ship.Rotation.Y;
                float angIntY = orRotY * (1.0f - 0.1f) + angleXZ * 0.1f;

                float orRotX = Ship.Rotation.X;
                float angIntX = orRotX * (1.0f - 0.1f) + angleYZ * 0.1f;

                //Ship.Rotation = new TGCVector3(angIntX , angIntY , 0);

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

            // Texto informativo de botones
            tButtons.Text = "CÁMARA: TAB \nMUSICA: M \nNOTAS: ESPACIO \nSUPERPODER: LEFT SHIFT \nDEVMOD: K \nOcultar ayuda con H";

            // Texto informativo del player

            totalPoints.Text = stat.totalPoints.ToString();
            
            multiplyPointsGUI.Text = "x" + stat.totalMultiply;

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

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(superPowerSprite);
            drawer2D.DrawSprite(songProgressBarSprite);

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();


            // Especificaciones en pantalla: posición de la nave y de la cámara
            if (developerModeGUI)
            {
                DrawText.drawText("Ship Position: \n" + Ship.Position, 5, 20, Color.Yellow);
                DrawText.drawText("Medium Elapsed: \n" + medium_elapsed, 145, 20, Color.Yellow);
                DrawText.drawText("Camera Position: \n" + Camara.Position, 5, 100, Color.Yellow);
                DrawText.drawText("Elapsed: \n" + ElapsedTime, 145, 60, Color.Yellow);
                DrawText.drawText("PUNTAJE ACTUAL: " + stat.totalPoints +
                "\nMULTIPLICADOR ACTUAL: " + stat.totalMultiply +
                "\nMULTIPLICADOR PARCIAL: " + stat.partialMultiply +
                "\nSUPERPODER: " + ((!superPowerStatus) ? "TRUÉ" : "FALSE" + ElapsedTime), 5, 180, Color.Yellow);
            }
            DrawText.drawText("SumElapsed: \n" + sum_elapsed, 145, 100, Color.White);


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

            totalPoints.render();
            multiplyPointsGUI.render();
            if (helpGUI)
            {
                tButtons.render();
            }
            if (penalty)
            {
                subPoints.render();
            }
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
            if (superPowerStatus)
            {
                superPowerBox.Render();
            }

            // Posición de cámaras
            camaraInterna.Target = Ship.Position;
            godCamera.Target = godBox.Position;


            //drawer2D.BeginDrawSprite();
            //drawer2D.DrawSprite(sprite);
            //drawer2D.EndDrawSprite();


            PostRender();
        }

     /*   public TGCVector3 getPositionAtMiliseconds(float miliseconds)
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
        } */

        private void concatTrack(TgcMesh track, TgcMesh path)
        {
            TGCVector3 lastVertex = TGCVector3.Empty;

            if(tracks.Count != 0)
            {
                lastVertex = shipManager.vertex_pool[0];
                for (int a = 0; a < shipManager.vertex_pool.Count; a++)
                {
                    TGCVector3 thisvertex = shipManager.vertex_pool[a];
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

            shipManager.addVertexCollection(path.getVertexPositions(), lastVertex);
        }

        private void addPowerBox()
        {
            var pathTexturaCaja = MediaDir + "Test\\Textures\\white_wall.jpg";
            var texture = TgcTexture.createTexture(pathTexturaCaja);

            var sizePowerBox = new TGCVector3(8, 1, 8);
            TGCBox powerbox = TGCBox.fromSize(sizePowerBox, texture);
            powerbox.Color = Color.Pink;
            TGCVector3 randomPosition = shipManager.vertex_pool[(new Random(Guid.NewGuid().GetHashCode())).Next(shipManager.vertex_pool.Count)];
            randomPosition.Y = randomPosition.Y + 3;
            powerbox.Position = randomPosition;
            powerbox.Transform = TGCMatrix.Identity;
            powerbox.updateValues();
            

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
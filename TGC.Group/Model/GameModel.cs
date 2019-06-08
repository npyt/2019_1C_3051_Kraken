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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TGC.Group.StateMachine;

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

        public Drawer2D drawer2D;
        GameState gameState;
        MenuState menuState;

        State currentState;

        public Font defaultFont = new Font("Verdana", 10, FontStyle.Regular);

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

        public override void Init()
        {

            drawer2D = new Drawer2D();
            menuState = new MenuState(this);

            currentState = menuState;
            
        }

        public override void Update()
        {
            PreUpdate();

            if(currentState != null)
                currentState.update(ElapsedTime);

            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            if (currentState != null)
                currentState.render(ElapsedTime);

            PostRender();
        }

        public void selectLevel(string folder)
        {
            gameState = new GameState(this, folder);
            currentState = gameState;
        }

        public void returnToMenu()
        {
            menuState.gameTime.counter_elapsed = 1;
            currentState = menuState;
        }

        public override void Dispose()
        {
            
        }
    }
}
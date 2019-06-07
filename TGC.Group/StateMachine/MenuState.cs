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
using TGC.Group.Model;

namespace TGC.Group.StateMachine
{
    class MenuState : State
    {
        List<string> folders = new List<string>();
        List<string> names = new List<string>();
        int selectedLevel = 0;

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
        }

        public override void render(float ElapsedTime)
        {
            int sx = Screen.PrimaryScreen.Bounds.Size.Width / 2;
            int sy = Screen.PrimaryScreen.Bounds.Size.Height / 2;

            for(int i=0; i<folders.Count; i++) {
                string folder = folders[i];
                string name = names[i];

                Color tColor = Color.White;
                if (i == selectedLevel)
                    tColor = Color.Yellow;                

                parent.DrawText.drawText(name, sx, sy, tColor);

                sy += 20;
            }
        }

        public override void update(float ElapsedTime)
        {
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
                parent.selectLevel(folders[selectedLevel]);
            }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class Core
    {
        static Dictionary<int, Scene> Scenes = new Dictionary<int, Scene>();

        public static bool CreateScene(int num)
        {
            if (Scenes.ContainsKey(num))
                return false;

            Scenes.Add(num, new Scene());

            return true;
        }

        public static Scene GetScene(int num)
        {
            return Scenes[num];
        }

    }
}

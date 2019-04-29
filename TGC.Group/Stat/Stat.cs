
namespace TGC.Group.Stats
{
    public class Stat
    {
        public float totalPoints { get; set; }
        public int totalMultiply { get; set; }

        public Stat (string playerName)
        {
            playerName = playerName;
            totalMultiply = 1;
        }

        

        public void addPoints(int pointsToAdd)
        {
            totalPoints += pointsToAdd * totalMultiply;
        }
    }
}

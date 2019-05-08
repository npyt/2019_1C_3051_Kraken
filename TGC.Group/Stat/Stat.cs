
namespace TGC.Group.Stats
{
    public class Stat
    {
        public string playerName;
        public float totalPoints { get; set; }
        public int partialMultiply { get; set; }
        public int totalMultiply { get; set; }

        public Stat (string player)
        {
            playerName = player;
            totalMultiply = 1;
        }
        
        public void addMultiply()
        {
            partialMultiply++;
            if (partialMultiply == 3)
            {
                totalMultiply++;
                partialMultiply = 0;
            }
        }

        public void duplicatePoints()
        {
            totalPoints = totalPoints * 2;
        }

        public void cancelMultiply()
        {
            partialMultiply = 0;
            totalMultiply = 1;
        }

        public void addPoints(int pointsToAdd)
        {
            totalPoints += pointsToAdd * totalMultiply;
            if (totalPoints < 0)
            {
                totalPoints = 0;
            }
        }
    }
}

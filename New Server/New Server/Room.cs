using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Server
{
    enum RoomStatus { playing,waiting };
    internal class Room
    {
        public Dictionary<int, Player> Watchers = new Dictionary<int, Player>();
        public List<string> doneFromWord = new List<string>();
        public static int WatchersCount { set; get; } = 0;
        public Dictionary<int, char> WordDict{ set;get; }
        public bool Join { set; get; } = true;
        public int NumberOfPlayers { set; get; }
        public Player Owner { set; get; }
        public Player Opponent { set; get; }
        public int OwnerScore { set; get; }
        public int OpponentScore { set; get; }
        public string Category { set; get; }
        public string Word { set; get; }
        public string PlayingNow;
        public string CurrentWord { set; get; }
        public static int Count  = 0;
        public int RoomId { set; get; }//uniqueKey
        public RoomStatus Status { set; get; }
        public string winner { set; get; }
        public string loser { set; get; }
        public Room(string cat)
        {
            
            WordDict = new Dictionary<int, char>();
            NumberOfPlayers = 1;
            Status = RoomStatus.waiting;
            RoomId = Count++;
            Category = cat;
            //Word = "dmm"; // need to change
            SelectedWord();
        }
        public override string ToString()
        {
            string Msg;
            if (Opponent != null)
            {
                Msg = RoomId.ToString() + ',' + NumberOfPlayers + ',' + Owner.Name + ',' + Opponent.Name + ',' + Status + ',' + Join.ToString() + ',' + Category + ',' + Word + ','+ Watchers.Count + ';';
            }
            else
            {
                Msg = RoomId.ToString() + ',' + NumberOfPlayers + ',' + Owner.Name + ',' + "" + ',' + Status + ',' + Join.ToString() + ',' + Category + ',' + Word + ',' + Watchers.Count +';';
            }
            return Msg;
        }

        public void SelectedWord()
        {
            string filePath = @".\file.txt";

            string[] lines = File.ReadAllLines(filePath);

            // Create a dictionary to store words by category
            Dictionary<string, List<string>> wordDictionary = new Dictionary<string, List<string>>();

            // Populate the dictionary
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string category = parts[0].Trim();
                    string word = parts[1].Trim();

                    if (!wordDictionary.ContainsKey(category))
                    {
                        wordDictionary[category] = new List<string>();
                    }

                    wordDictionary[category].Add(word);
                }
            }

            // Check if there are any words for the selected category
            if (wordDictionary.ContainsKey(Category))
            {
                List<string> wordsForCategory = wordDictionary[Category];

                // Select a random word from the list

                Word = wordsForCategory[new Random().Next(wordsForCategory.Count)];

            }

            char[] newWord = Word.ToCharArray();
            WordDict.Clear();
            for (int i = 0; i < newWord.Length; i++)
            {
                WordDict.Add(i, newWord[i]);
            }

        }

        public void Score()
        {
            StreamWriter file;
            file = File.AppendText(@".\score.txt");
            file.WriteLine($"{winner} win,{loser} lose");
            file.Close();
           
        }
    }
}

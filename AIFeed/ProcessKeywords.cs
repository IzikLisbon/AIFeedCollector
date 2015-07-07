using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIStoreCollection
{
    class ProcessKeywords
    {
        static Dictionary<Team, rssItem> RssPerTeam { get; set; }
        static Dictionary<Team, List<string>> teamKeywords =
            new Dictionary<Team, List<string>>()
            { 
                { Team.Scorpions, new List<string>() { "Windows", "PersistenceChannel" } }, 
                { Team.TroubleMakers, new List<string>() {"iOS", "Android", "Ruby"} },
                { Team.Web, new List<string>() {"Asp.Net"} },
            };

        private static void CountKeywordsPerTeam(rssItem item)
        {
            int scorpionsCount = CountKeywordOccurrences(item.description, Team.Scorpions);
            int webCount = CountKeywordOccurrences(item.description, Team.Scorpions);
        }

        private static int CountKeywordOccurrences(string description, Team team)
        {
            int keywordMatched = 0;
            List<string> keywords = teamKeywords[team];

            foreach (string keyword in keywords)
            {
                if (description.Contains(keyword))
                {
                    keywordMatched++;
                }
            }

            return keywordMatched;
        }
    }
}

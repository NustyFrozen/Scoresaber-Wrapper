/*MIT License

Copyright (c) 2018 Gal Meshulam

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ScoreSaberWrapper.API;

namespace ScoreSaberWrapper
{
    /// <summary>
    /// a class to sort the api calls
    /// </summary>
    public static class ScoresaberPath
    {
        public static string oldRootPath = @"https://scoresaber.com/";
        public static string RootPath = @"https://new.scoresaber.com/";
        public static string APIPath = @"api/";
        public static string badgesPath = @"static/badges/";
        public static string gamePath = @"game/";
        public static string exchangePath = "exchange.php";
        public static string fetchLeaderboardPath = "scores-pc.php?";
        public static string playerPath = "player/";
        public static string scoresPath = "scores/";
        public struct sorting
        {
            public static string topScores = "top/";
            public static string recentScores = "recent/";
        }
    }
    /// <summary>
    /// leaderboard class
    /// </summary>
    [Serializable]
    public class LeaderBoard
    {
        public string ranked;
        public string uid;
        public Playerscore[] scores;
        public int playerScore;
        /// <summary>
        /// check if a player is in the leaderboard or not
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool Contains(PlayerInfo player)
        {
            return Contains(player.playerId);
        }
        /// <summary>
        /// check if a player is in the leaderboard by playerID
        /// </summary>
        /// <param name="playerID">steam64 playerID</param>
        /// <returns></returns>
        public bool Contains(string playerID)
        {
            foreach (Playerscore x in scores)
                if (x.playerId == playerID)
                    return true;
            return false;
        }
    }
    /// <summary>
    /// playerinfo, use PlayerInfo.getPlayerInfo(scoresaberID) to get someone's info
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string playerName;
        public string avatarURL;
        public int rank;
        public int countryRank;
        public int pp;
        public string country;
        public string role;
        public Badge[] badges;
        public int permissions;
        public int inactive;
        public int banned;
        ScoreStats scoreStats;
        public List<Score> Scores;
        public static PlayerInfo getPlayerInfo(string scoresaberID)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ScoresaberPath.RootPath + ScoresaberPath.APIPath + ScoresaberPath.playerPath + scoresaberID + @"/full");
            request.Method = "GET";
            String test = String.Empty;
            request.UserAgent = "UnityPlayer/2019.3.2f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError) //scoresaber being shit as it is
                    return getPlayerInfo(scoresaberID);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInfo>(test.Substring(test.IndexOf('{'), test.LastIndexOf('}') + 1));
        }
        public void fetchScores()
        {
            bool inRange = true;
            for (int i = 0; inRange; i++)
                fetchScorePage(i, out inRange);
        }
        public void fetchScorePage(int page, out bool scoresRange)
        {
            if (playerId is null)
                throw new PlayerInfoException("the user wasn't assigned");
            if (Scores == null)
                Scores = new List<Score>();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ScoresaberPath.RootPath +
                ScoresaberPath.APIPath + ScoresaberPath.playerPath + playerId + @"/" + ScoresaberPath.scoresPath + ScoresaberPath.sorting.topScores);
            request.Method = "GET";
            String test = String.Empty;
            request.UserAgent = "UnityPlayer/2019.3.2f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
            if (test.Contains("This user has not set any scores!")) { scoresRange = false; return; } // out of scores
            Scores.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInfo>(test.Substring(test.IndexOf('{'), test.LastIndexOf('}') + 1)).Scores.ToArray());
            scoresRange = true;
        }
    }
    /// <summary>
    /// score class which is used in playerinfo class
    /// </summary>
    [Serializable]
    public class Score
    {
        public int rank;
        public long scoreID;
        public long score;
        public long unmodififiedScore;
        public string mods;
        public int pp;
        public int weight;
        public string timeSet;
        public int leaderboardId;
        public string songHash;
        public string songName;
        public string songSubName;
        public string songAuthorName;
        public string levelAuthorName;
        public Difficulty Difficulty;
        public string difficultyRaw;
        public long maxScore;
    }
    /// <summary>
    /// stats of a specific player used in playerinfo class
    /// </summary>
    [Serializable]
    public class ScoreStats
    {
        public long totalScore;
        public long totalRankedScore;
        public float averageRankedAccuracy;
        public int totalPlayCount;
        public int rankedPlayCount;
    }
    /// <summary>
    /// competition badges
    /// </summary>
    [Serializable]
    public class Badge
    {
        public string image;
        public string description;
    }
    [Serializable]
    public enum headset
    {
        cv1 = 1,
        riftS = 16,
        valveIndex = 64,
        quest = 32,
        unknown = 0,

    }
    /// <summary>
    /// playerscore format
    /// </summary>
    [Serializable]
    public class Playerscore
    {
        public string playerId;
        public string name;
        //public int rank;
        public string score;
        public string pp;
        public float weight;
        public string mods;
        public int badCuts;
        public int missedNotes;
        public int maxCombo;
        public int fullCombo;
        public headset hmd;
        bool replay;
    }
    /// <summary>
    /// Diff of a map sorted by scoresaber
    /// </summary>
    [Serializable]
    public enum Difficulty
    {
        expertPlus = 9,
        expert = 7,
        hard = 5,
        normal = 3,
        easy = 1
    }

    /// <summary>
    /// the main class where you want to do stuff :)
    /// </summary>
    public class API
    {
        private string playerID;
        private string sessionID;
        /// <summary>
        /// Creates an API Connection and resolve a session
        /// </summary>
        /// <param name="scoresaber">please type scoresaber profile or ID</param>
        public API(string scoresaber) // requires a scoresaber account to create a session
        {
            if (scoresaber.Contains("&"))
                throw new APIException("user link must not contains any formatting (example: &page=2&sort=2)");
            playerID = scoresaber.Replace("http://scoresaber.com/u/", "").Replace("https://scoresaber.com/u/", "").Replace("https://new.scoresaber.com/u/", "")
                .Replace("http://new.scoresaber.com/u/", "");
            createSession(playerID);
            long temp;
            /*if (long.TryParse(playerID, out temp))
                throw new UserFormattingException("invalid scoresaber ID");*/

        }
        /// <summary>
        /// check if the current session is expired
        /// </summary>
        /// <returns></returns>
        public bool isSessionExpired()
        {
            if (sessionID == "")
                return true;
            return false;
            //return sessionResolver().ranked.Contains("Session Expired!");
        }

        /// <summary>
        /// check if the player you used in the constructor passed the map
        /// </summary>
        /// <param name="levelID"> the levelID base64</param>
        /// <param name="difficulty"> difficulty of the map</param>
        /// <param name="gamemode"> gamemode</param>
        /// <returns></returns>
        public bool havePassed(string levelID, Difficulty difficulty, string gamemode = "SoloStandard")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://scoresaber.com/game/scores-n.php?levelId=" + levelID + "&difficulty=" + (int)difficulty + "&gameMode=" + gamemode + "&aroundPlayer=1");
                request.Method = "GET";
                String test = String.Empty;
                request.CookieContainer = new CookieContainer(2);
                request.CookieContainer.Add(new Cookie("PHPSESSID", sessionID, @"/game/scores-n.php", "scoresaber.com"));
                request.CookieContainer.Add(new Cookie("__cfduid", "d423d7f434520b26f71a45d0e6e457d001592667541", @"/game/scores-n.php", "scoresaber.com"));
                request.UserAgent = "UnityPlayer/2019.3.2f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    test = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
                LeaderBoard leaderboard = Newtonsoft.Json.JsonConvert.DeserializeObject<LeaderBoard>(test.Substring(test.IndexOf('{'), test.LastIndexOf('}') + 1));
                return leaderboard.scores[0].playerId != "123" && !(leaderboard.scores[0].mods.Contains("NA") ||
                    leaderboard.scores[0].mods.Contains("NF") || leaderboard.scores[0].mods.Contains("NO") ||
                    leaderboard.scores[0].mods.Contains("NF") || leaderboard.scores[0].mods.Contains("NW") || leaderboard.scores[0].mods.Contains("SS"));//idk why but thats what it does when you didn't pass the map
            }
            catch (Exception e)
            {
                //scoresaber is coded bad and sometimes get internal server error so why not handle it mid process
                if (e.Message.Contains("500")) //error code 500 is internal server error in http
                    return havePassed(levelID, difficulty, gamemode);
                else throw e;
            }
        }

        /// <summary>
        /// Fetching all the scores from the leaderboard and the leaderboard itself
        /// </summary>
        /// <param name="levelID">base64 mapHASH</param>
        /// <param name="difficulty">diff of the map</param>
        /// <param name="gamemode">gamemode</param>
        /// <returns></returns>
        public LeaderBoard getLeaderBoardALL(string levelID, Difficulty difficulty, string gamemode = "SoloStandard")
        {
            LeaderBoard temp;
            List<Playerscore> scorePatcher = new List<Playerscore>();
            for (int i = 1; true; i++)
            {
                temp = getLeaderBoard(levelID, difficulty, i, gamemode);
                if (temp.scores[0].playerId == "123") break; //out of pages
                scorePatcher.AddRange(temp.scores);

            }
            temp.scores = scorePatcher.ToArray();
            return temp;
        }
        /// <summary>
        /// Fetching the scores from the leaderboard by an index and the leaderboard itself
        /// </summary>
        /// <param name="levelID">base64 mapHASH</param>
        /// <param name="difficulty">diff of the map</param>
        /// <param name="page">from which page it should take the scores</param>
        /// <param name="gamemode">gamemode :)</param>
        /// <returns></returns>
        public LeaderBoard getLeaderBoard(string levelID, Difficulty difficulty, int page, string gamemode = "SoloStandard")
        {
            try
            {
                if (isSessionExpired())
                    throw new SessionExpired("Session Expired!");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ScoresaberPath.oldRootPath + ScoresaberPath.gamePath + ScoresaberPath.fetchLeaderboardPath + "levelId=" + levelID + "&difficulty=" + (int)difficulty + "&gameMode=" + gamemode + "&page=" + page);
                request.Method = "GET";
                String test = String.Empty;
                request.CookieContainer = new CookieContainer(2);
                request.CookieContainer.Add(new Cookie("PHPSESSID", sessionID, @"/game/scores-pc.php", "scoresaber.com"));
                request.CookieContainer.Add(new Cookie("__cfduid", "d423d7f434520b26f71a45d0e6e457d001592667541", @"/game/scores-pc.php", "scoresaber.com"));
                request.UserAgent = "ScoreSaber-PC/3.0.3.0";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    test = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LeaderBoard>(test.Substring(test.IndexOf('{'), test.LastIndexOf('}') + 1));
            }
            catch (Exception e)
            {
                //scoresaber is coded bad and sometimes get internal server error so why not handle it mid process
                if (e.Message.Contains("500")) //error code 500 is internal server error in http
                    return getLeaderBoard(levelID, difficulty, page, gamemode);
                else throw e;//throwing an other error if something else caused the error
            }
        }
        /// <summary>
        /// recreating a session by the existing playerID
        /// </summary>
        public void reconnectSession() => createSession(playerID);
        /// <summary>
        /// creating a session
        /// </summary>
        /// <param name="PlayerID">create a session by a specific playerID</param>
        private void createSession(string PlayerID)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ScoresaberPath.oldRootPath + ScoresaberPath.gamePath + ScoresaberPath.exchangePath);
                request.Method = "POST";
                String test = String.Empty;
                request.UserAgent = "UnityPlayer/2019.3.2f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
                request.ContentLength = ("playerid=" + PlayerID).Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (Stream postdata = request.GetRequestStream())
                {
                    postdata.Write(Encoding.ASCII.GetBytes("playerid=" + PlayerID), 0, ("playerid=" + PlayerID).Length);

                }
                var response = (HttpWebResponse)request.GetResponse();
                sessionID = new StreamReader(response.GetResponseStream()).ReadToEnd();
                sessionID = sessionID.Substring(sessionID.IndexOf('|'), sessionID.Length - sessionID.IndexOf('|')).Replace("|", "");
            }
            catch (Exception e)
            {
                //scoresaber is coded bad and sometimes get internal server error so why not handle it mid process
                if (e.Message.Contains("500")) //error code 500 is internal server error in http
                    createSession(PlayerID);
            }
        }

        #region Exceptions
        [Serializable]
        public class APIException : Exception
        {
            public APIException(string message)
                : base(message) { }
        }
        [Serializable]
        public class PlayerInfoException : Exception
        {
            public PlayerInfoException(string message)
                : base(message) { }
        }
        [Serializable]
        public class UserFormattingException : Exception
        {
            public UserFormattingException(string message)
                : base(message) { }
        }
        [Serializable]
        public class SessionExpired : Exception
        {
            public SessionExpired(string message)
                : base(message) { }
        }
        #endregion
    }
}
